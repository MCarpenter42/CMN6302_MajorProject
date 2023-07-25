namespace NeoCambion.Unity.PersistentUID
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEditor;

    using NeoCambion.Unity.Editor;

    public class PrefabUID : PersistentUID
    {

    }

    [CustomEditor(typeof(PrefabUID))]
    public class PrefabUIDEditor : Editor
    {
        PrefabUID targ { get { return target as PrefabUID; } }
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
}