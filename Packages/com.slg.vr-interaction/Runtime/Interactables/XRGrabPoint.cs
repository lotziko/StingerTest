
namespace UnityEngine.VRInteraction
{
    public abstract class XRGrabPoint : MonoBehaviour, IXRHandSkeletonSource
    {
        public abstract bool CanAttachHand();
        public abstract Vector3 GetAttachmentPosition(PosableHand posableHand);
        public abstract Quaternion GetAttachmentRotation(PosableHand posableHand);
        public abstract void GetSkeletonData(Hand hand, HandSkeletonData destination);
    }
}
