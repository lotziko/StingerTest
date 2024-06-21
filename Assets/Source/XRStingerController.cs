using System.Collections;
using UnityEngine;
using UnityEngine.VRInteraction;
using UnityEngine.XR.Interaction.Toolkit;

namespace StingerTest
{
    public class XRStingerController : MonoBehaviour
    {
        [SerializeField] private XRGrabbableInteractable m_Grip;
        [SerializeField] private Transform _targetingAnchor;
        [SerializeField] private Transform _missileAnchor;

        [SerializeField] private StingerMissileController _missilePrefab;

        [SerializeField] private AudioSource _preparingLockSfx;
        [SerializeField] private AudioSource _lockedSfx;
        [SerializeField] private AudioSource _launchSfx;

        private Coroutine _launchCoroutine;
        private bool _gripActivated = false;

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
            m_Grip.activated.AddListener(OnGripActivated);
        }

        private void RemoveListeners()
        {
            m_Grip.activated.RemoveListener(OnGripActivated);
        }

        private void OnGripActivated(ActivateEventArgs args)
        {
            _gripActivated = true;
            if (_launchCoroutine == null)
            {
                _launchCoroutine = StartCoroutine(LaunchCoroutine());
            }
        }

        private IEnumerator LaunchCoroutine()
        {
            const float requiredDot = 0.95f;

            ITarget missileTarget = null;
            foreach (ITarget target in ObjectsManager.Targets)
            {
                Vector3 directionToTarget = (target.Position - _targetingAnchor.position).normalized;
                if (Vector3.Dot(directionToTarget, _targetingAnchor.forward) > requiredDot)
                {
                    missileTarget = target;
                }
            }

            if (missileTarget == null)
            {
                // No target
                _launchCoroutine = null;
                yield break;
            }

            // Preparing lock
            _preparingLockSfx.Play();
            for (float i = 0; i < 10; i += Time.deltaTime)
            {
                if (missileTarget == null || Vector3.Dot((missileTarget.Position - _targetingAnchor.position).normalized, _targetingAnchor.forward) < requiredDot)
                {
                    // Lock cancelled
                    _preparingLockSfx.Stop();
                    _launchCoroutine = null;
                    yield break;
                }
                yield return null;
            }
            _preparingLockSfx.Stop();

            // Waiting for launch
            _lockedSfx.Play();
            _gripActivated = false;
            while (!_gripActivated)
            {
                if (Vector3.Dot((missileTarget.Position - _targetingAnchor.position).normalized, _targetingAnchor.forward) < requiredDot)
                {
                    // Launch cancelled
                    _lockedSfx.Stop();
                    _launchCoroutine = null;
                    yield break;
                }
                yield return null;
            }
            _gripActivated = false;
            _lockedSfx.Stop();

            // Launch
            StingerMissileController missile = Instantiate(_missilePrefab, _missileAnchor.position, _missileAnchor.rotation);
            missile.SetTarget(missileTarget);
            _launchCoroutine = null;
            _launchSfx.Play();
        }
    }
}