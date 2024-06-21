
namespace UnityEngine.VRInteraction
{
    [SerializeField]
    [CreateAssetMenu(fileName = "NewHandPose")]
    public class HandPose : ScriptableObject
    {
        public HandInfo LeftHandInfo = new HandInfo();
        public HandInfo RightHandInfo = new HandInfo();

        public HandInfo GetInfo(Hand hand)
        {
            if (hand == Hand.Left)
                return LeftHandInfo;
            return RightHandInfo;
        }
    }
}
