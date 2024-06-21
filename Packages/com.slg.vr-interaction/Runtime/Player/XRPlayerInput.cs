using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace UnityEngine.VRInteraction
{
    public interface IXRPlayerInput
    {
        UnityEvent<int> OnSnapTurn { get; }
        UnityEvent OnCrouchTap { get; }
        UnityEvent OnJumpPrepare { get; }
        UnityEvent OnJump { get; }
        UnityEvent OnRun { get; }
        bool IsCrouchHold { get; }
        Vector2 MovementAxis { get; }
        int CrouchValue { get; }
    }

    public class XRPlayerInput : MonoBehaviour, IXRPlayerInput
    {
        [Header("Actions")]
        [SerializeField] private InputActionProperty _movementInput;
        [SerializeField] private InputActionProperty _snapTurnInput;
        [SerializeField] private InputActionProperty _crouchInput;
        [SerializeField] private InputActionProperty _jumpInput;
        [SerializeField] private InputActionProperty _runInput;

        [SerializeField] private UnityEvent<int> _onSnapTurn;
        [SerializeField] private UnityEvent _onCrouchTap;
        [SerializeField] private UnityEvent _onJumpPrepare;
        [SerializeField] private UnityEvent _onJump;
        [SerializeField] private UnityEvent _onRun;

        private Vector2 _movementAxis;
        private bool _isCrouching;
        private bool _isCrouchHold;
        private int _crouchValue;
        private int _previousCrouchValue;
        private float _crouchTimer;
        private float _lastRunPressTime = 0f;

        private void Start()
        {
            _movementInput.action.performed += OnMovementPerformed;
            _movementInput.action.canceled += OnMovementCanceled;
            _snapTurnInput.action.performed += OnSnapTurnPerformed;
            _crouchInput.action.performed += OnCrouchPerformed;
            _jumpInput.action.performed += OnJumpPerformed;
            _jumpInput.action.canceled += OnJumpCanceled;
            //_runInput.action.performed += OnRunPerformed;
        }

        private void OnDestroy()
        {
            _movementInput.action.performed -= OnMovementPerformed;
            _movementInput.action.canceled -= OnMovementCanceled;
            _snapTurnInput.action.performed -= OnSnapTurnPerformed;
            _crouchInput.action.performed -= OnCrouchPerformed;
            _jumpInput.action.performed -= OnJumpPerformed;
            _jumpInput.action.canceled -= OnJumpCanceled;
            //_runInput.action.performed -= OnRunPerformed;
        }

        private void FixedUpdate()
        {
            if (_crouchValue != 0)
            {
                if (_crouchValue != _previousCrouchValue)
                {
                    _isCrouching = true;
                    _crouchTimer = 0f;
                }
                if (_isCrouching)
                {
                    if (_crouchTimer > 0.15f)
                    {
                        _isCrouchHold = true;
                    }
                    _crouchTimer += Time.fixedDeltaTime;
                }
            }
            else
            {
                if (_crouchTimer <= 0.15f)
                {
                    _isCrouching = false;
                    _onCrouchTap?.Invoke();
                }
                if (_crouchValue != _previousCrouchValue)
                {
                    _isCrouching = false;
                    _isCrouchHold = false;
                }
            }
            _previousCrouchValue = _crouchValue;
        }

        UnityEvent<int> IXRPlayerInput.OnSnapTurn => _onSnapTurn;
        UnityEvent IXRPlayerInput.OnCrouchTap => _onCrouchTap;
        UnityEvent IXRPlayerInput.OnJumpPrepare => _onJumpPrepare;
        UnityEvent IXRPlayerInput.OnJump => _onJump;
        UnityEvent IXRPlayerInput.OnRun => _onRun;
        bool IXRPlayerInput.IsCrouchHold => _isCrouchHold;
        Vector2 IXRPlayerInput.MovementAxis => _movementAxis;
        int IXRPlayerInput.CrouchValue => _crouchValue;

        private void OnMovementPerformed(InputAction.CallbackContext obj)
        {
            _movementAxis = obj.ReadValue<Vector2>();
        }

        private void OnMovementCanceled(InputAction.CallbackContext obj)
        {
            _movementAxis = Vector2.zero;
        }

        private void OnSnapTurnPerformed(InputAction.CallbackContext obj)
        {
            _onSnapTurn?.Invoke((int)Mathf.Sign(obj.ReadValue<Vector2>().x));
        }

        private void OnCrouchPerformed(InputAction.CallbackContext obj)
        {
            Vector2 axis = obj.ReadValue<Vector2>();
        }

        private void OnJumpPerformed(InputAction.CallbackContext obj)
        {
            _onJumpPrepare?.Invoke();
        }

        private void OnJumpCanceled(InputAction.CallbackContext obj)
        {
            _onJump?.Invoke();
        }

        private void OnRunPerformed(InputAction.CallbackContext obj)
        {
            if (_lastRunPressTime > 0)
            {
                if (Time.time - _lastRunPressTime < 0.3f)
                {
                    _onRun?.Invoke();
                    _lastRunPressTime = 0f;
                }
            }
            _lastRunPressTime = Time.time;
        }
    }
}
