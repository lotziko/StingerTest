using TMPro;
using UnityEngine.UI;

namespace UnityEngine.VRInteraction
{
    public class XRPistolAmmoCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_Label;
        [SerializeField] private Image m_Counter;
        [SerializeField] private XRPistolController m_Controller;

        private void Start()
        {
            AddListeners();
            OnAmmoUpdated();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            m_Controller.ammoUpdated.AddListener(OnAmmoUpdated);
        }

        private void RemoveListeners()
        {
            m_Controller.ammoUpdated.RemoveListener(OnAmmoUpdated);
        }

        private void OnAmmoUpdated()
        {
            int maxCount = m_Controller.maxAmmoCount;
            m_Counter.fillAmount = maxCount > 0 ? (float)m_Controller.ammoCount / maxCount : 0;//m_Label.text = m_Controller.ammoCount.ToString();
        }
    }
}
