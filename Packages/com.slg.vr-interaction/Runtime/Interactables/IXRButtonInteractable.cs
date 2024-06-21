using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public interface IXRButtonInteractable : IXRInteractable
    {
        void OnPrimaryPressed(ButtonPressEventArgs args);

        void OnPrimaryReleased(ButtonReleaseEventArgs args);
    }
}
