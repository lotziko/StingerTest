using System.Collections;
using UnityEngine;

namespace StingerTest
{
    public class StingerMissileController : MonoBehaviour, IMissile
    {
        [SerializeField] private float _movementSpeed = 250f;
        [SerializeField] private float _rotateSpeed = 50f;

        private Transform _transform;

        private ITarget _target;

        public Vector3 Position => _transform.position;

        public void SetTarget(ITarget target)
        {
            _target = target;
        }

        private void Awake()
        {
            _transform = transform;
        }

        private void Start()
        {
            ObjectsManager.Register(this);
            StartCoroutine(FlyCoroutine());
        }

        private void OnDestroy()
        {
            ObjectsManager.Unregister(this);
        }

        private IEnumerator FlyCoroutine()
        {
            Vector3 startPosition = _transform.position;
            Vector3 engineStartPosition = startPosition + _transform.forward * 8f;
            for (float i = 0; i < 0.25f; i += Time.deltaTime)
            {
                _transform.position = Vector3.Lerp(startPosition, engineStartPosition, i / 0.25f);
                yield return null;
            }

            float acceleration = 0.25f;
            float missileSpeed;
            while (Vector3.Distance(_transform.position, _target.Position) > 10f)
            {
                acceleration = Mathf.Clamp01(acceleration + 0.125f * Time.deltaTime);
                missileSpeed = acceleration * _movementSpeed;
                _transform.position += missileSpeed * Time.deltaTime * _transform.forward;

                if (_target != null)
                {
                    Vector3 targetPosition = _target.Position;
                    Vector3 targetVelocity = _target.Velocity;

                    Vector3 missilePosition = _transform.position;

                    Vector3 interceptPoint = CalculateInterceptionPoint(targetPosition, targetVelocity, missilePosition, missileSpeed);

                    // Move the missile towards the intercept point
                    Vector3 direction = (interceptPoint - missilePosition).normalized;
                    _transform.position += missileSpeed * Time.deltaTime * direction;

                    // Calculate the new rotation towards the intercept point
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    // Smoothly rotate the missile towards the target rotation
                    _transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotateSpeed * Time.deltaTime);
                }
                yield return null;
            }

            Destroy();
            _target.Destroy();
        }

        Vector3 CalculateInterceptionPoint(Vector3 targetPosition, Vector3 targetVelocity, Vector3 missilePosition, float missileSpeed)
        {
            Vector3 relativePosition = targetPosition - missilePosition;
            float relativeSpeed = missileSpeed;

            float timeToIntercept = CalculateTimeToIntercept(relativePosition, targetVelocity, relativeSpeed);
            Vector3 interceptPoint = targetPosition + targetVelocity * timeToIntercept;

            return interceptPoint;
        }

        float CalculateTimeToIntercept(Vector3 relativePosition, Vector3 targetVelocity, float missileSpeed)
        {
            float a = Vector3.Dot(targetVelocity, targetVelocity) - missileSpeed * missileSpeed;
            float b = 2 * Vector3.Dot(targetVelocity, relativePosition);
            float c = Vector3.Dot(relativePosition, relativePosition);

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                // No real solution, no intercept possible
                return float.PositiveInfinity;
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);

            if (t1 > 0 && t2 > 0)
            {
                return Mathf.Min(t1, t2);
            }
            else if (t1 > 0)
            {
                return t1;
            }
            else
            {
                return t2;
            }
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
