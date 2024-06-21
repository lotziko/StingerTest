using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.VRInteraction
{
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_Controllers)]
    [DisallowMultipleComponent]
    public class XRHandController : MonoBehaviour
    {
        [SerializeField] private InputActionProperty m_PositionAction;
        [SerializeField] private InputActionProperty m_RotationAction;
        [SerializeField] private InputActionProperty m_SelectAction;
        [SerializeField] private InputActionProperty m_ActivateAction;
        [SerializeField] private InputActionProperty m_ActivateActionValue;
        [SerializeField] private InputActionProperty m_CommonPrimaryAction;

        private InteractionState m_SelectInteractionState;
        private InteractionState m_ActivateInteractionState;
        private InteractionState m_CommonPrimaryInteractionState;

        public InputActionProperty positionAction
        {
            get => m_PositionAction;
            set => SetInputActionProperty(ref m_PositionAction, value);
        }
        
        public InputActionProperty rotationAction
        {
            get => m_RotationAction;
            set => SetInputActionProperty(ref m_RotationAction, value);
        }
        
        public InputActionProperty selectAction
        {
            get => m_SelectAction;
            set => SetInputActionProperty(ref m_SelectAction, value);
        }

        public InteractionState selectInteractionState => m_SelectInteractionState;

        public InteractionState activateInteractionState => m_ActivateInteractionState;

        public InteractionState commonPrimaryInteractionState => m_CommonPrimaryInteractionState;

        private void OnEnable()
        {
            EnableAllDirectActions();
            Application.onBeforeRender += OnBeforeRender;
        }

        private void OnDisable()
        {
            DisableAllDirectActions();
            Application.onBeforeRender -= OnBeforeRender;
        }

        private void OnBeforeRender()
        {
            UpdateTracking();
            UpdateInput();
        }

        private void Update()
        {
            UpdateTracking();
            UpdateInput();
        }

        private void EnableAllDirectActions()
        {
            m_PositionAction.EnableDirectAction();
            m_RotationAction.EnableDirectAction();
            m_SelectAction.EnableDirectAction();
            m_ActivateAction.EnableDirectAction();
            m_CommonPrimaryAction.EnableDirectAction();
        }

        private void DisableAllDirectActions()
        {
            m_PositionAction.DisableDirectAction();
            m_RotationAction.DisableDirectAction();
            m_SelectAction.DisableDirectAction();
            m_ActivateAction.DisableDirectAction();
            m_CommonPrimaryAction.DisableDirectAction();
        }

        private void UpdateTracking()
        {
            if (m_PositionAction.action != null)
            {
                transform.localPosition = m_PositionAction.action.ReadValue<Vector3>();
            }

            if (m_RotationAction.action != null)
            {
                transform.localRotation = m_RotationAction.action.ReadValue<Quaternion>();
            }
        }

        private void UpdateInput()
        {
            InputAction selectValueAction = m_SelectAction.action;
            m_SelectInteractionState.SetFrameState(IsPressed(m_SelectAction.action), ReadValue(selectValueAction));

            InputAction activateValueAction = m_ActivateActionValue.action;
            if (activateValueAction == null || activateValueAction.bindings.Count <= 0)
                activateValueAction = m_ActivateAction.action;
            m_ActivateInteractionState.SetFrameState(IsPressed(m_ActivateAction.action), ReadValue(activateValueAction));

            InputAction commonPrimaryValueAction = m_CommonPrimaryAction.action;
            m_CommonPrimaryInteractionState.SetFrameState(IsPressed(m_CommonPrimaryAction.action), ReadValue(commonPrimaryValueAction));
        }

        private bool IsPressed(InputAction action)
        {
            if (action == null)
                return false;

#if INPUT_SYSTEM_1_1_OR_NEWER || INPUT_SYSTEM_1_1_PREVIEW // 1.1.0-preview.2 or newer, including pre-release
                return action.phase == InputActionPhase.Performed;
#else
            if (action.activeControl is ButtonControl buttonControl)
                return buttonControl.IsPressed(buttonControl.pressPointOrDefault);

            if (action.activeControl is AxisControl)
                return action.ReadValue<float>() >= 0.75f;

            return action.triggered || action.phase == InputActionPhase.Performed;
#endif
        }

        private float ReadValue(InputAction action)
        {
            if (action == null)
                return default;

            if (action.activeControl is AxisControl)
                return action.ReadValue<float>();

            if (action.activeControl is Vector2Control)
                return action.ReadValue<Vector2>().magnitude;

            return IsPressed(action) ? 1f : 0f;
        }

        private void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }
    }
}