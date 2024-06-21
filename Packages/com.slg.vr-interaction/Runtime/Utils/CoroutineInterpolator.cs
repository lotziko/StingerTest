using System;
using System.Collections;

namespace UnityEngine.VRInteraction
{
    public delegate float UniversalFloatInterpolator(float from, float to);

    public interface IInterpolatorBase
    {
        void OnProc(UniversalFloatInterpolator interpolator);
    }

    public class InterpolatorEmpty : IInterpolatorBase
    {
        void IInterpolatorBase.OnProc(UniversalFloatInterpolator interpolator)
        { }
    }

    public class InterpolatorFloat : IInterpolatorBase
    {
        float m_From;
        float m_To;
        Action<float> m_OnInterpolationProc;

        public InterpolatorFloat(float from, float to, Action<float> onInterpolationProc)
        {
            m_From = from;
            m_To = to;
            m_OnInterpolationProc = onInterpolationProc;
        }

        void IInterpolatorBase.OnProc(UniversalFloatInterpolator interpolator)
        {
            if (m_OnInterpolationProc != null)
                m_OnInterpolationProc(interpolator(m_From, m_To));
        }
    }

    public class InterpolatorVector2 : IInterpolatorBase
    {
        Vector2 m_From;
        Vector2 m_To;
        Action<Vector2> m_OnInterpolationProc;

        public InterpolatorVector2(Vector2 from, Vector2 to, Action<Vector2> onInterpolationProc)
        {
            m_From = from;
            m_To = to;
            m_OnInterpolationProc = onInterpolationProc;
        }

        void IInterpolatorBase.OnProc(UniversalFloatInterpolator interpolator)
        {
            if (m_OnInterpolationProc != null)
                m_OnInterpolationProc(new Vector2(interpolator(m_From.x, m_To.x), interpolator(m_From.y, m_To.y)));
        }
    }

    public class InterpolatorVector3 : IInterpolatorBase
    {
        Vector3 m_From;
        Vector3 m_To;
        Action<Vector3> m_OnInterpolationProc;

        public InterpolatorVector3(Vector3 from, Vector3 to, Action<Vector3> onInterpolationProc)
        {
            m_From = from;
            m_To = to;
            m_OnInterpolationProc = onInterpolationProc;
        }

        void IInterpolatorBase.OnProc(UniversalFloatInterpolator interpolator)
        {
            if (m_OnInterpolationProc != null)
                m_OnInterpolationProc(new Vector3(interpolator(m_From.x, m_To.x), interpolator(m_From.y, m_To.y), interpolator(m_From.z, m_To.z)));
        }
    }

    public class InterpolatorVector4 : IInterpolatorBase
    {
        Vector4 m_From;
        Vector4 m_To;
        Action<Vector4> m_OnInterpolationProc;

        public InterpolatorVector4(Vector4 from, Vector4 to, Action<Vector4> onInterpolationProc)
        {
            m_From = from;
            m_To = to;
            m_OnInterpolationProc = onInterpolationProc;
        }

        void IInterpolatorBase.OnProc(UniversalFloatInterpolator interpolator)
        {
            if (m_OnInterpolationProc != null)
                m_OnInterpolationProc(new Vector4(interpolator(m_From.x, m_To.x), interpolator(m_From.y, m_To.y), interpolator(m_From.z, m_To.z), interpolator(m_From.w, m_To.w)));
        }
    }

    public class InterpolatorColor : IInterpolatorBase
    {
        Color m_From;
        Color m_To;
        Action<Color> m_OnInterpolationProc;

        public InterpolatorColor(Color from, Color to, Action<Color> onInterpolationProc)
        {
            m_From = from;
            m_To = to;
            m_OnInterpolationProc = onInterpolationProc;
        }

        void IInterpolatorBase.OnProc(UniversalFloatInterpolator interpolator)
        {
            if (m_OnInterpolationProc != null)
                m_OnInterpolationProc(new Color(interpolator(m_From.r, m_To.r), interpolator(m_From.g, m_To.g), interpolator(m_From.b, m_To.b), interpolator(m_From.a, m_To.a)));
        }
    }

    public class CoroutineInterpolatorInternal
    {
        IInterpolatorBase m_InterpolatorCore;
        float m_Epsilon = 0.0001f;
        Action m_OnInterpolationComplete;
        float m_Value;
        float m_DeltaValue;
        Interpolations.eInterpolation m_Interpolation;
        bool m_IsComplete;

