using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.Interpolation;
using NeoCambion.IO;
using NeoCambion.IO.Unity;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Random.Unity;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
#if UNITY_EDITOR
using NeoCambion.Unity.Editor;
#endif
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;

public class ControlIcon : UIObject
{
    [SerializeField] Sprite fallback;

    [SerializeField] Sprite mouseAndKeyboard;
    public Sprite MouseAndKeyboard { get { return mouseAndKeyboard != null ? mouseAndKeyboard : fallback; } }
    [SerializeField] Sprite gamepadGeneric;
    public Sprite GamepadGeneric { get { return gamepadGeneric != null ? gamepadGeneric : fallback; } }
    [SerializeField] Sprite gamepadXbox;
    public Sprite GamepadXbox { get { return gamepadXbox != null ? gamepadXbox : GamepadGeneric; } }
    [SerializeField] Sprite gamepadPlaystation;
    public Sprite GamepadPlaystation { get { return gamepadPlaystation != null ? gamepadPlaystation : GamepadGeneric; } }
    [SerializeField] Sprite gamepadNintendo;
    public Sprite GamepadNintendo { get { return gamepadNintendo != null ? gamepadNintendo : GamepadGeneric; } }

    protected override void Initialise()
    {
        base.Initialise();
        UIManager.controlIcons.Add(this);
        ControlSchemeUpdate(ControlsHandler.activeControlScheme, ControlsHandler.activeGamepadType);
    }

    public void ControlSchemeUpdate(ControlScheme scheme, GamepadType gamepad = GamepadType.None)
    {
        switch (scheme)
        {
            default:
                Sprite = fallback;
                break;
                
            case ControlScheme.MouseAndKeyboard:
                Sprite = MouseAndKeyboard;
                break;
                
            case ControlScheme.Gamepad:
                Sprite = gamepad switch
                {
                    GamepadType.Xbox => GamepadXbox,
                    GamepadType.PlayStation => GamepadPlaystation,
                    GamepadType.Nintendo => GamepadNintendo,
                    _ => GamepadGeneric,
                };
                break;
        }
    }

    public override void OnShow()
    {
        base.OnShow();

    }

    public void UpdateEditorSprite()
    {
        if (!Application.isPlaying)
        {
            if (mouseAndKeyboard != null)
                Sprite = mouseAndKeyboard;
            else if (gamepadGeneric != null)
                Sprite = gamepadGeneric;
            else if (gamepadXbox != null)
                Sprite = gamepadXbox;
            else if (gamepadPlaystation != null)
                Sprite = gamepadPlaystation;
            else if (gamepadNintendo != null)
                Sprite = gamepadNintendo;
            else
                Sprite = fallback;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ControlIcon))]
public class ControlIconEditor : Editor
{
    private ControlIcon targ { get { return target as ControlIcon; } }
    private GUIContent label = new GUIContent();
    private GUIStyle boundingH => EditorStylesExtras.noMarginsNoPadding;
    private GUIStyle boundingV => EditorStyles.inspectorDefaultMargins;

    public override void OnInspectorGUI()
    {
        label.tooltip = null;
        EditorElements.BeginHorizVert(boundingH, boundingV);
        {
            label.text = "Visible On Start";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("visibleOnStart"), label);
            EditorGUILayout.Space(4);

            label.text = "Fallback";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fallback"), label);
            label.text = "Mouse & Keyboard";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mouseAndKeyboard"), label);
            label.text = "Gamepad (Generic)";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gamepadGeneric"), label);
            label.text = "Gamepad (Xbox)";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gamepadXbox"), label);
            label.text = "Gamepad (PlayStation)";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gamepadPlaystation"), label);
            label.text = "Gamepad (Nintendo)";
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gamepadNintendo"), label);
        }
        EditorElements.EndHorizVert();
        if (serializedObject.ApplyModifiedProperties())
            targ.UpdateEditorSprite();
    }
}
#endif
