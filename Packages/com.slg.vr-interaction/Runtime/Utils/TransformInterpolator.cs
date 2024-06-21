
namespace UnityEngine.VRInteraction
{
    // Based on: https://forum.unity.com/threads/motion-interpolation-solution-to-eliminate-fixedupdate-stutter.1325943/
    [DefaultExecutionOrder(-5)]
    public class TransformInterpolator : MonoBehaviour
    {
        private struct TransformData
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        private TransformData m_PreviousTransformData;
        private TransformData m_TransformData;
        private bool m_IsInterpolated;

        private void OnEnable()
        {
            m_PreviousTransformData.position = transform.position;
            m_PreviousTransformData.rotation = transform.rotation;
            m_IsInterpolated = false;
        }

        private void FixedUpdate()
        {
            if (m_IsInterpolated)
            {
                transform.position = m_TransformData.position;
                transform.rotation = m_TransformData.rotation;
                m_IsInterpolated = false;
            }

            m_PreviousTransformData.position = transform.position;
            m_PreviousTransformData.rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            if (!m_IsInterpolated)
            {
                m_TransformData.position = transform.position;
                m_TransformData.rotation = transform.rotation;
                m_IsInterpolated = true;
            }

            float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            transform.position = Vector3.Lerp(m_PreviousTransformData.position, m_TransformData.position, t);
            transform.rotation = Quaternion.Lerp(m_PreviousTransformData.rotation, m_TransformData.rotation, t);
        }

        //private void FixedUpdate()
        //{
        //    m_PreviousPosition = m_Target.position;
        //}

        //[SerializeField] private Transform m_Target;

        //private Vector3 m_PreviousPosition;

        //private void Update()
        //{
        //    float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        //    transform.SetPositionAndRotation(Vector3.Lerp(m_PreviousPosition, m_Target.position, t), m_Target.rotation);
        //}

        //private void FixedUpdate()
        //{
        //    m_PreviousPosition = m_Target.position;
        //}

        //private Vector3 m_FirstPosition;
        //private Vector3 m_SecondPosition;
        //private Quaternion m_FirstRotation;
        //private Quaternion m_SecondRotation;

        //private void Awake()
        //{
        //    m_FirstPosition = m_SecondPosition = m_Target.position;
        //    m_FirstRotation = m_SecondRotation = m_Target.rotation;
        //}

        //private void FixedUpdate()
        //{
        //    m_SecondPosition = m_FirstPosition;
        //    m_FirstPosition = m_Target.position;

        //    m_SecondRotation = m_FirstRotation;
        //    m_FirstRotation = m_Target.rotation;
        //}

        //private void LateUpdate()
        //{
        //    float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        //    Debug.Log(t);
        //    //transform.SetPositionAndRotation(Vector3.Lerp(m_SecondPosition, m_FirstPosition, t), Quaternion.Lerp(m_SecondRotation, m_FirstRotation, t));
        //}
    }
}
