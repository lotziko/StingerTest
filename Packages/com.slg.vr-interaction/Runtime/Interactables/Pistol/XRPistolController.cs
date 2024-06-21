using System.Collections;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public class FireEvent : UnityEvent<FireEventArgs>
    {
    }

    public class AmmoUpdateEvent : UnityEvent
    {
    }

    public class FireEventArgs
    {
        public bool hasShot { get; set; }
        public bool hasHit { get; set; }
        public Ray ray { get; set; }
        public RaycastHit hit { get; set; }
    }

    public class XRPistolController : MonoBehaviour
    {
        private static readonly int s_SlideAmountID = Animator.StringToHash("SlideAmount");
        private static readonly int s_TriggerAmountID = Animator.StringToHash("TriggerAmount");
        private static readonly int s_InsertAmmoID = Animator.StringToHash("InsertAmmo");
        private static readonly int s_DropAmmoID = Animator.StringToHash("DropAmmo");
        private static readonly int s_HasAmmoID = Animator.StringToHash("HasAmmo");
        private static readonly int s_IsSlidingID = Animator.StringToHash("IsSliding");
        private static readonly int s_FireID = Animator.StringToHash("Fire");

        [SerializeField] private XRGrabbableInteractable m_Grip;
        [SerializeField] private XRGunSliderInteractable m_Slider;
        [SerializeField] private XRGunAmmoSlotInteractor m_AmmoSlot;
        [SerializeField] private Animator m_Animator;

        [Header("Fire options")]
        [SerializeField] private float m_FireInterval = 0.135f;
        [SerializeField] private float m_FireDistance = 40f;
        [SerializeField] private float m_HitDamage = 10f;
        [SerializeField] private float m_HitForce = 5000f;
        [SerializeField] private LayerMask m_FireMask;
        [SerializeField] private Transform m_BulletSpawnOrigin;

        [Header("Events")]
        [SerializeField] private FireEvent m_Fired = new FireEvent();
        [SerializeField] private AmmoUpdateEvent m_AmmoUpdated = new AmmoUpdateEvent();

        public FireEvent fired
        {
            get { return m_Fired; }
            set { m_Fired = value; }
        }

        public AmmoUpdateEvent ammoUpdated
        {
            get { return m_AmmoUpdated; }
            set { m_AmmoUpdated = value; }
        }

        public int ammoCount
        {
            get { return m_AmmoSlot.ammoCount + (m_HasChamberedBullet ? 1 : 0); }
        }

        public int maxAmmoCount
        {
            get { return m_AmmoSlot.maxAmmoCount; }
        }

        private readonly LinkedPool<FireEventArgs> m_FireEventArgs = new LinkedPool<FireEventArgs>(() => new FireEventArgs(), collectionCheck: false);

        private int m_BaseLayerIndex;
        private int m_SliderLayerIndex;

        private Coroutine m_GrabbingCoroutine;

        private bool m_HasChamberedBullet = false;
        private bool m_IsClipAnimationInProgress = false;
        private float m_FireTimer = 0f;

        private void Awake()
        {
            m_BaseLayerIndex = m_Animator.GetLayerIndex("Base");
            m_SliderLayerIndex = m_Animator.GetLayerIndex("Slider");
        }

        private void Start()
        {
            AddListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            m_Grip.selectEntered.AddListener(OnGripSelectEntered);
            m_Grip.selectExited.AddListener(OnGripSelectExited);
            m_Grip.activated.AddListener(OnGripActivated);
            m_Grip.buttonPressed.AddListener(OnGripButtonPressed);
            m_Slider.slided.AddListener(OnSlide);
            m_Slider.selectEntered.AddListener(OnSlideStarted);
            m_Slider.selectExited.AddListener(OnSlideFinished);
            m_Slider.fullSlided.AddListener(OnFullSlided);
            //m_Slider.ejected.AddListener(OnEject);
            m_AmmoSlot.selectEntered.AddListener(OnAmmoInsert);
        }

        private void RemoveListeners()
        {
            m_Grip.selectEntered.RemoveListener(OnGripSelectEntered);
            m_Grip.selectExited.RemoveListener(OnGripSelectExited);
            m_Grip.activated.RemoveListener(OnGripActivated);
            m_Grip.buttonPressed.RemoveListener(OnGripButtonPressed);
            m_Slider.slided.RemoveListener(OnSlide);
            m_Slider.selectEntered.RemoveListener(OnSlideStarted);
            m_Slider.selectExited.RemoveListener(OnSlideFinished);
            m_Slider.fullSlided.RemoveListener(OnFullSlided);
            //m_Slider.ejected.RemoveListener(OnEject);
            m_AmmoSlot.selectEntered.RemoveListener(OnAmmoInsert);
        }

        private IEnumerator GrabbingCoroutine()
        {
            WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
            IXRActivateValueInteractor interactor = m_Grip.firstInteractorSelecting as IXRActivateValueInteractor;

            while (true)
            {
                // Update animator
                m_Animator.SetFloat(s_TriggerAmountID, interactor?.value ?? 0f);
                // Update fire timer
                m_FireTimer += Time.fixedDeltaTime;

                yield return waitForFixedUpdate;
            }
        }

        private void OnGripSelectEntered(SelectEnterEventArgs args)
        {
            if (m_GrabbingCoroutine == null)
            {
                m_GrabbingCoroutine = StartCoroutine(GrabbingCoroutine());
            }
        }

        private void OnGripSelectExited(SelectExitEventArgs args)
        {
            if (m_GrabbingCoroutine != null)
            {
                StopCoroutine(m_GrabbingCoroutine);
                m_GrabbingCoroutine = null;
            }
        }

        private void OnGripActivated(ActivateEventArgs args)
        {
            if (CanFire())
            {
                Fire();
                ResetFireTimer();
            }
            else
            {
                DryFire();
            }
        }

        private void OnGripButtonPressed(ButtonPressEventArgs args)
        {
            if (args.button == XRCommonButton.Primary)
            {
                if (CanDropClip())
                {
                    DropClip();
                }
            }
        }

        private void OnSlideStarted(SelectEnterEventArgs args)
        {
            m_Animator.SetBool(s_IsSlidingID, true);
        }

        private void OnSlideFinished(SelectExitEventArgs args)
        {
            m_Animator.SetBool(s_IsSlidingID, false);
        }

        private void OnSlide(float amount)
        {
            m_Animator.SetFloat(s_SlideAmountID, amount);
        }

        private void OnFullSlided()
        {
            bool isLocked = m_Slider.isLocked;
            if (m_AmmoSlot.TryRemoveAmmo())
            {
                m_Slider.isLocked = false;
                m_HasChamberedBullet = true;
            }
            else
            {
                m_Slider.isLocked = !m_Slider.isLocked;
            }
            m_AmmoUpdated?.Invoke();
        }

        private void OnEject()
        {
            // Spawn casing
        }

        private void OnAmmoInsert(SelectEnterEventArgs args)
        {
            m_Animator.SetTrigger(s_InsertAmmoID);
            m_IsClipAnimationInProgress = true;
        }

        private void OnAnimationClipInserted()
        {
            m_IsClipAnimationInProgress = false;
            m_AmmoUpdated?.Invoke();
        }

        private void OnAnimationClipDropped()
        {
            m_IsClipAnimationInProgress = false;
            m_AmmoSlot.DropAmmo();
            m_AmmoUpdated?.Invoke();
        }

        private void Fire()
        {
            bool hasMoreAmmo = m_AmmoSlot.HasAmmo();
            m_Animator.SetBool(s_HasAmmoID, hasMoreAmmo);
            m_Animator.SetTrigger(s_FireID);
            if (hasMoreAmmo)
            {
                m_AmmoSlot.TryRemoveAmmo();
            }
            else
            {
                m_HasChamberedBullet = false;
                m_Slider.isLocked = true;
            }
            m_AmmoUpdated?.Invoke();

            using (m_FireEventArgs.Get(out FireEventArgs args))
            {
                args.hasShot = true;
                args.hasHit = false;

                Ray fireRay = new Ray(m_BulletSpawnOrigin.position, m_BulletSpawnOrigin.forward);
                args.ray = fireRay;

                if (Physics.Raycast(fireRay, out RaycastHit hit, m_FireDistance, m_FireMask, QueryTriggerInteraction.Ignore))
                {
                    // Apply force to hit object
                    Rigidbody hitRigidbody = hit.collider.attachedRigidbody;
                    if (hitRigidbody)
                        hitRigidbody.AddForceAtPosition(-hit.normal * m_HitForce, hit.point);

                    // Apply damage to hit object
                    IDamageable damageable = (hitRigidbody != null ? hitRigidbody.GetComponent<IDamageable>() : hit.collider.GetComponentInParent<IDamageable>());
                    if (damageable != null)
                        damageable.Damage(m_HitDamage);

                    args.hasHit = true;
                    args.hit = hit;
                }

                m_Fired.Invoke(args);
            }
        }

        private void DryFire()
        {
            using (m_FireEventArgs.Get(out FireEventArgs args))
            {
                args.hasShot = false;
                m_Fired?.Invoke(args);
            }
        }

        private void DropClip()
        {
            m_Animator.SetTrigger(s_DropAmmoID);
            m_IsClipAnimationInProgress = true;
        }

        private void ResetFireTimer()
        {
            m_FireTimer = 0f;
        }

        private bool CanFire()
        {
            return m_HasChamberedBullet && Mathf.Approximately(m_Slider.value, 0f) && m_FireTimer >= m_FireInterval;
        }

        private bool CanDropClip()
        {
            return !m_IsClipAnimationInProgress && m_AmmoSlot.hasSelection;
        }
    }
}