using Unity.XR.CoreUtils;

namespace UnityEngine.VRInteraction
{
    public class XRSimplePlayerController : MonoBehaviour
    {
        [SerializeField] private XROrigin _rig;

        [SerializeField] private float _movementSpeed = 1f;
        [SerializeField] private float _playerHeight = 1.89f;
        [SerializeField] private CharacterController _characterController;

        [Header("Transforms")]
        [SerializeField] private Transform _body;
        [SerializeField] private Transform _chest;
        [SerializeField] private Transform _neckPivot;

        [Header("Input")]
        [SerializeField] private XRPlayerInput _playerInput;

        private IXRPlayerInput Input => _playerInput;

        private float _realCrouchAmount;
        private Vector2 _lerpedAxis;

        private void Start()
        {
            Input.OnSnapTurn.AddListener(OnTurn);
        }

        private void OnDestroy()
        {
            Input.OnSnapTurn.RemoveListener(OnTurn);
        }

        private void Update()
        {
            UpdateBody();
            Move();
        }

        private void OnTurn(int sign)
        {
            Quaternion rotation = Quaternion.Euler(0, sign * 45f, 0);

            _body.rotation *= rotation;
        }

        private void Move()
        {
            _lerpedAxis = Vector2.MoveTowards(_lerpedAxis, Input.MovementAxis, 6f * Time.deltaTime);

            Transform cameraTransform = _rig.Camera.transform;
            Vector3 direction = cameraTransform.forward * _lerpedAxis.y + cameraTransform.right * _lerpedAxis.x;
            direction.y = 0f;
            direction.Normalize();
            Vector3 axis = direction * _lerpedAxis.magnitude * 0.5f;
            Vector3 delta = axis * _movementSpeed * Time.deltaTime;
            _characterController.Move(delta);
            _characterController.Move(Physics.gravity);

            UpdateHMDMovement();
        }

        private void UpdateBody()
        {
            //_characterController.height = _rig.CameraInOriginSpaceHeight;

            Quaternion headRotation = _rig.Camera.transform.rotation;
            headRotation.x = headRotation.z = 0;
            headRotation.Normalize();
            _chest.localPosition = Vector3.up * (_rig.CameraInOriginSpaceHeight - _playerHeight + 0.2f);
            _chest.rotation = headRotation;
        }

        private void UpdateHMDMovement()
        {
            Vector3 delta = Vector3.Scale(_neckPivot.position - _body.position, new Vector3(1, 0, 1));

            if (delta.magnitude > 0.001f)
            {
                _characterController.Move(delta);
                _rig.transform.localPosition -= _body.InverseTransformDirection(delta);
            }
        }
    }
}