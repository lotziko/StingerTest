using System.Collections;
using Unity.XR.CoreUtils;

namespace UnityEngine.VRInteraction
{
    [DefaultExecutionOrder(6)]
    public class XRPlayerControllerV2 : MonoBehaviour
    {
        private enum State
        {
            Default = 1, PreparingJump = 2, Jumping = 3
        }

        private enum Crouch
        {
            Standing, HighSquat, Squat, LowSquat, Count
        }

        [SerializeField] private XROrigin _rig;

        [Header("Options")]
        [SerializeField] private float _walkingSpeed = 4f;
        [SerializeField] private float _runningSpeed = 6f;
        [SerializeField] private float _airAcceleration = 5f;
        [SerializeField] private float _snapTurnAmount = 45f;
        [SerializeField] private float _jumpForce = 20000f;

        [SerializeField] private float _playerHeight = 1.89f;
        [SerializeField] private float _bodyHeight = 0.64f;
        [SerializeField] private float _originalLegsHeight = 0.95f;
        [SerializeField] private float _chestOffset = 0.25f;

        [Header("Ground options")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckDistance = 0.08f;
        [SerializeField] private float _maxStandingSlope = 45f;
        [SerializeField] private AnimationCurve _groundSlopeSpeedMultiplier;
        [SerializeField] private AnimationCurve _croachingSpeedMultiplier;

        [Header("Rigidbodies")]
        [SerializeField] private Rigidbody _head;
        [SerializeField] private Rigidbody _body;
        [SerializeField] private Rigidbody _legs;
        [SerializeField] private Rigidbody _wheel;

        [Header("Transforms")]
        [SerializeField] private Transform _neckPivot;

        [Header("Colliders")]
        [SerializeField] private SphereCollider _headSphere;
        [SerializeField] private SphereCollider _wheelSphere;
        [SerializeField] private SphereCollider _fenderSphere;
        [SerializeField] private CapsuleCollider _kneeCapsule;
        [SerializeField] private CapsuleCollider _chestCapsule;
        [SerializeField] private CapsuleCollider _pelvisCapsule;

        [Header("Joints")]
        [SerializeField] private ConfigurableJoint _headBodyJoint;
        [SerializeField] private ConfigurableJoint _legsBodyJoint;
        [SerializeField] private ConfigurableJoint _legsWheelJoint;

        [Header("Input")]
        [SerializeField] private XRPlayerInput _playerInput;

        private State _state = State.Default;
        private Crouch _crouch = Crouch.Standing;
        private Vector3 _rigPreviousPosition;
        private float _jumpOffset;
        private float _crouchOffset;
        private float _crouchTargetOffset;

        private float _originalWheelRadius;
        private float _originalFenderRadius;
        private float _currentSlope;
        private Vector3 _groundNormal;
        private float _realCrouchAmount;
        private float _fakeCrouchAmount;
        private float _legHeight;

        private bool _isGrounded;
        private bool _isRunning;

        private IXRPlayerInput Input => _playerInput;
        private float BodyMass => _head.mass + _body.mass + _legs.mass + _wheel.mass;

        private void Awake()
        {
            _originalWheelRadius = _wheelSphere.radius;
            _originalFenderRadius = _fenderSphere.radius;
        }

        private void Start()
        {
            Input.OnSnapTurn.AddListener(OnTurn);
            Input.OnRun.AddListener(OnRun);
            Input.OnJumpPrepare.AddListener(OnJumpPrepare);
            Input.OnJump.AddListener(OnJump);
            Input.OnCrouchTap.AddListener(OnCrouchTap);
        }

        private void OnDestroy()
        {
            Input.OnSnapTurn.RemoveListener(OnTurn);
            Input.OnRun.RemoveListener(OnRun);
            Input.OnJumpPrepare.RemoveListener(OnJumpPrepare);
            Input.OnJump.RemoveListener(OnJump);
            Input.OnCrouchTap.RemoveListener(OnCrouchTap);
        }

        private void Update()
        {
            CheckIfGrounded();
            UpdateCrouch();
            UpdateLegs();
            Move();
            UpdateHead();
            UpdateBody();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy)
                return;
            if (!_pelvisCapsule || !_headSphere || !_wheelSphere || !_pelvisCapsule)
            {
                Debug.LogWarning("Player has missing parts.");
                return;
            }

            _bodyHeight = Vector3.Distance(_headSphere.transform.position, _pelvisCapsule.transform.position);
            _playerHeight = Vector3.Distance(_wheelSphere.transform.position, _headSphere.transform.position) + _wheelSphere.radius + _headSphere.radius;
            _originalLegsHeight = (float)System.Math.Round(_playerHeight - _bodyHeight - _headSphere.radius - _wheelSphere.radius, 2);
            _chestOffset = Vector3.Distance(_pelvisCapsule.transform.position, _chestCapsule.transform.position);
        }
#endif

