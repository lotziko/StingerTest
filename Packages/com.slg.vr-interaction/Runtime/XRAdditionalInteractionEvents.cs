using System;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public enum XRCommonButton
    {
        Primary,
        Secondary
    }

    [Serializable]
    public sealed class ButtonPressEvent : UnityEvent<ButtonPressEventArgs>
    {
    }

    public class ButtonPressEventArgs : BaseInteractionEventArgs
    {
        /// <summary>
        /// The Interactor associated with the interaction event.
        /// </summary>
        public new IXRButtonInteractor interactorObject
        {
            get => (IXRButtonInteractor)base.interactorObject;
            set => base.interactorObject = value;
        }

        /// <summary>
        /// The Interactable associated with the interaction event.
        /// </summary>
        public new IXRButtonInteractable interactableObject
        {
            get => (IXRButtonInteractable)base.interactableObject;
            set => base.interactableObject = value;
        }

        /// <summary>
        /// The button being pressed
        /// </summary>
        public XRCommonButton button { get; set; }
    }

    [Serializable]
    public sealed class ButtonReleaseEvent : UnityEvent<ButtonReleaseEventArgs>
    {
    }

    public class ButtonReleaseEventArgs : BaseInteractionEventArgs
    {
        /// <summary>
        /// The Interactor associated with the interaction event.
        /// </summary>
        public new IXRButtonInteractor interactorObject
        {
            get => (IXRButtonInteractor)base.interactorObject;
            set => base.interactorObject = value;
        }

        /// <summary>
        /// The Interactable associated with the interaction event.
        /// </summary>
        public new IXRButtonInteractable interactableObject
        {
            get => (IXRButtonInteractable)base.interactableObject;
            set => base.interactableObject = value;
        }

        /// <summary>
        /// The button being released
        /// </summary>
        public XRCommonButton button { get; set; }
    }
}