        public CoroutineInterpolatorInternal(Action onInterpolationComplete, Interpolations.eInterpolation interpolation, IInterpolatorBase interolatorCore)
        {
            m_Interpolation = interpolation;
            m_OnInterpolationComplete = onInterpolationComplete;
            m_InterpolatorCore = interolatorCore;

        }

        public void Init(float time)
        {
            if (time > m_Epsilon)
            {
                m_DeltaValue = 1.0f / time;
                m_Value = 0.0f;
                NotifyProc();
            }
            else
            {
                m_IsComplete = true;
                m_Value = 1.0f;
                NotifyProc();
                NotifyComplete();
            }
        }

        public bool IsActive()
        { return !m_IsComplete; }

        public void Update(float time)
        {
            m_Value += m_DeltaValue * time;

            if (m_Value >= 1.0f)
            {
                m_Value = 1.0f;
                m_IsComplete = true;
            }

            NotifyProc();

            if (m_IsComplete)
                NotifyComplete();
        }

        public void ForcedComplete()
        {
            if (m_IsComplete)
                return;

            m_Value = 1.0f;
            m_IsComplete = true;
            NotifyProc();
            NotifyComplete();
        }

        void NotifyProc()
        { m_InterpolatorCore.OnProc(UniversalFloatInterpolator); }

        void NotifyComplete()
        {
            if (m_OnInterpolationComplete != null)
                m_OnInterpolationComplete();
        }

        float UniversalFloatInterpolator(float from, float to)
        { return Interpolations.Interpolate(from, to, m_Value, m_Interpolation); }
    }

    public partial class CoroutineInterpolator
    {
        static IEnumerator InterpolProcUnmanaged(float time, Action onInterpolationComplete, Interpolations.eInterpolation interpolation, IInterpolatorBase interpolatorCore)
        {
            CoroutineInterpolatorInternal interpolator = new CoroutineInterpolatorInternal(onInterpolationComplete, interpolation, interpolatorCore);
            interpolator.Init(time);

            while (interpolator.IsActive())
            {
                yield return new WaitForEndOfFrame();
                interpolator.Update(Time.deltaTime);
            }
        }

        // ---

        MonoBehaviour m_MonoBehaviour;
        CoroutineInterpolatorInternal m_CoroutineInterpolatorInternal = null;
        float m_TimeScale = 1.0f;

        enum eTime
        {
            TIME_SCALED,
            TIME_UNSCALED,
            TIME_FIXED,
        }

        eTime m_TimeType = eTime.TIME_SCALED;

        void CoroutineInterpolator_w(MonoBehaviour monoBehaviour)
        { m_MonoBehaviour = monoBehaviour; }

        void Skip_w()
        { m_CoroutineInterpolatorInternal = null; }

        void ForcedComplete_w()
        {
            if (m_CoroutineInterpolatorInternal != null)
            {
                m_CoroutineInterpolatorInternal.ForcedComplete();
                m_CoroutineInterpolatorInternal = null;
            }
        }

        void SetTimeScale_w(float timeScale)
        { m_TimeScale = timeScale; }

        float GetTimeScale_w()
        { return m_TimeScale; }

        void SetUnscaledTime_w()
        { m_TimeType = eTime.TIME_UNSCALED; }

        void SetScaledTime_w()
        { m_TimeType = eTime.TIME_SCALED; }

        void SetFixedTime_w()
        { m_TimeType = eTime.TIME_FIXED; }

        IEnumerator InterpolProc(float time, Action onInterpolationComplete, Interpolations.eInterpolation interpolation, IInterpolatorBase interolatorCore)
        {
            CoroutineInterpolatorInternal interpolator = m_CoroutineInterpolatorInternal = new CoroutineInterpolatorInternal(onInterpolationComplete, interpolation, interolatorCore);
            interpolator.Init(time);

            while (interpolator.IsActive())
            {
                if (m_TimeType != eTime.TIME_FIXED)
                    yield return new WaitForEndOfFrame();
                else
                    yield return new WaitForFixedUpdate();

                if (m_CoroutineInterpolatorInternal != interpolator)
                    yield break;

                float time_ = 0.0f;
                switch (m_TimeType)
                {
                    case eTime.TIME_SCALED:
                        time_ = Time.deltaTime;
                        break;
                    case eTime.TIME_UNSCALED:
                        time_ = Time.unscaledDeltaTime;
                        break;
                    case eTime.TIME_FIXED:
                        time_ = Time.fixedDeltaTime;
                        break;
                    default:
                        //Assert.Check(false, m_TimeType.ToString());
                        time_ = 0.0f;
                        break;
                }

                interpolator.Update(time_ * m_TimeScale);
            }
        }
    }

