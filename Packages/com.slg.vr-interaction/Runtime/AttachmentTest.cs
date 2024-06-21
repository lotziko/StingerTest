
namespace UnityEngine.VRInteraction
{
    public class AttachmentTest : MonoBehaviour
    {
        public Transform hand;
        //public Transform grabPoint;
        public PosableHand posableHand;
        public XRAnimatedGrabPoint grabPoint;
        //public PoseContainer poseContainer;

        [ContextMenu("Attach")]
        public void Attach()
        {
            hand.position = grabPoint.GetAttachmentPosition(posableHand);
            hand.rotation = grabPoint.GetAttachmentRotation(posableHand);
            //grabPoint.AttachHand(posableHand);
        }

        private void OnDrawGizmos()
        {
            //HandInfo handInfo = poseContainer.Poses[0].GetInfo(posableHand.Hand);
            //Vector3 grabPosition = grabPoint.position;
            //Quaternion grabRotation = grabPoint.rotation;

            //hand.position = grabPosition + (grabRotation * handInfo.AttachPosition);// + hand.rotation * -posableHand.transform.localPosition;// + (grabRotation * handInfo.AttachRotation * -posableHand.transform.localPosition);
            //hand.rotation = grabRotation * handInfo.AttachRotation;// * Quaternion.Inverse(posableHand.transform.localRotation);

            //HandInfo handInfo = poseContainer.Poses[0].GetInfo(posableHand.Hand);
            //Vector3 grabPosition = grabPoint.position;
            //Quaternion grabRotation = grabPoint.rotation;

            //Gizmos.DrawSphere(grabPosition + (grabRotation * handInfo.AttachPosition) - hand.rotation * posableHand.transform.localPosition, 0.01f);
            //Gizmos.DrawSphere(hand.TransformPoint(posableHand.transform.localPosition), 0.01f);
            //Gizmos.DrawLine(posableHand.transform.position, posableHand.transform.position);
        }
    }
}
