using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.VRInteraction
{
    public class DecalTest : MonoBehaviour
    {
        [SerializeField] private GameObject m_ImpactPrefab;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    //if (hit.collider.TryGetComponent(out MonoBehaviour hitFx))
                    {
                        GameObject decal = Instantiate(m_ImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        decal.transform.SetParent(hit.transform, true);
                    }
                }
            }
        }
    }
}
