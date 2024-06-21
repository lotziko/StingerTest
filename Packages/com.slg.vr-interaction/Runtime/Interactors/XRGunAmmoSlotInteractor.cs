using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public class XRGunAmmoSlotInteractor : XRBaseInteractor, IXRSocket
    {
        private IXRGunAmmoInteractable m_ValidAmmoInteractable;

        public int ammoCount
        {
            get { return ((IXRGunAmmoInteractable)firstInteractableSelected)?.ammoCount ?? 0; }
        }

        public int maxAmmoCount
        {
            get { return ((IXRGunAmmoInteractable)firstInteractableSelected)?.maxAmmoCount ?? 0; }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (interactablesSelected.Count > 0 || m_ValidAmmoInteractable != null)
                return;
            
            if (interactionManager.TryGetInteractableForCollider(other, out IXRInteractable interactable))
            {
                IXRGunAmmoInteractable ammoInteractable = interactable as IXRGunAmmoInteractable;
                if (ammoInteractable != null && ammoInteractable.firstInteractorSelecting != null && ammoInteractable.firstInteractorSelecting != this as IXRSelectInteractor)
                {
                    interactionManager.SelectExit(ammoInteractable.firstInteractorSelecting, ammoInteractable);
                    m_ValidAmmoInteractable = ammoInteractable;
                }                
            }
        }

        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            if (m_ValidAmmoInteractable != null)
            {
                targets.Add(m_ValidAmmoInteractable);
                m_ValidAmmoInteractable = null;
            }
        }

        public bool HasAmmo()
        {
            if (firstInteractableSelected != null)
                return ((IXRGunAmmoInteractable)firstInteractableSelected).hasAmmo;
            return false;
        }

        public bool TryRemoveAmmo()
        {
            if (firstInteractableSelected != null)
                return ((IXRGunAmmoInteractable)firstInteractableSelected).TryRemoveAmmo();
            return false;
        }

        public void DropAmmo()
        {
            if (firstInteractableSelected != null)
                interactionManager.SelectExit(this, firstInteractableSelected);
        }
    }
}