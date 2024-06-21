using System.Collections;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    public class SlideEvent : UnityEvent<float>
    {
    }

    public class EjectEvent : UnityEvent
    {
    }

    public class FullSlideEvent : UnityEvent
    {
    }

    public class SlideRackEvent : UnityEvent<float>
    {
    }

    public class XRGunSliderInteractable : XRGrabbableInteractable
    {
        [SerializeField] private Transform m_SlideStart;
        [SerializeField] private Transform m_SlideEnd;
        [SerializeField] private Transform m_GrabStart;

        [Header("Options")]
        [SerializeField, Range(0, 1)] private float m_EjectPercent = 0.85f;
        [SerializeField, Range(0, 1)] private float m_ChamberPercent = 0.5f;
        [SerializeField, Range(0, 1)] private float m_SpringDifficulty = 0.7f;
        [SerializeField, Range(0, 1)] private float m_LockPercent = 0.75f;

        [Header("Events")]
        [SerializeField] private SlideEvent m_Slided = new SlideEvent();
        [SerializeField] private FullSlideEvent m_FullSlided = new FullSlideEvent();
        [SerializeField] private SlideRackEvent m_SlideRacked = new SlideRackEvent();

        private bool m_IsLocked = false;
        private float m_Value = 0f;

        public SlideEvent slided
        {
            get { return m_Slided; }
            set { m_Slided = value; }
        }

        /// <summary>
        /// An event called when releasing selection with full slide. Should be used for lock/unlock.
        /// </summary>
        public FullSlideEvent fullSlided
        {
            get { return m_FullSlided; }
            set { m_FullSlided = value; }
        }

        public SlideRackEvent slideRacked
        {
            get { return m_SlideRacked; }
            set { m_SlideRacked = value; }
        }

        public float value
        {
            get { return m_Value; }
        }

        public bool isLocked
        {
            get { return m_IsLocked; }
            set
            {
                if (m_IsLocked != value)
                {
                    m_IsLocked = value;
                    if (value)
                    {
                        if (!Mathf.Approximately(m_Value, m_LockPercent))
                            m_Slided?.Invoke(m_LockPercent);
                        m_Value = Mathf.Clamp(m_Value, m_LockPercent, 1f);
                    }
                }
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            m_GrabStart.position = args.interactorObject.transform.position;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            // Slide unlock
            if (Mathf.Approximately(m_Value, 1f))
            {
                m_FullSlided?.Invoke();
            }
            m_SlideRacked?.Invoke(m_Value - GetMinValue());
        }

        private void FixedUpdate()
        {
            float newValue = 0f;
            if (interactorsSelecting.Count > 0)
            {
                newValue = GetSlidingAmount(firstInteractorSelecting.transform.position);
            }
            else
            {
                newValue = Mathf.Clamp(m_Value - (3f / m_SpringDifficulty) * Time.fixedDeltaTime, GetMinValue(), 1f);
            }

            if (!Mathf.Approximately(newValue, m_Value))
            {
                m_Value = newValue;
                m_Slided?.Invoke(newValue);
            }
        }

        private float GetSlidingAmount(Vector3 interactorPosition)
        {
            float minValue = GetMinValue();

            Vector3 startPosition = m_SlideStart.localPosition;
            Vector3 endPosition = m_SlideEnd.localPosition;

            Vector3 backDirection = endPosition - startPosition;
            Vector3 pullDirection = m_SlideStart.parent.InverseTransformVector(interactorPosition - m_GrabStart.position);
            Vector3 projectedVector = Vector3.Project(pullDirection, backDirection.normalized);

            // Clamp opposite direction
            if (Vector3.Dot(projectedVector, backDirection) < 0)
                return minValue;

            return Mathf.Clamp(projectedVector.sqrMagnitude / backDirection.sqrMagnitude * m_SpringDifficulty, minValue, 1f);
        }

        private float GetMinValue()
        {
            return m_IsLocked ? m_LockPercent : 0f;
        }
    }
}