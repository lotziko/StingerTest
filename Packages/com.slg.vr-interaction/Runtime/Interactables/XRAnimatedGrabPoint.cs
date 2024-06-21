
namespace UnityEngine.VRInteraction
{
    public class XRAnimatedGrabPoint : XRGrabPoint
    {
        [Tooltip("Specify which hand this grab point was animated for. Other hand will be mirrored from it.")]
        [SerializeField] private Hand m_ExpectedGrabHand;
        [SerializeField] private HandSkeleton m_LeftHandSkeleton;
        [SerializeField] private HandSkeleton m_RightHandSkeleton;

        private PosableHand m_PosableHand;
        private Coroutine m_HandCoroutine;

        public override bool CanAttachHand()
        {
            return m_PosableHand == null;
        }
        
        public override Vector3 GetAttachmentPosition(PosableHand posableHand)
        {
            return GetHandPosition(posableHand.hand);
        }
        
        public override Quaternion GetAttachmentRotation(PosableHand posableHand)
        {
            return GetHandRotation(posableHand.hand);
        }

        public override void GetSkeletonData(Hand hand, HandSkeletonData destination)
        {
            destination.isValid = true;
            destination.position = GetHandPosition(hand);
            destination.rotation = GetHandRotation(hand);
            HandSkeleton skeleton = m_ExpectedGrabHand == Hand.Left ? m_LeftHandSkeleton : m_RightHandSkeleton;
            skeleton.Copy(destination.bonePositions, destination.boneRotations, hand != m_ExpectedGrabHand);
        }

        private Vector3 GetHandPosition(Hand hand)
        {
            Vector3 localHandPosition = m_ExpectedGrabHand == Hand.Left ? m_LeftHandSkeleton.transform.localPosition : m_RightHandSkeleton.transform.localPosition;

            if (hand != m_ExpectedGrabHand)
                localHandPosition.x *= -1;

            return m_LeftHandSkeleton.transform.parent.TransformPoint(localHandPosition);
        }

        private Quaternion GetHandRotation(Hand hand)
        {
            Quaternion localHandRotation = m_ExpectedGrabHand == Hand.Left ? m_LeftHandSkeleton.transform.localRotation : m_RightHandSkeleton.transform.localRotation;

            if (hand != m_ExpectedGrabHand)
            {
                localHandRotation.y *= -1;
                localHandRotation.z *= -1;
            }

            return m_LeftHandSkeleton.transform.parent.rotation * localHandRotation;
        }
    }
}
