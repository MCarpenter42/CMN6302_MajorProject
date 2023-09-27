using UnityEngine;
using UnityEngine.UI;

using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ClearButtonCallbacks))]
public class ClearButtonCallbacksEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Italic,
        };
        EditorGUILayout.LabelField("Provides an additional function for built-in buttons.", style);
    }
}
#endif

public class ClearButtonCallbacks : MonoBehaviourExt
{
    private Button Button => GetComponent<Button>();
    public void Clear() => Button.onClick.RemoveAllListeners();
}
