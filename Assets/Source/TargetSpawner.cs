using System.Collections;
using UnityEngine;

namespace StingerTest
{
    public class TargetSpawner : MonoBehaviour
    {
        [SerializeField] private PlaneController _su25TargetPrefab;
        [SerializeField] private VehicleController _btr80TargetPrefab;

        private Coroutine _spawnTargetsCoroutine;

        private void Start()
        {
            SpawnTargets();
            ObjectsManager.OnUnregistered += OnUnregistered;
        }

        private void OnDestroy()
        {
            ObjectsManager.OnUnregistered -= OnUnregistered;
        }

        private void OnUnregistered(IObject obj)
        {
            if (ObjectsManager.Targets.Count == 0 && _spawnTargetsCoroutine == null)
            {
                _spawnTargetsCoroutine = StartCoroutine(DelayedSpawn(5f));
            }
        }

        private IEnumerator DelayedSpawn(float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnTargets();
        }

        private void SpawnTargets()
        {
            PlaneController plane = Instantiate(_su25TargetPrefab);
            plane.GenerateCirclePath();
            VehicleController vehicle = Instantiate(_btr80TargetPrefab);
            vehicle.GeneratePosition();
            _spawnTargetsCoroutine = null;
        }
    }
}
