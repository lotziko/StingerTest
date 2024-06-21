using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public interface IXRGunAmmoInteractable : IXRSelectInteractable
    {
        int ammoCount { get; }
        int maxAmmoCount { get; }
        bool hasAmmo { get; }
        
        bool TryRemoveAmmo();
    }

    public class XRGunAmmoInteractable : XRGrabbableInteractable, IXRGunAmmoInteractable
    {
        [SerializeField] private int m_AmmoCount = 15;

        private int m_CurrentAmmoCount;

        public int ammoCount => m_CurrentAmmoCount;
        public int maxAmmoCount => m_AmmoCount;
        public bool hasAmmo => m_CurrentAmmoCount > 0;

        protected override void Awake()
        {
            base.Awake();
            m_CurrentAmmoCount = m_AmmoCount;
        }

        public bool TryRemoveAmmo()
        {
            if (m_CurrentAmmoCount > 0)
            {
                --m_CurrentAmmoCount;
                return true;
            }

            return false;
        }
    }
}
