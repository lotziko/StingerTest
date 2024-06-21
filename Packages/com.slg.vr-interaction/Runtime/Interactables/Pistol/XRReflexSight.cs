
namespace UnityEngine.VRInteraction
{
    [ExecuteAlways]
    public class XRReflexSight : MonoBehaviour
    {
        private static readonly int s_SightCenterPositionWSId = Shader.PropertyToID("_SightCenterPositionWS");
        private static readonly int s_SightIntersectionPositionWSId = Shader.PropertyToID("_SightIntersectionPositionWS");
        private static readonly int s_SightForwardDirectionWSId = Shader.PropertyToID("_SightForwardDirectionWS");
        private static readonly int s_SightUpDirectionWSId = Shader.PropertyToID("_SightUpDirectionWS");
        private static readonly int s_SightRightDirectionWSId = Shader.PropertyToID("_SightRightDirectionWS");

        [SerializeField] private MeshRenderer m_Lens;
        [SerializeField] private Transform m_BarrelStart;
        [SerializeField] private Transform m_BulletSpawn;
        [SerializeField] private float m_MaxDistance = 30f;
        [SerializeField] private float m_InverseReticleScale = 1f;

        private MaterialPropertyBlock m_PropertyBlock;
        private float m_CurrentTimer = 0f;

        private float m_CurrentDistance = 0f;
        private float m_InterpolatedDistance = 0f;

        private void Update()
        {
            if (m_PropertyBlock == null)
                m_PropertyBlock = new MaterialPropertyBlock();

            Vector4 intersection = Vector4.zero;
            if (Physics.Raycast(m_BulletSpawn.position, m_BulletSpawn.forward, out RaycastHit hit, m_MaxDistance))
                intersection = hit.point;
            else
                intersection = m_BulletSpawn.position + m_BulletSpawn.forward * m_MaxDistance;

            intersection.w = m_InverseReticleScale;

            m_PropertyBlock.SetVector(s_SightCenterPositionWSId, m_BarrelStart.position);
            m_PropertyBlock.SetVector(s_SightIntersectionPositionWSId, intersection);
            m_PropertyBlock.SetVector(s_SightForwardDirectionWSId, m_BarrelStart.forward);
            m_PropertyBlock.SetVector(s_SightUpDirectionWSId, m_BarrelStart.up);
            m_PropertyBlock.SetVector(s_SightRightDirectionWSId, m_BarrelStart.right);

            m_Lens.SetPropertyBlock(m_PropertyBlock);
        }
    }
}
