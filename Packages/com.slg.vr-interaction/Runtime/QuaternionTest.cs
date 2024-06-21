using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.VRInteraction
{
    public class QuaternionTest : MonoBehaviour
    {
        [ContextMenu("Debug")]
        public void Debug()
        {
            UnityEngine.Debug.Log(transform.rotation.x + " " + transform.rotation.y + " " + transform.rotation.z + " " + transform.rotation.w);
        }
    }
}
