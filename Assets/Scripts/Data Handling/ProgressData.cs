using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using NeoCambion.Unity.Editor;

public class ProgressData : Core
{
    public int stageReached;

    public void Save()
    {

    }

    public void Load()
    {
        
    }
}

[CustomEditor(typeof(ProgressData))]
[CanEditMultipleObjects]
public class ProgressDataEditor : Editor
{
    ControlsHandler targ { get { return target as ControlsHandler; } }
    Rect elementRect;
    GUIContent label = new GUIContent();

    bool disableAll = false;

    public override void OnInspectorGUI()
    {
        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding);
        {
            EditorElements.RequiredComponent("Necessary for user input functionality");
        }
        EditorElements.EndHorizVert();
    }
}
