using System.Collections;

namespace UnityEngine.VRInteraction
{
    public enum Hand
    {
        Left, Right
    }

    [SelectionBase]
    [ExecuteAlways]
    public class PosableHand : MonoBehaviour
    {
        [SerializeField] private Hand m_Hand;
        [SerializeField] private Transform m_PhysicsHand;
        [SerializeField] private HandSkeleton m_HandSkeleton;
        [SerializeField] private XRHandTrackingSkeleton m_HandTrackingSkeleton;

        private HandSkeletonData m_HandTrackingData = new HandSkeletonData();
        private HandSkeletonData m_OverrideData = new HandSkeletonData();
        private IXRHandSkeletonSource m_OverrideSkeleton;
        private CoroutineInterpolator m_WeightInterpolator;
        private float m_OverrideWeight = 0f;

        public Hand hand
        { get { return m_Hand; } }

        public Transform wrist
        { get { return m_HandSkeleton?.wrist; } }

        public Transform[] joints
        { get { return m_HandSkeleton?.joints; } }

        public HandSkeleton handSkeleton
        { get { return m_HandSkeleton; } }

        public void SetOverrideSkeletonSource(IXRHandSkeletonSource skeletonSource, float blendingTime = 0f)
        {
            m_OverrideSkeleton = skeletonSource;

            float targetValue = skeletonSource == null ? 0f : 1f;
            m_WeightInterpolator.Skip();
            m_WeightInterpolator.Interpolate(m_OverrideWeight, targetValue, blendingTime, (float weight) => m_OverrideWeight = weight);
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                m_WeightInterpolator = new CoroutineInterpolator(this);
                StartCoroutine(HandUpdateCoroutine());
            }
        }

        private IEnumerator HandUpdateCoroutine()
        {
            while (true)
            {
                if (m_OverrideWeight < 1f)
                    m_HandTrackingSkeleton.GetSkeletonData(m_Hand, m_HandTrackingData);

                if (m_OverrideWeight > 0f && m_OverrideSkeleton != null)
                    m_OverrideSkeleton.GetSkeletonData(m_Hand, m_OverrideData);

                if (Mathf.Approximately(m_OverrideWeight, 0f) && m_HandTrackingData.isValid)
                    m_HandSkeleton.Apply(m_HandTrackingData);
                else if (Mathf.Approximately(m_OverrideWeight, 1f) && m_OverrideData.isValid)
                    m_HandSkeleton.Apply(m_OverrideData);
                else if (m_HandTrackingData.isValid && m_OverrideData.isValid)
                    m_HandSkeleton.Apply(m_HandTrackingData, m_OverrideData, m_OverrideWeight);

                yield return null;
            }
        }
    }
}