    public partial class CoroutineInterpolator
    {
        public static Coroutine InterpolateUnmanaged(MonoBehaviour monoBehaviour, float startValue, float completeValue, float time, Action<float> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return monoBehaviour.StartCoroutine(InterpolProcUnmanaged(time, onInterpolationComplete, interpolation, new InterpolatorFloat(startValue, completeValue, onInterpolationProc))); }

        public static Coroutine InterpolateUnmanaged(MonoBehaviour monoBehaviour, Vector2 startValue, Vector2 completeValue, float time, Action<Vector2> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return monoBehaviour.StartCoroutine(InterpolProcUnmanaged(time, onInterpolationComplete, interpolation, new InterpolatorVector2(startValue, completeValue, onInterpolationProc))); }

        public static Coroutine InterpolateUnmanaged(MonoBehaviour monoBehaviour, Vector3 startValue, Vector3 completeValue, float time, Action<Vector3> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return monoBehaviour.StartCoroutine(InterpolProcUnmanaged(time, onInterpolationComplete, interpolation, new InterpolatorVector3(startValue, completeValue, onInterpolationProc))); }

        public static Coroutine InterpolateUnmanaged(MonoBehaviour monoBehaviour, Vector4 startValue, Vector4 completeValue, float time, Action<Vector4> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return monoBehaviour.StartCoroutine(InterpolProcUnmanaged(time, onInterpolationComplete, interpolation, new InterpolatorVector4(startValue, completeValue, onInterpolationProc))); }

        public static Coroutine InterpolateUnmanaged(MonoBehaviour monoBehaviour, Color startValue, Color completeValue, float time, Action<Color> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return monoBehaviour.StartCoroutine(InterpolProcUnmanaged(time, onInterpolationComplete, interpolation, new InterpolatorColor(startValue, completeValue, onInterpolationProc))); }

        public Coroutine Interpolate(float startValue, float completeValue, float time, Action<float> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return m_MonoBehaviour.StartCoroutine(InterpolProc(time, onInterpolationComplete, interpolation, new InterpolatorFloat(startValue, completeValue, onInterpolationProc))); }

        public Coroutine Interpolate(Vector2 startValue, Vector2 completeValue, float time, Action<Vector2> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return m_MonoBehaviour.StartCoroutine(InterpolProc(time, onInterpolationComplete, interpolation, new InterpolatorVector2(startValue, completeValue, onInterpolationProc))); }

        public Coroutine Interpolate(Vector3 startValue, Vector3 completeValue, float time, Action<Vector3> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return m_MonoBehaviour.StartCoroutine(InterpolProc(time, onInterpolationComplete, interpolation, new InterpolatorVector3(startValue, completeValue, onInterpolationProc))); }

        public Coroutine Interpolate(Vector4 startValue, Vector4 completeValue, float time, Action<Vector4> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return m_MonoBehaviour.StartCoroutine(InterpolProc(time, onInterpolationComplete, interpolation, new InterpolatorVector4(startValue, completeValue, onInterpolationProc))); }

        public Coroutine Interpolate(Color startValue, Color completeValue, float time, Action<Color> onInterpolationProc, Action onInterpolationComplete = null, Interpolations.eInterpolation interpolation = Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR)
        { return m_MonoBehaviour.StartCoroutine(InterpolProc(time, onInterpolationComplete, interpolation, new InterpolatorColor(startValue, completeValue, onInterpolationProc))); }

        public static Coroutine TimerUnmanaged(MonoBehaviour monoBehaviour, float time, Action onComplete)
        { return monoBehaviour.StartCoroutine(InterpolProcUnmanaged(time, onComplete, Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR, new InterpolatorEmpty())); }

        public Coroutine Timer(float time, Action onComplete)
        { return m_MonoBehaviour.StartCoroutine(InterpolProc(time, onComplete, Interpolations.eInterpolation.INTERPOLATE_TYPE_LINEAR, new InterpolatorEmpty())); }

        public CoroutineInterpolator(MonoBehaviour monoBehaviour)
        { CoroutineInterpolator_w(monoBehaviour); }

        public void Skip()
        { Skip_w(); }

        public void ForcedComplete()
        { ForcedComplete_w(); }

        public void SetTimeScale(float timeScale)
        { SetTimeScale_w(timeScale); }

        public float GetTimeScale()
        { return GetTimeScale_w(); }

        public void SetUnscaledTime()
        { SetUnscaledTime_w(); }

        public void SetScaledTime()
        { SetScaledTime_w(); }

        public void SetFixedTime()
        { SetFixedTime_w(); }
    }
}
