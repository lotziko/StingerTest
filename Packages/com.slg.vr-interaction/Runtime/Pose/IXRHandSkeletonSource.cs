
namespace UnityEngine.VRInteraction
{
    public interface IXRHandSkeletonSource
    {
        void GetSkeletonData(Hand hand, HandSkeletonData destination);
    }

    public class HandSkeletonData
    {
        public const int FINGER_COUNT = 5;
        public const int BONE_COUNT = 24;

        public bool isValid { get; set; }

        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }

        public Vector3[] bonePositions { get; set; } = new Vector3[BONE_COUNT];
        public Quaternion[] boneRotations { get; set; } = new Quaternion[BONE_COUNT];
    }
}
