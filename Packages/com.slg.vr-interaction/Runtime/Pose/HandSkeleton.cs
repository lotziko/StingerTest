
namespace UnityEngine.VRInteraction
{
    public class HandSkeleton : MonoBehaviour
    {
        [SerializeField] private Transform m_Wrist;
        [SerializeField] private Transform[] m_Joints = new Transform[HandSkeletonData.BONE_COUNT];

        public Transform wrist
        {
            get { return m_Wrist; }
        }

        public Transform[] joints
        {
            get { return m_Joints; }
        }

        public void Apply(HandSkeletonData data)
        {
            transform.position = data.position;
            transform.rotation = data.rotation;

            for (int i = 0; i < m_Joints.Length; i++)
                m_Joints[i].SetLocalPositionAndRotation(data.bonePositions[i], data.boneRotations[i]);
        }

        public void Apply(HandSkeletonData a, HandSkeletonData b, float t)
        {
            transform.position = Vector3.Lerp(a.position, b.position, t);
            transform.rotation = Quaternion.Lerp(a.rotation, b.rotation, t);

            for (int i = 0; i < m_Joints.Length; i++)
                m_Joints[i].SetLocalPositionAndRotation(Vector3.Lerp(a.bonePositions[i], b.bonePositions[i], t), Quaternion.Lerp(a.boneRotations[i], b.boneRotations[i], t));
        }

        //public void Apply(Quaternion[] localRotations)
        //{
        //    for(int i = 0; i < localRotations.Length; i++)
        //        m_Joints[i].localRotation = localRotations[i];
        //}

        public void Copy(Vector3[] positions, Quaternion[] rotations, bool mirror = false)
        {
            for (int i = 0; i < m_Joints.Length; i++)
            {
                Vector3 localPosition = m_Joints[i].localPosition;
                Quaternion localRotation = m_Joints[i].localRotation;

                if (mirror)
                {
                    localPosition.x *= -1;
                    localRotation.y *= -1;
                    localRotation.z *= -1;
                }

                positions[i] = localPosition;
                rotations[i] = localRotation;
            }
        }

        //public void Apply(HandSkeleton skeleton, bool mirror = false)
        //{
        //    // Wrist transform should not be changed at all to avoid breaking the skeleton root, move entire hand instead of it

        //    Transform[] skeletonJoints = skeleton.m_Joints;
        //    for (int i = 0; i < m_Joints.Length; i++)
        //    {
        //        Vector3 localPosition = skeletonJoints[i].localPosition;
        //        Quaternion localRotation = skeletonJoints[i].localRotation;

        //        if (mirror)
        //        {
        //            localPosition.x *= -1;
        //            localRotation.y *= -1;
        //            localRotation.z *= -1;
        //        }

        //        m_Joints[i].SetLocalPositionAndRotation(localPosition, localRotation);
        //    }
        //}

#if UNITY_EDITOR
        [ContextMenu("Find joints")]
        private void FindJoints()
        {
            if (m_Wrist == null)
                return;

            // Find roots
            Transform[] fingerRoots = new Transform[5];
            string[] fingerNames = new string[] { "thumb", "index", "middle", "ring", "pinky" };
            for (int i = 0; i < m_Wrist.childCount; i++)
            {
                Transform child = m_Wrist.GetChild(i);
                for (int j = 0; j < 5; j++)
                {
                    if (child.name.Contains(fingerNames[j]))
                    {
                        fingerRoots[j] = child;
                    }
                }
            }

            // Find joints
            int offset = 0;
            for (int i = 0; i < 5; i++)
            {
                Transform fingerRoot = fingerRoots[i];
                Transform[] bones = fingerRoot.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < bones.Length; j++)
                {
                    m_Joints[offset] = bones[j];
                    ++offset;
                }
            }
        }
#endif
    }
}
