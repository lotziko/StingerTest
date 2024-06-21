using System;
using System.Collections.Generic;

namespace StingerTest
{
    public static class ObjectsManager
    {
        private static List<ITarget> _targets = new();
        private static List<IMissile> _missiles = new();

        public static IReadOnlyList<ITarget> Targets => _targets;
        public static IReadOnlyList<IMissile> Missiles => _missiles;

        public static event Action<IObject> OnUnregistered;

        public static void Register(IObject obj)
        {
            if (obj is ITarget target)
            {
                _targets.Add(target);
            }
            else if (obj is IMissile missile)
            {
                _missiles.Add(missile);
            }
        }

        public static void Unregister(IObject obj)
        {
            if (obj is ITarget target)
            {
                _targets.Remove(target);
            }
            else if (obj is IMissile missile)
            {
                _missiles.Remove(missile);
            }
            OnUnregistered?.Invoke(obj);
        }
    }
}
