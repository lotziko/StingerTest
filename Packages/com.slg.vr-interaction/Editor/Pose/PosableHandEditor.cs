using UnityEngine;
using UnityEngine.VRInteraction;

namespace UnityEditor.VRInteraction
{
    [CustomEditor(typeof(PosableHand))]
    public class PosableHandEditor : Editor
    {
        //private SerializedProperty m_Hand;
        //private SerializedProperty m_Wrist;
        //private SerializedProperty m_FingerRoots;
        
        private Transform m_ActiveJoint;

        //private void OnEnable()
        //{
        //    m_Hand = serializedObject.FindProperty("m_Hand");
        //    m_Wrist = serializedObject.FindProperty("m_Wrist");
        //    m_FingerRoots = serializedObject.FindProperty("m_FingerRoots");
        //}

        private void OnSceneGUI()
        {
            if (Application.isPlaying)
                return;

            DrawHandles();
            DrawActiveHandle();
        }

        //public override void OnInspectorGUI()
        //{
        //    using (new EditorGUI.DisabledScope(true))
        //    {
        //        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
        //    }

        //    EditorGUILayout.PropertyField(m_Hand);

        //    EditorGUILayout.PropertyField(m_Wrist);

        //    m_FingerRoots.isExpanded = EditorGUILayout.Foldout(m_FingerRoots.isExpanded, "Fingers");
        //    if (m_FingerRoots.isExpanded)
        //    {
        //        EditorGUI.indentLevel++;
        //        EditorGUILayout.PropertyField(m_FingerRoots.GetArrayElementAtIndex(0), new GUIContent("Thumb"));
        //        EditorGUILayout.PropertyField(m_FingerRoots.GetArrayElementAtIndex(1), new GUIContent("Index"));
        //        EditorGUILayout.PropertyField(m_FingerRoots.GetArrayElementAtIndex(2), new GUIContent("Middle"));
        //        EditorGUILayout.PropertyField(m_FingerRoots.GetArrayElementAtIndex(3), new GUIContent("Ring"));
        //        EditorGUILayout.PropertyField(m_FingerRoots.GetArrayElementAtIndex(4), new GUIContent("Pinky"));
        //        EditorGUI.indentLevel--;
        //    }

        //    serializedObject.ApplyModifiedProperties();
        //}

        private void DrawHandles()
        {
            Transform[] joints = (target as PosableHand).joints;
            if (joints == null)
                return;

            foreach (Transform joint in joints)
            {
                if (joint == null || joint.childCount == 0)
                    continue;

                bool isActive = Handles.Button(joint.position, joint.rotation, 0.01f, 0.005f, Handles.SphereHandleCap);
                if (isActive)
                {
                    m_ActiveJoint = joint;
                    return;
                }
            }
        }

        private void DrawActiveHandle()
        {
            if (m_ActiveJoint != null)
            {
                Quaternion currentRotation = m_ActiveJoint.rotation;

                EditorGUI.BeginChangeCheck();
                Quaternion rotation = Handles.RotationHandle(currentRotation, m_ActiveJoint.position);
                if (EditorGUI.EndChangeCheck())
                {
                    m_ActiveJoint.rotation = rotation;
                    Undo.RegisterCompleteObjectUndo(m_ActiveJoint, "Changed Joint Rotation");
                }
            }
        }
    }
}
