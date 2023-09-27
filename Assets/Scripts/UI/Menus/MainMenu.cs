using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using NeoCambion;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class MainMenu : GenericMenu
{
    public TMP_Text startGameButtonText;

    protected override void Initialise()
    {
        startGameButtonText.text = GameDataStorage.CheckForActiveRun() ? "RESUME GAME" : "START NEW GAME";
        base.Initialise();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MainMenu))]
public class MainMenuEditor : UIObjectSetContainerEditor
{
    public new static void Header() => TypeHeader("Main Menu", 14);

    public new static void DrawInspector(InheritEditor editor)
    {
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("startGameButtonText"), new GUIContent("\"Start Game\" Text"));
    }
}
#endif
