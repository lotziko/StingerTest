using System;

namespace UnityEngine.VRInteraction
{
    [CreateAssetMenu(menuName = "XR/Joint Settings", fileName = "JointSettings")]
    public class JointSettings : ScriptableObject
    {
        public float DamperDelayPower = 15f;

        public RotationDriveMode RotationDriveMode = RotationDriveMode.Slerp;
        
        [Header("Linear Drives")]
        [Tooltip("If true then X,Y,Z will all use X values.")]
        public bool XMaster = true;
        public JointDriveSettings XDrive;
        [DrawIf("XMaster", false, ComparisonType.Equals)]
        public JointDriveSettings YDrive;
        [DrawIf("XMaster", false, ComparisonType.Equals)]
        public JointDriveSettings ZDrive;
        
        [Header("Angular Drives")]
        public AngularJointDriveSettings SlerpDrive;
        [DrawIf("RotationDriveMode", RotationDriveMode.XYAndZ, ComparisonType.Equals)]
        public AngularJointDriveSettings AngularXDrive;
        [DrawIf("RotationDriveMode", RotationDriveMode.XYAndZ, ComparisonType.Equals)]
        public AngularJointDriveSettings AngularYZDrive;
        
        [Header("Linear Limits")]
        public SoftJointLimitSettings LinearLimit;
        public SoftJointLimitSpringSettings LinearLimitSpring;

        public ConfigurableJointMotion XMotion = ConfigurableJointMotion.Free;
        public ConfigurableJointMotion YMotion = ConfigurableJointMotion.Free;
        public ConfigurableJointMotion ZMotion = ConfigurableJointMotion.Free;

        [Header("Angular Limits")]
        public ConfigurableJointMotion AngularXMotion = ConfigurableJointMotion.Free;
        public ConfigurableJointMotion AngularYMotion = ConfigurableJointMotion.Free;
        public ConfigurableJointMotion AngularZMotion = ConfigurableJointMotion.Free;

        public SoftJointLimitSettings LowAngularXLimit;
        public SoftJointLimitSettings HighAngularXLimit;
        public SoftJointLimitSpringSettings AngularXLimitSpring;

        public SoftJointLimitSettings AngularYLimit;
        public SoftJointLimitSettings AngularZLimit;
        public SoftJointLimitSpringSettings AngularYZLimitSpring;

        [Header("Other Settings")]
        public bool EnableCollision = false;

        public bool EnablePreprocessing = true;

        public float BreakForce = float.PositiveInfinity;
        public float BreakTorque = float.PositiveInfinity;

        public float MassScale = 1f;
        public float ConnectedMassScale = 1f;

        public void ApplySettings(ConfigurableJoint joint)
        {
            joint.xMotion = XMotion;
            joint.yMotion = YMotion;
            joint.zMotion = ZMotion;

            joint.angularXMotion = AngularXMotion;
            joint.angularYMotion = AngularYMotion;
            joint.angularZMotion = AngularZMotion;

            joint.linearLimitSpring = LinearLimitSpring.CreateSpring();
            joint.linearLimit = LinearLimit.CreateJointLimit();

            joint.angularXLimitSpring = AngularXLimitSpring.CreateSpring();
            joint.lowAngularXLimit = LowAngularXLimit.CreateJointLimit();
            joint.highAngularXLimit = HighAngularXLimit.CreateJointLimit();

            joint.angularYZLimitSpring = AngularYZLimitSpring.CreateSpring();
            joint.angularYLimit = AngularYLimit.CreateJointLimit();
            joint.angularZLimit = AngularZLimit.CreateJointLimit();

            joint.xDrive = XDrive.CreateJointDrive();
            if (XMaster)
            {
                joint.yDrive = joint.xDrive;
                joint.zDrive = joint.xDrive;
            }
            else
            {
                joint.yDrive = YDrive.CreateJointDrive();
                joint.zDrive = ZDrive.CreateJointDrive();
            }
            
            joint.rotationDriveMode = RotationDriveMode;

            if (RotationDriveMode == RotationDriveMode.Slerp)
            {
                joint.slerpDrive = SlerpDrive.CreateJointDrive();
            }
            else
            {
                joint.angularXDrive = AngularXDrive.CreateJointDrive();
                joint.angularYZDrive = AngularYZDrive.CreateJointDrive();
            }

            joint.enableCollision = EnableCollision;
            joint.enablePreprocessing = EnablePreprocessing;
            joint.breakForce = BreakForce;
            joint.breakTorque = BreakTorque;
            joint.massScale = MassScale;
            joint.connectedMassScale = ConnectedMassScale;
        }
    }

    [Serializable]
    public class JointDriveSettings
    {
        public float Spring = 360000;
        public float Damper = 120000;
        public float MaxForce = 1200;

        public JointDrive CreateJointDrive()
        {
            var drive = new JointDrive();
            drive.positionSpring = Spring;
            drive.positionDamper = Damper;
            drive.maximumForce = MaxForce;
            return drive;
        }
    }

    [Serializable]
    public class AngularJointDriveSettings
    {
        public float Spring = 40000;
        public float Damper = 1000;
        public float MaxForce = 150;

        public JointDrive CreateJointDrive()
        {
            var drive = new JointDrive();
            drive.positionSpring = Spring;
            drive.positionDamper = Damper;
            drive.maximumForce = MaxForce;
            return drive;
        }
    }

    [Serializable]
    public class SoftJointLimitSettings
    {
        public float Limit;
        public float Bounciness;
        public float ContactDistance;

        public SoftJointLimit CreateJointLimit()
        {
            var limit = new SoftJointLimit();

            limit.limit = Limit;
            limit.bounciness = Bounciness;
            limit.contactDistance = ContactDistance;
            return limit;
        }
    }

    [Serializable]
    public class SoftJointLimitSpringSettings
    {
        public float Spring;
        public float Damper;

        public SoftJointLimitSpring CreateSpring()
        {
            var spring = new SoftJointLimitSpring();
            spring.spring = Spring;
            spring.damper = Damper;
            return spring;
        }
    }
}
