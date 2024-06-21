using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    // Inventory slot is an interactor and an interactable at the same time.
    // It uses XRIntentoryItemInteractable to be able to receive hover and selection of the hand
    public class XRInventorySlot : XRBaseInteractor, IXRSocket
    {
        [SerializeField] private XRIntentoryItemInteractable m_ItemInteractable;
        [SerializeField] private XRBaseInteractable m_SpawnItem;

        private List<IXRSelectInteractable> m_ValidTargets = new List<IXRSelectInteractable>();

        protected override void Start()
        {
            base.Start();
            AddListeners();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveListeners();
        }

        private void AddListeners()
        {
            m_ItemInteractable.selectEntered.AddListener(OnItemSelected);
        }

        private void RemoveListeners()
        {
            m_ItemInteractable.selectEntered.RemoveListener(OnItemSelected);
        }

        private void OnItemSelected(SelectEnterEventArgs args)
        {
            IXRSelectInteractable slotInteractable = firstInteractableSelected;

            if (slotInteractable != null)
            {
                if (m_SpawnItem)
                {
                    IXRSelectInteractable item = Instantiate(m_SpawnItem, transform.position, transform.rotation);
                    interactionManager.SelectExit(args.interactorObject, args.interactableObject);
                    interactionManager.SelectEnter(args.interactorObject, item);
                }
                else
                {
                    interactionManager.SelectExit(args.interactorObject, args.interactableObject);
                    interactionManager.SelectExit(this, slotInteractable);
                    interactionManager.SelectEnter(args.interactorObject, slotInteractable);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (firstInteractableSelected == null && interactionManager.TryGetInteractableForCollider(other, out IXRInteractable interactable) && !m_ValidTargets.Contains(interactable))
            {
                if (interactable is IXRSelectInteractable selectInteractable && selectInteractable.firstInteractorSelecting != null)
                {
                    m_ValidTargets.Add(selectInteractable);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (firstInteractableSelected == null && interactionManager.TryGetInteractableForCollider(other, out IXRInteractable interactable) && m_ValidTargets.Contains(interactable))
            {
                if (interactable is IXRSelectInteractable selectInteractable)
                {
                    m_ValidTargets.Remove(selectInteractable);
                }
            }
        }


        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            if (m_ValidTargets.Any())
            {
                for (int i = 0; i < m_ValidTargets.Count; i++)
                {
                    if (m_ValidTargets[i].firstInteractorSelecting == null)
                    {
                        targets.Add(m_ValidTargets[i]);
                        m_ValidTargets.Clear();
                        break;
                    }
                }
            }
        }
    }
}
