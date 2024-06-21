using UnityEngine.InputSystem.XR;

namespace UnityEngine.VRInteraction
{
    public class InterpolatedTrackedPoseDriver : TrackedPoseDriver
    {
        [SerializeField] private Transform m_Target;

        private Vector3 m_PreviousPosition;

        //private void Update()
        //{
        //    Physics.autoSimulation = false;
        //    Physics.Simulate(Time.deltaTime);
        //}

        //private void FixedUpdate()
        //{
        //    m_PreviousPosition = m_Target.position;
        //}

        //protected override void PerformUpdate()
        //{
        //    base.PerformUpdate();
        //    float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        //    m_Target.SetPositionAndRotation(Vector3.Lerp(m_PreviousPosition, transform.position, t), transform.rotation);
        //}
    }
}
