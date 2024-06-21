
namespace UnityEngine.VRInteraction
{
    public class XRRigidbodyHands : XRBaseHands
    {
        [SerializeField] private ConfigurableJoint _controllerJoint;

        private Vector3 m_PreviousControllerPosition;
        private Quaternion m_PreviousTargetRotation;
        //[SerializeField] private float m_MovementSpeed = 5000f;
        //[SerializeField] private float m_RotationSpeed = 5000f;
        //[SerializeField] private float m_Damping = 0.5f;

        private void FixedUpdate()
        {
            UpdateAnchor();
            //UpdateTargetVelocity();
        }

        private void UpdateAnchor()
        {
            //_controllerJoint.targetPosition = _handAnchor.position;
            _controllerJoint.targetRotation = _handAnchor.rotation;

            Vector3 targetPosition = _handAnchor.position;
            Vector3 velocity = (targetPosition - m_PreviousControllerPosition) / Time.fixedDeltaTime;
            m_PreviousControllerPosition = targetPosition;

            Vector3 newPos = Quaternion.AngleAxis(0f, Vector3.up) * targetPosition;
            Vector3 tangentialV = (newPos - targetPosition) / Time.fixedDeltaTime;

            _controllerJoint.targetVelocity = velocity + tangentialV;

            //Vector3 angularVelocity = AngularVelocity(_handAnchor.rotation, m_PreviousTargetRotation);
            //_controllerJoint.targetAngularVelocity = angularVelocity;
            //if (_controllerJoint.autoConfigureConnectedAnchor)
            //    _controllerJoint.autoConfigureConnectedAnchor = false;
            //if (_controllerJoint.targetPosition != Vector3.zero)
            //    _controllerJoint.targetPosition = Vector3.zero;
            //_controllerJoint.connectedAnchor = _handAnchor.position;

            //Vector3 xAxis = _controllerJoint.axis;
            //Vector3 zAxis = Vector3.Cross(_controllerJoint.axis, _controllerJoint.secondaryAxis).normalized;
            //Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;
            //Quaternion lookRotation = Quaternion.LookRotation(zAxis, yAxis);
            //_controllerJoint.targetRotation = Quaternion.Inverse(lookRotation) * Quaternion.Inverse(_handAnchor.rotation) * lookRotation;
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

        //private void UpdateTargetVelocity()
        //{
        //    Vector3 targetVelocity = Vector3.ClampMagnitude(_handAnchor.position - _rigidbody.position, 1f) * m_MovementSpeed * Time.fixedDeltaTime;
        //    Vector3 velocityDamping = Vector3.zero;//-_rigidbody.velocity * m_Damping;
        //    _rigidbody.AddForce(targetVelocity + velocityDamping, ForceMode.Force);

        //    _rigidbody.rotation = _handAnchor.rotation;
        //    //Vector3 targetAngluarVelocity = Vector3.Cross(transform.forward, _handAnchor.forward) * m_RotationSpeed * Time.fixedDeltaTime;
        //    //Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(_handAnchor.rotation);
        //    //rotationDelta.ToAngleAxis(out float rotationAngle, out Vector3 rotationAxis);
        //    //Vector3 targetAngluarVelocity = rotationAxis * rotationAngle * Mathf.Deg2Rad * m_RotationSpeed * Time.fixedDeltaTime;
        //    //Vector3 angularVelocityDamping = -_rigidbody.angularVelocity * m_Damping;
        //    //_rigidbody.angularVelocity = targetAngluarVelocity + angularVelocityDamping;
        //}
    }
}