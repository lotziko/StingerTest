using System;
using System.Collections.Generic;

namespace UnityEngine.VRInteraction
{
    [Serializable]
    public class HandInfo
    {
        public Vector3 AttachPosition = Vector3.zero;
        public Quaternion AttachRotation = Quaternion.identity;
        public Vector3[] JointPositions = null;
        public Quaternion[] JointRotations = null;
    }
}
