namespace NeoCambion.Unity.PersistentUID
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    using NeoCambion.Unity.Editor;
#endif

    public class ObjectUID : PersistentUID
    {

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ObjectUID))]
    public class ObjectUIDEditor : Editor
    {
        ObjectUID targ { get { return target as ObjectUID; } }
        Rect rect;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PersistentUID.displayAsHex = EditorGUILayout.Toggle("Display UID in Hex", PersistentUID.displayAsHex);
            if (PersistentUID.displayAsHex)
            {
                rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), new GUIContent("UID"));
                EditorGUI.SelectableLabel(rect, targ.hexUID);
            }
            else
            {
                rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), new GUIContent("UID"));
                EditorGUI.SelectableLabel(rect, targ.UID.ToString());
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}