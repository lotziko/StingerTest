using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public interface IXRButtonInteractor : IXRInteractor
    {
        bool shouldPressPrimary { get; }

        bool shouldReleasePrimary { get; }

        void GetAdditionalActionTargets(List<IXRButtonInteractable> targets);
    }
}
