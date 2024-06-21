using System.Linq;
using UnityEngine;
using UnityEngine.VRInteraction;

namespace UnityEditor.VRInteraction
{
    public enum MirrorAxis
    {
        X, Y, Z
    }

    public static class PosableHandUtility
    {
        public static void ApplyPose(PosableHand posableHand, HandPose pose)
        {
            if (pose == null)
                Debug.LogError("Pose is null.");

            HandInfo info = pose.GetInfo(posableHand.hand);

            ApplyJointRotations(posableHand, info.JointRotations);

            Transform transform = posableHand.transform;
            transform.localPosition = info.AttachPosition;
            transform.localRotation = info.AttachRotation;
        }

        public static void ApplyJointRotations(PosableHand posableHand, Quaternion[] rotations)
        {
            Transform[] jointTransforms = posableHand.joints;
            
            if (rotations.Length == jointTransforms.Length)
                for (int i = 0; i < jointTransforms.Length; i++)
                    jointTransforms[i].localRotation = rotations[i];
        }

        public static void ApplyDefaultPose(PosableHand posableHand)
        {
            if (!PrefabUtility.IsPartOfAnyPrefab(posableHand.gameObject))
            {
                Debug.LogError("Hand is a not prefab.");
                return;
            }

            foreach (Transform transform in posableHand.GetComponentsInChildren<Transform>(true))
            {
                PrefabUtility.RevertObjectOverride(transform, InteractionMode.AutomatedAction);
            }
        }

        public static void ApplyPoseInfo(PosableHand posableHand, HandInfo handInfo)
        {
            handInfo.AttachPosition = posableHand.transform.localPosition;
            handInfo.AttachRotation = posableHand.transform.localRotation;
            handInfo.JointPositions = posableHand.joints.Select((t) => t.localPosition).ToArray();
            handInfo.JointRotations = posableHand.joints.Select((t) => t.localRotation).ToArray();
        }

        public static void MirrorToHand(PosableHand fromPosableHand, PosableHand toPosableHand, MirrorAxis axis = MirrorAxis.X)
        {
            Quaternion[] mirroredJoints = MirrorJoints(fromPosableHand.joints);
            ApplyJointRotations(toPosableHand, mirroredJoints);
            
            toPosableHand.transform.localPosition = MirrorPosition(fromPosableHand.transform, axis);
            toPosableHand.transform.localRotation = MirrorRotation(fromPosableHand.transform, axis);
        }

        private static Quaternion[] MirrorJoints(Transform[] joints)
        {
            Quaternion[] result = new Quaternion[joints.Length];

            for (int i = 0; i < joints.Length; i++)
            {
                result[i] = MirrorJointRotation(joints[i]);
            }

            return result;
        }

        private static Quaternion MirrorJointRotation(Transform joint)
        {
            Quaternion rotation = joint.localRotation;
            rotation.y *= -1;
            rotation.z *= -1;
            return rotation;
        }

        private static Vector3 MirrorPosition(Transform transform, MirrorAxis axis)
        {
            Vector3 position = transform.localPosition;
            switch (axis)
            {
                case MirrorAxis.X:
                    position.x *= -1f;
                    break;
                case MirrorAxis.Y:
                    position.y *= -1f;
                    break;
                case MirrorAxis.Z:
                    position.z *= -1f;
                    break;
            }
            return position;
        }

        private static Quaternion MirrorRotation(Transform otherTransform, MirrorAxis axis)
        {
            Transform center = otherTransform.parent;
            Vector3 direction = Vector3.zero;
            switch (axis)
            {
                case MirrorAxis.X:
                    direction = Vector3.right;
                    break;
                case MirrorAxis.Y:
                    direction = Vector3.up;
                    break;
                case MirrorAxis.Z:
                    direction = Vector3.forward;
                    break;
            }

            Vector3 forward = otherTransform.forward;
            Vector3 up = otherTransform.up;

            if (otherTransform.parent != null)
            {
                forward = otherTransform.parent.InverseTransformDirection(otherTransform.forward);
                up = otherTransform.parent.InverseTransformDirection(otherTransform.up);
            }

            Vector3 reflectedForward = Vector3.Reflect(forward, direction);
            Vector3 reflectedUp = Vector3.Reflect(up, direction);

            return Quaternion.LookRotation(reflectedForward, reflectedUp);
        }
    }
}
