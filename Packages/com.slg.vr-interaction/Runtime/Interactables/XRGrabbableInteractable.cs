using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public interface IXRGrabbableInteractable : IXRInteractable
    {
        Rigidbody rigidbody { get; }
        bool isLocked { get; set; }

        bool CanAttract();
        bool TryGetGrabPoint(Vector3 position, out XRGrabPoint result);
    }

    public class XRGrabbableInteractable : XRBaseInteractable, IXRGrabbableInteractable, IXRButtonInteractable
    {
        [SerializeField] private Rigidbody m_Rigidbody;
        [SerializeField] private List<XRGrabPoint> m_GrabPoints;
        [SerializeField] private JointSettings m_CustomJointSettings;
        [SerializeField] private XRGrabbableInteractable m_DependencyInteractable;

        [SerializeField] private ButtonPressEvent m_ButtonPressed = new ButtonPressEvent();
        [SerializeField] private ButtonReleaseEvent m_ButtonReleased = new ButtonReleaseEvent();

        private bool m_IsGrabLocked = false;

        public JointSettings customJointSettings
        {
            get { return m_CustomJointSettings; }
        }

        public ButtonPressEvent buttonPressed
        {
            get { return m_ButtonPressed; }
            set { m_ButtonPressed = value; }
        }

        public ButtonReleaseEvent buttonReleased
        {
            get { return m_ButtonReleased; }
            set { m_ButtonReleased = value; }
        }

        public new Rigidbody rigidbody => m_Rigidbody;

        public bool isLocked
        {
            get { return m_IsGrabLocked; }
            set { m_IsGrabLocked = value; }
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            if (m_DependencyInteractable && (m_DependencyInteractable.firstInteractorSelecting == null || m_DependencyInteractable.m_IsGrabLocked))
                return false;

            if (interactorsSelecting.Count > 1)
                return interactorsSelecting.Contains(interactor);
            else if (interactorsSelecting.Count > 0)
                return firstInteractorSelecting == interactor;

            return base.IsSelectableBy(interactor);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactorObject is IXRSocket)
            {
                m_Rigidbody.detectCollisions = false;
                m_Rigidbody.isKinematic = true;
                transform.SetParent(args.interactorObject.GetAttachTransform(this), false);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (args.interactorObject is IXRSocket)
            {
                m_Rigidbody.detectCollisions = true;
                m_Rigidbody.isKinematic = false;
                transform.SetParent(null, true);
            }
        }

        public bool CanAttract()
        {
            return m_DependencyInteractable == null;
        }

        public bool TryGetGrabPoint(Vector3 position, out XRGrabPoint result)
        {
            result = null;
            if (m_GrabPoints.Count == 0)
                return false;
            
            float closestDistance = float.MaxValue;
            float tempDistance;

            for (int i = 0; i < m_GrabPoints.Count; i++)
            {
                XRGrabPoint grabPoint = m_GrabPoints[i];
                if (grabPoint.CanAttachHand())
                {
                    if ((tempDistance = Vector3.SqrMagnitude(grabPoint.transform.position - position)) < closestDistance)
                    {
                        result = grabPoint;
                        closestDistance = tempDistance;
                    }
                }
            }

            return result != null;
        }

        public void OnPrimaryPressed(ButtonPressEventArgs args)
        {
            m_ButtonPressed.Invoke(args);
        }

        public void OnPrimaryReleased(ButtonReleaseEventArgs args)
        {
            m_ButtonReleased.Invoke(args);
        }
    }
}
