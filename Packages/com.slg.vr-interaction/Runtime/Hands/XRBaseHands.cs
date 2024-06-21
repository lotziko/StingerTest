using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.VRInteraction
{
    [RequireComponent(typeof(Rigidbody))]
    public class XRBaseHands : MonoBehaviour
    {
        [Header("Hand options")]
        [SerializeField] protected Transform _handAnchor;
        [SerializeField] protected PosableHand _posableHand;
        [SerializeField] protected XRPalmInteractor _interactor;
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected Transform _palmTransform;
        [SerializeField] protected JointSettings _grabJointSettings;

        private GrabbingData m_GrabbingData;

        private void Start()
        {
            AddListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            _interactor.selectEntered.AddListener(OnSelectEntered);
            _interactor.selectExited.AddListener(OnSelectExited);
        }

        private void RemoveListeners()
        {
            _interactor.selectEntered.RemoveListener(OnSelectEntered);
            _interactor.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_GrabbingData != null)
                return;

            StartCoroutine(SelectEnteredDelayedCoroutine(args));
        }

        private IEnumerator SelectEnteredDelayedCoroutine(SelectEnterEventArgs args)
        {
            yield return null;

            if (args.interactableObject is IXRGrabbableInteractable interactable)
            {
                if (interactable.TryGetGrabPoint(args.interactorObject.GetAttachTransform(interactable).position, out XRGrabPoint grabPoint))
                {
                    interactable.isLocked = true;

                    Hand hand = _posableHand.hand;

                    GameObject grabTarget = new GameObject("Grab target");
                    grabTarget.transform.SetParent(grabPoint.transform);
                    grabTarget.transform.SetPositionAndRotation(grabPoint.GetAttachmentPosition(_posableHand), grabPoint.GetAttachmentRotation(_posableHand));

                    Vector3 previousPosition = transform.position;
                    Quaternion previousRotation = transform.rotation;

                    Rigidbody interactableRigidbody = interactable.rigidbody;
                    Transform interactableTransform = interactable.transform;

                    if (interactable.CanAttract())
                    {
                        // Place attract target
                        // TODO Calculate somehow position/rotation for attractTarget
                        _rigidbody.detectCollisions = false;
                        transform.position = grabTarget.transform.position;
                        transform.rotation = grabTarget.transform.rotation;

                        GameObject attractTarget = new GameObject("Attract target");
                        attractTarget.transform.SetParent(transform);
                        attractTarget.transform.SetPositionAndRotation(interactableTransform.position, interactableTransform.rotation);

                        transform.position = previousPosition;
                        transform.rotation = previousRotation;
                        _rigidbody.detectCollisions = true;

                        // Wait for hand attachment
                        float attachmentTime = 0.2f;
                        _posableHand.SetOverrideSkeletonSource(grabPoint, attachmentTime);
                        interactableRigidbody.useGravity = false;
                        yield return CoroutineInterpolator.InterpolateUnmanaged(this, 0f, 1f, attachmentTime,
                            (float t) =>
                            {
                                interactableTransform.position = Vector3.Lerp(interactableTransform.position, attractTarget.transform.position, t);
                                interactableRigidbody.rotation = Quaternion.Slerp(interactableTransform.rotation, attractTarget.transform.rotation, t);
                            });
                        interactableRigidbody.useGravity = true;
                        Destroy(attractTarget);

                        if (_interactor.firstInteractableSelected == null)
                        {
                            interactable.isLocked = false;
                            yield break;
                        }
                    }
                    else
                    {
                        _posableHand.SetOverrideSkeletonSource(grabPoint, 0.15f);
                    }

                    // Create joint
                    {
                        // TODO Animate
                        // TODO Try calculating connection anchor instead of changing hand position/rotation

                        _rigidbody.detectCollisions = false;
                        transform.position = grabTarget.transform.position;
                        transform.rotation = grabTarget.transform.rotation;

                        (ConfigurableJoint, JointSettings) jointData = CreateInteractableJoint(args.interactableObject.transform);
                        m_GrabbingData = new GrabbingData()
                        {
                            HandTarget = grabTarget,
                            GrabTransform = grabPoint.transform,
                            Joint = jointData.Item1,
                            Settings = jointData.Item2
                        };

                        transform.position = previousPosition;
                        transform.rotation = previousRotation;
                        _rigidbody.detectCollisions = true;
                    }

                    // TODO Need to check angle between hand and grab target and then attach hand
                    // Objects with poses rotate to hand and then grab, objects without poses move hand to them and then grab
                    _posableHand.transform.SetParent(grabTarget.transform, false);
                    interactable.isLocked = false;
                }
            }
            else
            {
                // TODO Dynamic attachment
            }
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (m_GrabbingData == null)
                return;

            if (m_GrabbingData.GrabTransform && m_GrabbingData.GrabTransform.TryGetComponent(out XRGrabPoint grabPoint))
            {
                _posableHand.transform.SetParent(transform, false);
                _posableHand.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                _posableHand.SetOverrideSkeletonSource(null, 0.15f);

                Destroy(m_GrabbingData.HandTarget);
                Destroy(m_GrabbingData.Joint);
                m_GrabbingData = null;
            }
        }

        private (ConfigurableJoint, JointSettings) CreateInteractableJoint(Transform interactableTransform)
        {
            ConfigurableJoint joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = interactableTransform.GetComponentInParent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = true;
            joint.anchor = Vector3.zero;
            JointSettings settings = null;
            if (interactableTransform.TryGetComponent(out XRGrabbableInteractable interactable) && interactable.customJointSettings)
                settings = interactable.customJointSettings;
            else
                settings = _grabJointSettings;
            settings.ApplySettings(joint);
            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = 0.01f;
            return (joint, settings);
        }

        private class GrabbingData
        {
            public GameObject HandTarget { get; set; }
            public Transform GrabTransform { get; set; }
            public ConfigurableJoint Joint { get; set; }
            public JointSettings Settings { get; set; }
        }
    }
}