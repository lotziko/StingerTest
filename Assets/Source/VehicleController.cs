using UnityEngine;

namespace StingerTest
{
    public class VehicleController : MonoBehaviour, ITarget
    {
        public Vector3 Velocity => Vector3.zero;

        public Vector3 Position => _transform.position;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void Start()
        {
            ObjectsManager.Register(this);
        }

        private void OnDestroy()
        {
            ObjectsManager.Unregister(this);
        }

        public void GeneratePosition()
        {
            const float positionRadius = 150f;

            Vector2 position = Random.insideUnitCircle.normalized * positionRadius;
            if (Physics.Raycast(new Ray(new Vector3(position.x, 300f, position.y), Vector3.down), out RaycastHit hitInfo))
            {
                _transform.position = hitInfo.point;
            }
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
