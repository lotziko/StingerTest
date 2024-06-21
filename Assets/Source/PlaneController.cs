using System.Collections.Generic;
using UnityEngine;

namespace StingerTest
{
    public class PlaneController : MonoBehaviour, ITarget
    {
        [SerializeField] private float _movementSpeed = 138.889f; // 500km/h
        [SerializeField] private float _turnSpeed = 3f;

        private Transform _transform;
        private List<Vector3> _waypoints = new();
        private List<Vector3> _currentWaypoints = new();

        private Vector3 _velocity;

        public Vector3 Position => _transform.position;

        public Vector3 Velocity => _velocity;

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

        private void FixedUpdate()
        {
            if (_waypoints.Count == 0)
            {
                return;
            }

            if (_currentWaypoints.Count == 0)
            {
                _currentWaypoints.AddRange(_waypoints);
            }

            Vector3 targetWaypoint = _currentWaypoints[0];
            Vector3 direction = (targetWaypoint - _transform.position).normalized;
            _velocity = _movementSpeed * Time.fixedDeltaTime * direction;
            _transform.position += _velocity;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(_transform.position, targetWaypoint) < 100f)
            {
                _currentWaypoints.RemoveAt(0);
            }
        }

        public void GenerateCirclePath()
        {
            const int waypointsCount = 24;
            const float flyRadius = 1500;
            const float flyHeight = 250;

            float offset = Random.value * Mathf.PI * 2;
            for (int i = 0; i < 24; i++)
            {
                _waypoints.Add(new Vector3(Mathf.Cos(offset + (float)i / waypointsCount * Mathf.PI * 2) * flyRadius, flyHeight, Mathf.Sin(offset + (float)i / waypointsCount * Mathf.PI * 2) * flyRadius));
            }
            _transform.position = _waypoints[0];
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