        private void OnTurn(int sign)
        {
            Quaternion rotation = Quaternion.Euler(0, sign * 45f, 0);

            _head.transform.rotation *= rotation;
            _body.transform.rotation *= rotation;
            _legs.transform.rotation *= rotation;
            _wheel.transform.rotation *= rotation;
        }

        private void OnRun()
        {
            _isRunning = true;
        }

        private void OnJumpPrepare()
        {
            if (_state == State.Default)
            {
                _state = State.PreparingJump;
                StartCoroutine(JumpCoroutine());
            }
        }

        private void OnJump()
        {
            if (_state == State.PreparingJump)
            {
                _state = State.Jumping;
            }
        }

        private void OnCrouchTap()
        {
            SetCrouchLevel(_crouch + Input.CrouchValue);
        }

        private void CheckIfGrounded()
        {
            _isGrounded = Physics.SphereCast(_wheel.position + Vector3.up * Physics.defaultContactOffset, _wheelSphere.radius - Physics.defaultContactOffset, Vector3.down, out var hitInfo, _groundCheckDistance, _groundLayer, QueryTriggerInteraction.Ignore);
            if (_isGrounded)
            {
                _currentSlope = Vector3.Angle(Vector3.up, hitInfo.normal);
                _groundNormal = hitInfo.normal;
            }
        }

        private void Move()
        {
            Vector2 axis = Input.MovementAxis;
            Vector3 direction = Vector3.Cross(Vector3.up, _rig.Camera.transform.forward * axis.y + _rig.Camera.transform.right * axis.x).normalized;
            bool isMoving = (Mathf.Abs(axis.x) > 0.1f || Mathf.Abs(axis.y) > 0.1f);
            _wheel.freezeRotation = false;

            if (!_isGrounded)
            {
                //air movement with axis
                Vector3 velocity = (_rig.Camera.transform.forward * axis.y + _rig.Camera.transform.right * axis.x) * _airAcceleration;
                _head.AddForce(velocity);
                _body.AddForce(velocity);
                _legs.AddForce(velocity);
                _wheel.AddForce(velocity);
            }

            if (Input.MovementAxis.magnitude < 0.01f)
                _isRunning = false;

            _wheel.angularVelocity *= 0.1f;
            _wheel.AddTorque(direction * axis.magnitude * (_isRunning ? _runningSpeed : _walkingSpeed) * BodyMass * _groundSlopeSpeedMultiplier.Evaluate(_currentSlope) * _croachingSpeedMultiplier.Evaluate(_legHeight / _originalLegsHeight) * (_state == State.Jumping ? 0.5f : 1f));

            if (_isGrounded && !isMoving && _currentSlope < _maxStandingSlope)
            {
                if (_wheel.angularVelocity.magnitude < 1f)
                {
                    _wheel.angularVelocity = Vector3.zero;
                    _wheel.freezeRotation = true;
                }
            }
            UpdateHMDMovement();
        }

        private IEnumerator JumpCoroutine()
        {
            JointDrive drive = _legsBodyJoint.yDrive;
            float legsMass = _legs.mass;
            float wheelMass = _wheel.mass;

            while (_state == State.PreparingJump)
            {
                _jumpOffset = Mathf.Clamp(_jumpOffset + Time.deltaTime * 5f, 0f, 0.5f);
                yield return null;
            }

            _legs.mass = 5f;
            _wheel.mass = 5f;

            _legsBodyJoint.yDrive = new UnityEngine.JointDrive()
            {
                positionSpring = _jumpForce,
                positionDamper = 2f * Mathf.Sqrt(_jumpForce * _wheel.mass),
                maximumForce = _jumpForce
            };

            if (_state == State.Jumping)
            {
                _jumpOffset = 0f;
                yield return new WaitForSeconds(0.5f);
            }

            ScaleBalls(false);

            _legsBodyJoint.yDrive = drive;
            _legs.mass = legsMass;
            _wheel.mass = wheelMass;
            _state = State.Default;
        }

        private void CalculateLegHeight(float fakeCrouch)
        {
            _realCrouchAmount = Mathf.Clamp((float)System.Math.Round(_playerHeight - _rig.CameraInOriginSpaceHeight - _headSphere.radius, 2), 0f, _originalLegsHeight);
            _fakeCrouchAmount = Mathf.Clamp(fakeCrouch, 0f, _originalLegsHeight);
            _legHeight = Mathf.Clamp(_originalLegsHeight - _realCrouchAmount - _fakeCrouchAmount, 0f, _originalLegsHeight);
        }

