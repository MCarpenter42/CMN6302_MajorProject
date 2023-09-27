using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

using NeoCambion.Unity.Editor;
#endif

public class SceneAttributes : Core
{
    [SerializeField] ControlState initialControlState;
    public ControlState InitialControlState => initialControlState;

    [SerializeField] bool gameplayScene;
    public bool GameplayScene => gameplayScene;

    [SerializeField] bool stageProgression;
    public bool StageProgression => GameplayScene && stageProgression;
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneAttributes))]
public class SceneAttributesEditor : Editor
{
    GUIContent label = new GUIContent();
    public override void OnInspectorGUI()
    {
        label.tooltip = null;
        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding, EditorStyles.inspectorFullWidthMargins);
        {
            label.text = "Initial Control State";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialControlState"), label);
            label.text = "Is Gameplay Scene";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gameplayScene"), label);
            label.text = "Do Stage Progression";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stageProgression"), label);
        }
        EditorElements.EndHorizVert();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
