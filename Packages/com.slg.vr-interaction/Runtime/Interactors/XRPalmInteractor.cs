using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public class XRPalmInteractor : XRBaseInteractor, IXRActivateValueInteractor, IXRButtonInteractor
    {
        [SerializeField] private XRHandController m_Controller;

        [Header("Grab options")]
        [SerializeField] private LayerMask m_GrabLayer;
        [SerializeField] private float m_GrabRadius = 0.2f;
        [SerializeField] private float m_GrabTime = 0.1f;
        [SerializeField] private Transform m_GrabAnchor;

        public XRHandController xrController
        {
            get => m_Controller;
            set => m_Controller = value;
        }

        private readonly LinkedPool<ActivateEventArgs> m_ActivateEventArgs = new LinkedPool<ActivateEventArgs>(() => new ActivateEventArgs(), collectionCheck: false);
        private readonly LinkedPool<DeactivateEventArgs> m_DeactivateEventArgs = new LinkedPool<DeactivateEventArgs>(() => new DeactivateEventArgs(), collectionCheck: false);
        private readonly LinkedPool<ButtonPressEventArgs> m_ButtonPressEventArgs = new LinkedPool<ButtonPressEventArgs>(() => new ButtonPressEventArgs(), collectionCheck: false);
        private readonly LinkedPool<ButtonReleaseEventArgs> m_ButtonReleaseEventArgs = new LinkedPool<ButtonReleaseEventArgs>(() => new ButtonReleaseEventArgs(), collectionCheck: false);

        private static readonly List<IXRActivateInteractable> s_ActivateTargets = new List<IXRActivateInteractable>();
        private static readonly List<IXRButtonInteractable> s_ButtonTargets = new List<IXRButtonInteractable>();

        private readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();

        public override bool isSelectActive
        {
            get
            {
                if (!base.isSelectActive)
                    return false;

                return m_Controller.selectInteractionState.active;
            }
        }

        public float value => hasSelection && m_Controller != null ? m_Controller.activateInteractionState.value : 0f;

        public bool shouldActivate => hasSelection && m_Controller != null && m_Controller.activateInteractionState.activatedThisFrame;

        public bool shouldDeactivate => hasSelection && m_Controller != null && m_Controller.activateInteractionState.deactivatedThisFrame;

        public bool shouldPressPrimary => hasSelection && m_Controller != null && m_Controller.commonPrimaryInteractionState.activatedThisFrame;

        public bool shouldReleasePrimary => hasSelection && m_Controller != null && m_Controller.commonPrimaryInteractionState.deactivatedThisFrame;

        public void GetActivateTargets(List<IXRActivateInteractable> targets)
        {
            targets.Clear();
            if (hasSelection)
            {
                foreach (var interactable in interactablesSelected)
                {
                    if (interactable is IXRActivateInteractable activateInteractable)
                    {
                        targets.Add(activateInteractable);
                    }
                }
            }
        }

        public void GetAdditionalActionTargets(List<IXRButtonInteractable> targets)
        {
            targets.Clear();
            if (hasSelection)
            {
                foreach (var interactable in interactablesSelected)
                {
                    if (interactable is IXRButtonInteractable additionalActionInteractable)
                    {
                        targets.Add(additionalActionInteractable);
                    }
                }
            }
        }

        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                ProcessSelection();
                ProcessActivation();
                ProcessButtons();
            }
        }
        
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            if (m_ValidTargets.Any())
            {
                targets.AddRange(m_ValidTargets);
                m_ValidTargets.Clear();
            }
        }
        
        public override Transform GetAttachTransform(IXRInteractable interactable)
        {
            return m_GrabAnchor;
        }

        private void ProcessSelection()
        {
            if (m_Controller.selectInteractionState.activatedThisFrame && !hasSelection)
            {
                if (TryGrabInteractable(out IXRSelectInteractable interactable))
                {
                    m_ValidTargets.Add(interactable);
                }
            }
        }

        private void ProcessActivation()
        {
            bool sendActivate = shouldActivate;
            bool sendDeactivate = shouldDeactivate;

            if (sendActivate || sendDeactivate)
            {
                GetActivateTargets(s_ActivateTargets);

                if (sendActivate)
                    SendActivateEvent(s_ActivateTargets);

                // Note that this makes it possible for an interactable to receive an OnDeactivated event
                // but not the earlier OnActivated event if it was selected afterward.
                if (sendDeactivate)
                    SendDeactivateEvent(s_ActivateTargets);
            }
        }

        private void ProcessButtons()
        {
            bool sendPressPrimary = shouldPressPrimary;
            bool sendReleasePrimary = shouldReleasePrimary;

            if (sendPressPrimary || sendReleasePrimary)
            {
                GetAdditionalActionTargets(s_ButtonTargets);

                if (sendPressPrimary)
                    SendButtonPressEvent(s_ButtonTargets, XRCommonButton.Primary);

                // Note that this makes it possible for an interactable to receive an OnDeactivated event
                // but not the earlier OnActivated event if it was selected afterward.
                if (sendReleasePrimary)
                    SendButtonReleaseEvent(s_ButtonTargets, XRCommonButton.Primary);
            }
        }

        private bool TryGrabInteractable(out IXRSelectInteractable interactable)
        {
            IXRSelectInteractor selectInteractor = this as IXRSelectInteractor;
            if (Physics.CheckSphere(m_GrabAnchor.position, m_GrabRadius, m_GrabLayer))
            {
                Collider[] colliders = Physics.OverlapSphere(m_GrabAnchor.position, m_GrabRadius, m_GrabLayer, QueryTriggerInteraction.Collide);
                Dictionary<Transform, (Collider collider, IXRSelectInteractable interactable)> grabTransforms = new Dictionary<Transform, (Collider collider, IXRSelectInteractable interactable)>();
                foreach (Collider collider in colliders)
                {
                    if (interactionManager.TryGetInteractableForCollider(collider, out IXRInteractable baseInteractable))
                    {
                        IXRSelectInteractable selectInteractable = baseInteractable as IXRSelectInteractable;
                        if (!selectInteractable.IsSelectableBy(selectInteractor))
                            continue;
                        if (baseInteractable is IXRGrabbableInteractable grabInteractable)
                        {
                            if (grabInteractable.TryGetGrabPoint(GetAttachTransform(baseInteractable).position, out XRGrabPoint grabPoint))
                            {
                                Transform grabTransform = grabPoint.transform;
                                if (!grabTransforms.ContainsKey(grabTransform) && Vector3.Dot(m_GrabAnchor.forward, (grabTransform.position - m_GrabAnchor.position).normalized) > 0.5f)
                                    grabTransforms.Add(grabTransform, (collider, selectInteractable));
                            }
                        }
                        else
                        {
                            if (!grabTransforms.ContainsKey(baseInteractable.transform))
                                grabTransforms.Add(baseInteractable.transform, (collider, selectInteractable));
                        }
                    }
                }
                if (grabTransforms.Count > 0)
                {
                    Transform closest = grabTransforms.OrderBy((i) => Vector3.Distance(i.Key.position, m_GrabAnchor.position) + (i.Value.collider is SphereCollider sphereCollider ? (1f / sphereCollider.radius * 0.0002f) : 0f)).FirstOrDefault().Key;
                    interactable = grabTransforms[closest].interactable;
                    return true;
                }
            }
            interactable = null;
            return false;
        }

        private void SendActivateEvent(List<IXRActivateInteractable> targets)
        {
            foreach (var interactable in targets)
            {
                if (interactable == null || interactable as Object == null)
                    continue;

                using (m_ActivateEventArgs.Get(out ActivateEventArgs args))
                {
                    args.interactorObject = this;
                    args.interactableObject = interactable;
                    interactable.OnActivated(args);
                }
            }
        }

        private void SendDeactivateEvent(List<IXRActivateInteractable> targets)
        {
            foreach (IXRActivateInteractable interactable in targets)
            {
                if (interactable == null || interactable as Object == null)
                    continue;

                using (m_DeactivateEventArgs.Get(out DeactivateEventArgs args))
                {
                    args.interactorObject = this;
                    args.interactableObject = interactable;
                    interactable.OnDeactivated(args);
                }
            }
        }

        private void SendButtonPressEvent(List<IXRButtonInteractable> targets, XRCommonButton button)
        {
            foreach (IXRButtonInteractable interactable in targets)
            {
                if (interactable == null || interactable as Object == null)
                    continue;

                using (m_ButtonPressEventArgs.Get(out ButtonPressEventArgs args))
                {
                    args.interactorObject = this;
                    args.interactableObject = interactable;
                    args.button = button;
                    interactable.OnPrimaryPressed(args);
                }
            }
        }

        private void SendButtonReleaseEvent(List<IXRButtonInteractable> targets, XRCommonButton button)
        {
            foreach (IXRButtonInteractable interactable in targets)
            {
                if (interactable == null || interactable as Object == null)
                    continue;

                using (m_ButtonReleaseEventArgs.Get(out ButtonReleaseEventArgs args))
                {
                    args.interactorObject = this;
                    args.interactableObject = interactable;
                    args.button = button;
                    interactable.OnPrimaryReleased(args);
                }
            }
        }
    }
}
