using Unity.XR.CoreUtils;

namespace UnityEngine.VRInteraction
{
    public class XRJointHands : XRBaseHands
    {
        [Header("Rig options")]
        [SerializeField] private XROrigin _rig;
        [SerializeField] private Transform _rigBody;
        [SerializeField] private Transform _rigShoulder;
        [SerializeField] private ConfigurableJoint _controllerJoint;
        [SerializeField] private float _maxDistanceFromShoulder = 0.85f;
        
        private Vector3 m_PreviousControllerPosition;
        private Quaternion m_PreviousTargetRotation;
        private Vector3 m_PreviousCameraPosition;

        private void FixedUpdate()
        {
            UpdateJointAnchors();
            UpdateTargetVelocity();
            UpdateJointRotation();
        }

        private void UpdateJointAnchors()
        {
            Vector3 controllerLocalPosition = _rigBody.InverseTransformPoint(_handAnchor.position);
            if ((bool)_rigShoulder)
            {
                Vector3 shoulderLocalPosition = _rigBody.InverseTransformPoint(_rigShoulder.position);
                Vector3 localDelta = controllerLocalPosition - shoulderLocalPosition;
                localDelta = Vector3.ClampMagnitude(localDelta, _maxDistanceFromShoulder);
                Vector3 targetPosition = shoulderLocalPosition + localDelta;
                _controllerJoint.targetPosition = targetPosition;
            }
            else
            {
                if (Vector3.Distance(transform.position, _handAnchor.position) > _maxDistanceFromShoulder)
                {
                    transform.position = _handAnchor.position;
                }
                _controllerJoint.targetPosition = controllerLocalPosition;
            }
        }

        private Vector3 _previousCameraPosition;
        private Vector3 _previousControllerPosition;

        private void UpdateTargetVelocity()
        {
            Vector3 local = _rigBody.transform.InverseTransformPoint(_handAnchor.position);

            Vector3 tangentialV = Vector3.zero;
            Vector3 hmdV = Vector3.zero;

            Vector3 rigOffset = (_rig.CameraInOriginSpacePos - m_PreviousCameraPosition) / Time.fixedDeltaTime;
            rigOffset.y = 0f;
            m_PreviousCameraPosition = _rig.CameraInOriginSpacePos;

            Vector3 newPos = Quaternion.AngleAxis(0f, Vector3.up) * local;
            tangentialV = (newPos - local) / Time.fixedDeltaTime;
            hmdV = _rigBody.transform.InverseTransformDirection(rigOffset);

            Vector3 velocity = (local - m_PreviousControllerPosition) / Time.fixedDeltaTime;
            m_PreviousControllerPosition = local;
            _controllerJoint.targetVelocity = velocity + tangentialV + hmdV;

            Vector3 angularVelocity = AngularVelocity(_handAnchor.rotation, m_PreviousTargetRotation);
            _controllerJoint.targetAngularVelocity = Quaternion.Inverse(_rigBody.transform.rotation) * angularVelocity;

            m_PreviousTargetRotation = _handAnchor.rotation;

            //Vector3 rigOffset = (_rig.CameraInOriginSpacePos - _previousCameraPosition) / Time.fixedDeltaTime;
            //rigOffset.y = 0f;
            //_previousCameraPosition = _rig.CameraInOriginSpacePos;
            //Vector3 controllerLocalPosition = _rigBody.InverseTransformPoint(_handAnchor.position);
            //Vector3 controllerOffset = (controllerLocalPosition - _previousControllerPosition) / Time.fixedDeltaTime;
            //_previousControllerPosition = controllerLocalPosition;
            //_controllerJoint.targetVelocity = controllerOffset + rigOffset;
        }

        private Vector3 AngularVelocity(Quaternion current, Quaternion previous)
        {
            var deltaRotation = current * Quaternion.Inverse(previous);
            if (deltaRotation.w < 0)
            {
                deltaRotation.x = -deltaRotation.x;
                deltaRotation.y = -deltaRotation.y;
                deltaRotation.z = -deltaRotation.z;
                deltaRotation.w = -deltaRotation.w;
            }

            deltaRotation.ToAngleAxis(out var angle, out var axis);
            angle *= Mathf.Deg2Rad;
            return axis * (angle / Time.fixedDeltaTime);
        }

        private void UpdateJointRotation()
        {
            _controllerJoint.targetRotation = Quaternion.Inverse(_rigBody.rotation) * _handAnchor.rotation;
        }
    }
}
