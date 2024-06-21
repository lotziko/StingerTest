using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StingerTest
{
    public class MapController : MonoBehaviour
    {
        const float SCALE = 1 / 4000f;

        [SerializeField] private Image _targetIconPrefab;
        [SerializeField] private Image _missileIconPrefab;
        
        private Dictionary<ITarget, Image> _targetIcons = new();
        private Dictionary<IMissile, Image> _missileIcons = new();

        private void Start()
        {
            ObjectsManager.OnUnregistered += OnUnregistered;
        }

        private void OnDestroy()
        {
            ObjectsManager.OnUnregistered -= OnUnregistered;
        }

        private void OnUnregistered(IObject obj)
        {
            if (obj is ITarget target)
            {
                if (_targetIcons.TryGetValue(target, out Image icon))
                {
                    Destroy(icon);
                }
                _targetIcons.Remove(target);
            }
            else if (obj is IMissile missile)
            {
                if (_missileIcons.TryGetValue(missile, out Image icon))
                {
                    Destroy(icon);
                }
                _missileIcons.Remove(missile);
            }
        }

        private void Update()
        {
            foreach (ITarget target in ObjectsManager.Targets)
            {
                if (!_targetIcons.TryGetValue(target, out Image icon))
                {
                    icon = Instantiate(_targetIconPrefab, transform);
                    _targetIcons[target] = icon;
                }
                icon.rectTransform.anchoredPosition = new Vector2(target.Position.x * SCALE, target.Position.z * SCALE);
            }

            foreach (IMissile missile in ObjectsManager.Missiles)
            {
                if (!_missileIcons.TryGetValue(missile, out Image icon))
                {
                    icon = Instantiate(_missileIconPrefab, transform);
                    _missileIcons[missile] = icon;
                }
                icon.rectTransform.anchoredPosition = new Vector2(missile.Position.x * SCALE, missile.Position.z * SCALE);
            }
        }
    }
}