        private void SetCrouchLevel(Crouch crouch)
        {
            _crouch = (Crouch)Mathf.Clamp((int)crouch, 0, (int)Crouch.Count - 1);
            _crouchTargetOffset = GetCrouchOffset(_crouch);
        }

        private void UpdateHead()
        {
            _headSphere.transform.rotation = _rig.Camera.transform.rotation;

            Vector3 dir = _rig.Camera.transform.position - _body.transform.position;
            dir.Normalize();
            Vector3 anchor = _body.transform.TransformDirection(new Vector3(0, _bodyHeight - _realCrouchAmount, -_neckPivot.localPosition.z));
            anchor = Quaternion.FromToRotation(anchor, dir) * anchor;
            _headBodyJoint.connectedAnchor = _body.transform.InverseTransformDirection(anchor);

            //_headBodyJoint.connectedAnchor = _pelvisCapsule.transform.InverseTransformDirection(new Vector3(0, _bodyHeight - _realCrouchAmount, 0));
        }

        private void UpdateCrouch()
        {
            _crouchOffset = Mathf.MoveTowards(_crouchOffset, _crouchTargetOffset, 2f * Time.deltaTime);
            if (Input.IsCrouchHold)
            {
                if (Mathf.Approximately(_crouchOffset, _crouchTargetOffset))
                {
                    SetCrouchLevel(_crouch + Input.CrouchValue);
                }
            }
        }

        private void UpdateLegs()
        {
            CalculateLegHeight(_jumpOffset + _crouchOffset);

            _legsBodyJoint.anchor = Vector3.down * (_originalLegsHeight / 2f);
            _legsBodyJoint.targetPosition = Vector3.up * (_originalLegsHeight - _legHeight - _realCrouchAmount);
        }

        //is used to reposition body colliders
        private void UpdateBody()
        {
            Quaternion headRotation = _headSphere.transform.rotation;
            headRotation.x = headRotation.z = 0;
            headRotation.Normalize();

            Vector3 pelvisPosition = _pelvisCapsule.transform.position;

            _pelvisCapsule.transform.localPosition = Vector3.down * (_realCrouchAmount);
            _pelvisCapsule.transform.rotation = headRotation;
            _chestCapsule.transform.localPosition = _pelvisCapsule.transform.localPosition + Vector3.up * _chestOffset;
            _chestCapsule.transform.rotation = headRotation;

            _kneeCapsule.height = Vector3.Distance(pelvisPosition, _wheelSphere.transform.position);
            _kneeCapsule.transform.position = _wheelSphere.transform.position + Vector3.up * _kneeCapsule.height / 2f;
        }

        private void UpdateHMDRotation()
        {
            Vector3 cameraDirection = _rig.Camera.transform.forward;
            cameraDirection.y = 0;
            cameraDirection.Normalize();

            Quaternion rotation = Quaternion.FromToRotation(_body.transform.forward, cameraDirection);

            _body.transform.localRotation *= rotation;
            _rig.transform.localRotation *= Quaternion.Inverse(rotation);
        }

        private void ScaleBalls(bool shrink)
        {
            if (shrink)
            {
                _wheelSphere.radius = _kneeCapsule.radius;
                _fenderSphere.radius = _kneeCapsule.radius + 0.01f;
            }
            else
            {
                _wheelSphere.radius = _originalWheelRadius;
                _fenderSphere.radius = _originalFenderRadius;
            }
        }

        //is used to move rigidbodies during HMD movement
        private void UpdateHMDMovement()
        {
            Vector3 delta = Vector3.Scale(_neckPivot.position - _body.transform.position, new Vector3(1, 0, 1));//Vector3.Scale(_rig.Camera.transform.position - _head.transform.position, new Vector3(1, 0, 1));

            //if (delta.magnitude > 0.001f)
            {
                _wheel.MovePosition(_wheel.position + delta);
                _legs.MovePosition(_legs.position + delta);
                _head.MovePosition(_head.position + delta);
                _body.MovePosition(_body.position + delta);

                _rig.transform.localPosition -= _body.transform.InverseTransformDirection(delta);
            }
        }

        private float GetCrouchOffset(Crouch crouch)
        {
            switch (crouch)
            {
                case Crouch.Standing:
                    return 0f;
                case Crouch.HighSquat:
                    return _originalLegsHeight * 0.25f;
                case Crouch.Squat:
                    return _originalLegsHeight * 0.5f;
                case Crouch.LowSquat:
                    return _originalLegsHeight * 0.85f;
            }
            return 0f;
        }
    }
}
