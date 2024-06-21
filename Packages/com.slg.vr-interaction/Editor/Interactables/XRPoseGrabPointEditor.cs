using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.VRInteraction;

namespace UnityEditor.VRInteraction
{
    [CustomEditor(typeof(XRPoseGrabPoint))]
    public class XRPoseGrabPointEditor : Editor
    {
        private SerializedProperty m_Poses;
        private MirrorAxis m_MirrorAxis;

        private void OnEnable()
        {
            m_Poses = serializedObject.FindProperty("m_Poses");

            XRPoseGrabPoint grabPoint = target as XRPoseGrabPoint;

            if (CanEditPoses())
            {
                bool isInitialization = false;

                if (!grabPoint.editorLeftHandPreview)
                {
                    grabPoint.editorLeftHandPreview = CreateHand(VRInteractionEditorData.Instance.LeftHandPrefab);
                    isInitialization = true;
                }

                if (!grabPoint.editorRightHandPreview)
                {
                    grabPoint.editorRightHandPreview = CreateHand(VRInteractionEditorData.Instance.RightHandPrefab);
                    isInitialization = true;
                }

                if (isInitialization)
                    ApplyPose(grabPoint.editorSelectedPose);
            }
        }

        public override void OnInspectorGUI()
        {
            XRPoseGrabPoint grabPoint = target as XRPoseGrabPoint;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
            }

            if (CanEditPoses())
            {
                int poseCount = m_Poses.arraySize;
                string[] names = new string[poseCount];
                for (int i = 0; i < poseCount; i++)
                {
                    SerializedProperty pose = m_Poses.GetArrayElementAtIndex(i);
                    names[i] = pose.objectReferenceValue == null ? "Empty" : pose.objectReferenceValue.name;
                }

                // Switch pose
                int selectedPose = grabPoint.editorSelectedPose;
                EditorGUI.BeginChangeCheck();
                selectedPose = GUILayout.Toolbar(selectedPose, names);
                if (EditorGUI.EndChangeCheck())
                {
                    grabPoint.editorSelectedPose = selectedPose;
                    ApplyPose(selectedPose);
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add pose..", GUILayout.ExpandWidth(false)))
                    {
                        int index = m_Poses.arraySize;
                        m_Poses.InsertArrayElementAtIndex(index);
                        m_Poses.GetArrayElementAtIndex(index).objectReferenceValue = null;
                    }

                    if (GUILayout.Button("Remove pose", GUILayout.ExpandWidth(false)))
                    {
                        m_Poses.DeleteArrayElementAtIndex(selectedPose);
                        selectedPose = Mathf.Clamp(selectedPose - 1, 0, m_Poses.arraySize - 1);
                        grabPoint.editorSelectedPose = selectedPose;
                        ApplyPose(selectedPose);
                    }
                }

                if (m_Poses.arraySize > 0)
                {
                    EditorGUILayout.PropertyField(m_Poses.GetArrayElementAtIndex(selectedPose), new GUIContent("Pose"));

                    // Draw pose tab
                    float halfWidth = EditorGUIUtility.currentViewWidth / 2f;

                    PosableHand leftHand = grabPoint.editorLeftHandPreview;
                    PosableHand rightHand = grabPoint.editorRightHandPreview;

                    m_MirrorAxis = (MirrorAxis)EditorGUILayout.EnumPopup("Mirror axis", m_MirrorAxis);

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Left hand", GUILayout.Width(halfWidth));
                        GUILayout.Label("Right hand", GUILayout.Width(halfWidth));
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(IsHandActive(leftHand) ? "Hide" : "Show", GUILayout.Width(halfWidth)))
                            ToggleHand(leftHand);

                        if (GUILayout.Button(IsHandActive(rightHand) ? "Hide" : "Show", GUILayout.Width(halfWidth)))
                            ToggleHand(rightHand);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Mirror from R", GUILayout.Width(halfWidth)))
                            MirrorHand(rightHand, leftHand, m_MirrorAxis);

                        if (GUILayout.Button("Mirror from L", GUILayout.Width(halfWidth)))
                            MirrorHand(leftHand, rightHand, m_MirrorAxis);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Reset", GUILayout.Width(halfWidth)))
                            ResetHand(leftHand);

                        if (GUILayout.Button("Reset", GUILayout.Width(halfWidth)))
                            ResetHand(rightHand);
                    }

                    if (GUILayout.Button("Save"))
                        SavePose();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Poses can be only edited in prefab mode.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private PosableHand CreateHand(GameObject prefab)
        {
            GameObject hand = (GameObject)PrefabUtility.InstantiatePrefab(prefab, (target as XRPoseGrabPoint).transform);

            hand.hideFlags = HideFlags.DontSave;
            hand.gameObject.SetActive(false);

            return hand.GetComponent<PosableHand>();
        }

        private bool IsHandActive(PosableHand hand)
        {
            return hand.gameObject.activeSelf;
        }

        private void ToggleHand(PosableHand hand)
        {
            Undo.RecordObject(hand.gameObject, "Hand toggled");
            hand.gameObject.SetActive(!hand.gameObject.activeSelf);
            SceneView.RepaintAll();
        }

        private void ResetHand(PosableHand hand)
        {
            Undo.RecordObject(hand.gameObject, "Hand reset");
            PosableHandUtility.ApplyDefaultPose(hand);
            SceneView.RepaintAll();
        }

        private void MirrorHand(PosableHand fromHand, PosableHand toHand, MirrorAxis axis)
        {
            Undo.RecordObject(toHand.gameObject, "Hand mirrored");
            PosableHandUtility.MirrorToHand(fromHand, toHand, axis);
            SceneView.RepaintAll();
        }

        private void ApplyPose(int index)
        {
            XRPoseGrabPoint grabPoint = target as XRPoseGrabPoint;

            if (index >= grabPoint.poses.Count)
                return;

            HandPose pose = grabPoint.poses[index];
            if (pose == null)
                return;

            PosableHandUtility.ApplyPose(grabPoint.editorLeftHandPreview, pose);
            PosableHandUtility.ApplyPose(grabPoint.editorRightHandPreview, pose);
        }

        private void SavePose()
        {
            XRPoseGrabPoint grabPoint = target as XRPoseGrabPoint;
            HandPose pose = grabPoint.poses[grabPoint.editorSelectedPose];

            EditorUtility.SetDirty(pose);
            PosableHandUtility.ApplyPoseInfo(grabPoint.editorLeftHandPreview, pose.LeftHandInfo);
            PosableHandUtility.ApplyPoseInfo(grabPoint.editorRightHandPreview, pose.RightHandInfo);
        }

        private bool CanEditPoses()
        {
            return PrefabStageUtility.GetCurrentPrefabStage();
        }
    }
}