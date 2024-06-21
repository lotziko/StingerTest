using System;
using UnityEngine;

namespace UnityEditor.VRInteraction
{
    [Serializable]
    public class VRInteractionEditorData : ScriptableObject
    {
        [SerializeField]
        GameObject _leftHandPrefab = null;
        [SerializeField]
        GameObject _rightHandPrefab = null;

        public GameObject LeftHandPrefab
        {
            get { return _leftHandPrefab; }
        }

        public GameObject RightHandPrefab
        {
            get { return _rightHandPrefab; }
        }

        private static VRInteractionEditorData _instance;

        public static VRInteractionEditorData Instance
        {
            get
            {
                VRInteractionEditorData instance = _instance;

                if (instance != null)
                    return instance;

                // Find existing data in the project
                instance = FileUtility.FindAssetOfType<VRInteractionEditorData>();
                
                if (instance != null)
                {
                    _instance = instance;
                    return instance;
                }

                // Create new data asset
                string path = FileUtility.GetLocalDataDirectory() + "/VRInteraction Editor Data.asset";
                instance = FileUtility.LoadRequired<VRInteractionEditorData>(path);
                _instance = instance;
                return instance;
            }
        }
    }
}