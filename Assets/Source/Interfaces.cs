using UnityEngine;

namespace StingerTest
{
    public interface IObject
    {
        public Vector3 Position { get; }
        public void Destroy();
    }

    public interface IMissile : IObject
    {
    }

    public interface ITarget : IObject
    {
        public Vector3 Velocity { get; }
    }
}
