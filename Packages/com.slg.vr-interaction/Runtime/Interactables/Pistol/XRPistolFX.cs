
using System.Collections;

namespace UnityEngine.VRInteraction
{
    [RequireComponent(typeof(XRPistolController))]
    public class XRPistolFX : MonoBehaviour
    {
        [SerializeField] private XRPistolController m_Controller;
        [SerializeField] private XRGunSliderInteractable m_Slider;

        [Header("Fx")]
        [SerializeField] private ParticleSystem m_FireParticles;
        [SerializeField] private GameObject m_BulletSmokeTrailParticlesPrefab;
        [SerializeField] private GameObject m_ImpactPrefab;

        [Header("Sfx")]
        [SerializeField] private AudioClip m_FireSound;
        [SerializeField] private AudioClip m_DryFireSound;
        [SerializeField] private AudioClip m_InsertClipSound;
        [SerializeField] private AudioClip m_DropClipSound;
        [SerializeField] private AudioClip m_SlideRackSound;

        [Header("Sfx options")]
        [SerializeField] private AnimationCurve m_SlideRackVolumeCurve;

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
            m_Controller.fired.AddListener(OnAnimationFire);
            m_Slider.slideRacked.AddListener(OnAnimationSlideRack);
        }

        private void RemoveListeners()
        {
            m_Controller.fired.RemoveListener(OnAnimationFire);
            m_Slider.slideRacked.RemoveListener(OnAnimationSlideRack);
        }

        private void OnAnimationFire(FireEventArgs args)
        {
            if (args.hasShot)
            {
                m_FireParticles.Play();
                AudioSource.PlayClipAtPoint(m_FireSound, transform.position);

                Vector3 start = args.ray.origin;
                Vector3 end = Vector3.zero;
                int particleCount = Random.Range(25, 30); // Ribbon seems to have some hidden max particle length, so limit it to 30
                float distance = Random.Range(1.5f, 4f);

                if (args.hasHit)
                {
                    RaycastHit hit = args.hit;
                    end = Vector3.MoveTowards(start, hit.point, distance);
                    GameObject fx = Instantiate(m_ImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    fx.transform.SetParent(hit.transform, true);
                }
                else
                {
                    end = start + args.ray.direction * distance;
                }

                ParticleSystem clone = Instantiate(m_BulletSmokeTrailParticlesPrefab, transform).GetComponent<ParticleSystem>();
                
                for (int i = 0; i < particleCount; i++)
                {
                    float t = (float)i / (particleCount - 1);
                    Vector3 position = Vector3.LerpUnclamped(start, end, t);
                    clone.Emit(new ParticleSystem.EmitParams() { position = position }, 1);
                }
                StartCoroutine(DestroyParticleSystemCoroutine(clone));
            }
            else
            {
                AudioSource.PlayClipAtPoint(m_DryFireSound, transform.position);
            }
        }

        private IEnumerator DestroyParticleSystemCoroutine(ParticleSystem particleSystem)
        {
            yield return new WaitWhile(() => particleSystem.isPlaying);
            Destroy(particleSystem.gameObject);
        }

        private void OnAnimationSlideRack(float amount)
        {
            AudioSource.PlayClipAtPoint(m_SlideRackSound, transform.position, m_SlideRackVolumeCurve.Evaluate(amount));
        }

        private void OnAnimationClipInserted()
        {
            AudioSource.PlayClipAtPoint(m_InsertClipSound, transform.position);
        }

        private void OnAnimationDropClipButton()
        {
            AudioSource.PlayClipAtPoint(m_DropClipSound, transform.position);
        }
    }
}
