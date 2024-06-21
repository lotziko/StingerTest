using System;
using System.Collections.Generic;
using UnityEngine.XR.Hands;

namespace UnityEngine.VRInteraction
{
    public class XRHandTrackingSkeleton : MonoBehaviour, IXRHandSkeletonSource
    {
        [SerializeField] private Hand m_Hand;
        [SerializeField] private Transform m_PhysicsHand;

        private readonly List<XRHandSubsystem> m_HandSubsystems = new List<XRHandSubsystem>();
        private XRHandSubsystem m_HandSubsystem;

        private Vector3[] m_Positions = new Vector3[HandSkeletonData.BONE_COUNT];
        private Quaternion[] m_Rotations = new Quaternion[HandSkeletonData.BONE_COUNT];

        private void Start()
        {
            if (RefreshSDK())
            {
                if (m_HandSubsystem != null)
                {
                    m_HandSubsystem.updatedHands += OnHandUpdate;
                }
            }
        }

        private void OnDestroy()
        {
            if (m_HandSubsystem != null)
            {
                m_HandSubsystem.updatedHands -= OnHandUpdate;
            }
        }

        private void OnHandUpdate(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            if (updateType == XRHandSubsystem.UpdateType.Dynamic)
            {
                if (updateSuccessFlags.HasFlag(XRHandSubsystem.UpdateSuccessFlags.All))
                {
                    XRHand hand = m_Hand == Hand.Left ? subsystem.leftHand : subsystem.rightHand;
                    Pose wristPose = Pose.identity;
                    UpdateJoint(hand.GetJoint(XRHandJointID.Wrist), ref wristPose);

                    for (int fingerIndex = (int)XRHandFingerID.Thumb; fingerIndex <= (int)XRHandFingerID.Little; ++fingerIndex)
                    {
                        Pose parentPose = wristPose;
                        XRHandFingerID fingerId = (XRHandFingerID)fingerIndex;

                        int jointIndexBack = fingerId.GetBackJointID().ToIndex();

                        for (int jointIndex = fingerId.GetFrontJointID().ToIndex(); jointIndex <= jointIndexBack; ++jointIndex)
                        {
                            UpdateJoint(hand.GetJoint(XRHandJointIDUtility.FromIndex(jointIndex)), ref parentPose);
                        }
                    }
                }
            }
        }

        private void UpdateJoint(XRHandJoint joint, ref Pose parentPose, bool updateParent = true)
        {
            if (!joint.TryGetPose(out Pose pose))
                return;

            int index = joint.id.ToIndex() - 2;
            if (index >= 0)
            {
                Quaternion inverseParentRotation = Quaternion.Inverse(parentPose.rotation);
                m_Positions[index] = inverseParentRotation * (pose.position - parentPose.position);
                m_Rotations[index] = inverseParentRotation * pose.rotation;
            }

            if (updateParent)
                parentPose = pose;
        }

        public void GetSkeletonData(Hand hand, HandSkeletonData destination)
        {
            if (m_HandSubsystem == null)
            {
                destination.isValid = false;
                return;
            }

            destination.isValid = true;
            destination.position = m_PhysicsHand.position;
            destination.rotation = m_PhysicsHand.rotation;

            Array.Copy(m_Positions, destination.bonePositions, HandSkeletonData.BONE_COUNT);
            Array.Copy(m_Rotations, destination.boneRotations, HandSkeletonData.BONE_COUNT);
        }
        
        private bool RefreshSDK()
        {
            RefreshHandSubsystems();

            if (m_HandSubsystems.Count > 0)
            {
                m_HandSubsystem = m_HandSubsystems[0];
                if (!m_HandSubsystem.running)
                    m_HandSubsystem.Start();
                return true;
            }
            else
            {
                m_HandSubsystem = null;
            }

            return false;
        }

        private void RefreshHandSubsystems()
        {
            SubsystemManager.GetInstances(m_HandSubsystems);
        }
    }
}
