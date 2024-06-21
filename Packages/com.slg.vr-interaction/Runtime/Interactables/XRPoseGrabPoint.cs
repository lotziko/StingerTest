using System;
using System.Collections.Generic;

namespace UnityEngine.VRInteraction
{
    public class XRPoseGrabPoint : XRGrabPoint
    {
        [SerializeField] private List<HandPose> m_Poses = new List<HandPose>();

        public List<HandPose> poses
        { get { return m_Poses; } }

#if UNITY_EDITOR
        [SerializeField] private int m_EditorSelectedPose;

        public int editorSelectedPose
        { get { return m_EditorSelectedPose; } set { m_EditorSelectedPose = value; } }

        [SerializeField] private PosableHand m_EditorLeftHandPreview;

        public PosableHand editorLeftHandPreview
        { get { return m_EditorLeftHandPreview; } set { m_EditorLeftHandPreview = value; } }

        [SerializeField] private PosableHand m_EditorRightHandPreview;

        public PosableHand editorRightHandPreview
        { get { return m_EditorRightHandPreview; } set { m_EditorRightHandPreview = value; } }
#endif

        public override bool CanAttachHand()
        {
            return true;
        }

        public override Vector3 GetAttachmentPosition(PosableHand posableHand)
        {
            HandPose pose = m_Poses[0];
            HandInfo handInfo = pose.GetInfo(posableHand.hand);
            return transform.TransformPoint(handInfo.AttachPosition);
        }

        public override Quaternion GetAttachmentRotation(PosableHand posableHand)
        {
            HandPose pose = m_Poses[0];
            HandInfo handInfo = pose.GetInfo(posableHand.hand);
            return transform.rotation * handInfo.AttachRotation;
        }

        public override void GetSkeletonData(Hand hand, HandSkeletonData destination)
        {
            if (m_Poses.Count == 0)
            {
                destination.isValid = false;
                return;
            }

            HandInfo handInfo = m_Poses[0].GetInfo(hand);
            destination.isValid = true;
            destination.position = transform.TransformPoint(handInfo.AttachPosition);
            destination.rotation = transform.rotation * handInfo.AttachRotation;
            Array.Copy(handInfo.JointPositions, destination.bonePositions, HandSkeletonData.BONE_COUNT);
            Array.Copy(handInfo.JointRotations, destination.boneRotations, HandSkeletonData.BONE_COUNT);
        }
    }
}
