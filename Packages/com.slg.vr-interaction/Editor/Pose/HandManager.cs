using System;
using UnityEngine;
using UnityEngine.VRInteraction;

namespace UnityEditor.VRInteraction
{
    public class HandManager
    {
        static readonly Lazy<HandManager> _instance = new Lazy<HandManager>(() => new HandManager());

        //public static HandManager Instance => _instance.Value;
        
        private PosableHand _leftHandPreview;
        public PosableHand LeftHandPreview
        {
            get
            {
                if (_leftHandPreview == null)
                    _leftHandPreview = CreateHand(VRInteractionEditorData.Instance.LeftHandPrefab);
                return _leftHandPreview;
            }
        }

        private PosableHand _rightHandPreview;
        public PosableHand RightHandPreview
        {
            get
            {
                if (_rightHandPreview == null)
                    _rightHandPreview = CreateHand(VRInteractionEditorData.Instance.RightHandPrefab);
                return _rightHandPreview;
            }
        }

        public void AssignHandsToParent(Transform parent)
        {
            LeftHandPreview.transform.parent = parent;
            RightHandPreview.transform.parent = parent;
        }

        public void ApplyPose(HandPose pose)
        {
            PosableHandUtility.ApplyPose(LeftHandPreview, pose);
            PosableHandUtility.ApplyPose(RightHandPreview, pose);
        }

        private PosableHand CreateHand(GameObject prefab)
        {
            GameObject hand = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            hand.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            hand.gameObject.SetActive(false);

            return hand.GetComponent<PosableHand>();
        }

        public void SavePose(HandPose pose)
        {
            EditorUtility.SetDirty(pose);
            PosableHandUtility.ApplyPoseInfo(LeftHandPreview, pose.LeftHandInfo);
            PosableHandUtility.ApplyPoseInfo(RightHandPreview, pose.RightHandInfo);
        }

        //private Transform _poseHelper;
        //private GameObject _leftPreviewPrefab;
        //private GameObject _rightPreviewPrefab;

        //private PosableHand _leftPreview;
        //private PosableHand _rightPreview;
        //private HandPose _lastPose;

        //public bool HandsExist => _leftPreview && _rightPreview;

        //public PosableHand LeftPreview
        //{ get { return _leftPreview; } }

        //public PosableHand RightPreview
        //{ get { return _rightPreview; } }

        //public HandManager(Transform poseHelper,GameObject leftPreviewPrefab, GameObject rightPreviewPrefab)
        //{
        //    _poseHelper = poseHelper;
        //    _leftPreviewPrefab = leftPreviewPrefab;
        //    _rightPreviewPrefab = rightPreviewPrefab;

        //    CreateHands();
        //}

        //public void Dispose()
        //{
        //    DestroyHands();
        //}

        //public void AssignHandsToParent(Transform parent, HandPose pose)
        //{
        //    _leftPreview.transform.parent = parent;
        //    _rightPreview.transform.parent = parent;

        //    PosableHandUtility.ApplyPose(_leftPreview, pose);
        //    PosableHandUtility.ApplyPose(_rightPreview, pose);
        //    _lastPose = pose;
        //}

        //public void SavePose()
        //{
        //    EditorUtility.SetDirty(_lastPose);
        //    PosableHandUtility.ApplyPoseInfo(_leftPreview, _lastPose.LeftHandInfo);
        //    PosableHandUtility.ApplyPoseInfo(_rightPreview, _lastPose.RightHandInfo);
        //}

        //private void CreateHands()
        //{
        //    if (!_leftPreviewPrefab || !_rightPreviewPrefab || !_leftPreviewPrefab.GetComponent<PosableHand>() || !_rightPreviewPrefab.GetComponent<PosableHand>())
        //    {
        //        Debug.LogError("Can't create preview hands. Assign valid preview hand prefabs.");
        //        return;
        //    }

        //    if (!_leftPreview)
        //    {
        //        _leftPreview = CreateHand(_leftPreviewPrefab);
        //        _rightPreview = CreateHand(_rightPreviewPrefab);
        //    }
        //}

        //private void DestroyHands()
        //{
        //    if (_leftPreview)
        //        Object.DestroyImmediate(_leftPreview.gameObject);
        //    if (_rightPreview)
        //        Object.DestroyImmediate(_rightPreview.gameObject);
        //}

        //private PosableHand CreateHand(GameObject prefab)
        //{
        //    GameObject hand = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _poseHelper);

        //    hand.hideFlags = HideFlags.DontSave;
        //    hand.gameObject.SetActive(false);

        //    return hand.GetComponent<PosableHand>();
        //}
    }
}