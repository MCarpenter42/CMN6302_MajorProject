using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteInEditMode]
public class ShadowText : UIObject
{
    [Header("Component Objects")]
    public TextMeshProUGUI mainText;
    public RectTransform textTransform { get { return mainText.gameObject.GetOrAddComponent<RectTransform>(); } }
    public TextMeshProUGUI shadowText;
    public RectTransform shadowTransform { get { return shadowText.gameObject.GetOrAddComponent<RectTransform>(); } }

    [Header("Settings")]
    public Vector2 shadowOffset;
    public Color shadowColor = Color.black;

    private static Vector2 anchorMin = new Vector2(0, 0);
    private static Vector2 anchorMax = new Vector2(1, 1);
    private Vector2 shadowOffsetMin { get { return shadowOffset; } }
    private Vector2 shadowOffsetMax { get { return shadowOffset; } }

    public new string Text
    {
        get { return TextMesh.text; }
        set { TextMesh.text = value; mainText.text = value; shadowText.text = value; }
    }

    public new Color Color
    {
        get { return TextColor; }
        set { TextColor = value; mainText.color = value; }
    }
    public Color ShadowColor
    {
        get { return shadowColor; }
        set { shadowColor = shadowText.color = value; }
    }

    public void UpdateComponents()
    {
        TextMesh.enabled = false;
        if (mainText != null && mainText.gameObject != gameObject)
        {
            textTransform.anchorMin = anchorMin;
            textTransform.anchorMax = anchorMax;
            textTransform.offsetMin = Vector2.zero;
            textTransform.offsetMax = Vector2.zero;
            textTransform.pivot = 0.5f * Vector2.one;

            mainText.text = TextMesh.text;
            mainText.font = TextMesh.font;
            mainText.material = TextMesh.material ?? TextMesh.defaultMaterial;
            //mainText.fontMaterial = TextMesh.fontMaterial;
            mainText.fontStyle = TextMesh.fontStyle;
            mainText.fontSize = TextMesh.fontSize;
            mainText.enableAutoSizing = TextMesh.enableAutoSizing;
            mainText.fontSizeMin = TextMesh.fontSizeMin;
            mainText.fontSizeMax = TextMesh.fontSizeMax;
            mainText.characterWidthAdjustment = TextMesh.characterWidthAdjustment;
            mainText.lineSpacingAdjustment = TextMesh.lineSpacingAdjustment;
            mainText.color = TextMesh.color;
            mainText.colorGradient = TextMesh.colorGradient;
            mainText.colorGradientPreset = TextMesh.colorGradientPreset;
            mainText.faceColor = TextMesh.faceColor;
            //mainText.outlineColor = TextMesh.outlineColor;
            mainText.overrideColorTags = TextMesh.overrideColorTags;
            mainText.characterSpacing = TextMesh.characterSpacing;
            mainText.wordSpacing = TextMesh.wordSpacing;
            mainText.lineSpacing = TextMesh.lineSpacing;
            mainText.paragraphSpacing = TextMesh.paragraphSpacing;
            mainText.alignment = TextMesh.alignment;
            mainText.verticalAlignment = TextMesh.verticalAlignment;
            mainText.enableWordWrapping = TextMesh.enableWordWrapping;
            mainText.overflowMode = TextMesh.overflowMode;
            mainText.horizontalMapping = TextMesh.horizontalMapping;
            mainText.verticalMapping = TextMesh.verticalMapping;
        }
        if (shadowText != null && shadowText.gameObject != gameObject)
        {
            shadowTransform.anchorMin = anchorMin;
            shadowTransform.anchorMax = anchorMax;
            shadowTransform.offsetMin = shadowOffsetMin;
            shadowTransform.offsetMax = shadowOffsetMax;
            textTransform.pivot = 0.5f * Vector2.one;

            shadowText.text = TextMesh.text;
            shadowText.font = TextMesh.font;
            shadowText.material = TextMesh.material ?? TextMesh.defaultMaterial;
            //shadowText.fontMaterial = TextMesh.fontMaterial;
            shadowText.fontStyle = TextMesh.fontStyle;
            shadowText.fontSize = TextMesh.fontSize;
            shadowText.enableAutoSizing = TextMesh.enableAutoSizing;
            shadowText.fontSizeMin = TextMesh.fontSizeMin;
            shadowText.fontSizeMax = TextMesh.fontSizeMax;
            shadowText.characterWidthAdjustment = TextMesh.characterWidthAdjustment;
            shadowText.lineSpacingAdjustment = TextMesh.lineSpacingAdjustment;
            shadowText.color = shadowColor; // <--
            shadowText.colorGradient = new VertexGradient(shadowColor); // <--
            shadowText.colorGradientPreset = null; // <--
            shadowText.faceColor = shadowColor; // <--
            //shadowText.outlineColor = Color.clear; // <--
            shadowText.overrideColorTags = false; // <--
            shadowText.characterSpacing = TextMesh.characterSpacing;
            shadowText.wordSpacing = TextMesh.wordSpacing;
            shadowText.lineSpacing = TextMesh.lineSpacing;
            shadowText.paragraphSpacing = TextMesh.paragraphSpacing;
            shadowText.alignment = TextMesh.alignment;
            shadowText.verticalAlignment = TextMesh.verticalAlignment;
            shadowText.enableWordWrapping = TextMesh.enableWordWrapping;
            shadowText.overflowMode = TextMesh.overflowMode;
            shadowText.horizontalMapping = TextMesh.horizontalMapping;
            shadowText.verticalMapping = TextMesh.verticalMapping;
        }
    }

    protected override void Initialise()
    {
        base.Initialise();
        UpdateComponents();
    }

    public override void OnShow()
    {
        base.OnShow();
        mainText.gameObject.SetActive(true);
        shadowText.gameObject.SetActive(true);
    }

    public override void OnHide()
    {
        base.OnHide();
        mainText.gameObject.SetActive(false);
        shadowText.gameObject.SetActive(false);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ShadowText))]
[CanEditMultipleObjects]
public class ShadowTextEditor : Editor
{
    ShadowText targ { get { return target as ShadowText; } }
    Rect rect, btnRect;
    float btnSize = 24;
    public override void OnInspectorGUI()
    {
        bool changes = false;
        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding);
        {
            rect = EditorElements.ControlRect(32);
            btnRect = new Rect(rect);
            rect.x += (rect.width - 32) / 2.0f;
            rect.width = 32;
            btnRect.x += btnRect.width - btnSize - 10;
            btnRect.width = btnSize;
            btnRect.y += (btnRect.height - btnSize) / 2.0f;
            btnRect.height = btnSize;
            EditorGUI.LabelField(rect, EditorElements.IconContentBuiltin("UnityEditor.SceneHierarchyWindow@2x", "Control Component"));
            EditorGUILayout.LabelField("Controller for other components and child objects", EditorStylesExtras.LabelStyle(TextAnchor.MiddleCenter));
            EditorGUILayout.Space(6);

            //TextMeshProUGUI mainText = EditorGUI.ObjectField(EditorElements.PrefixLabel(new GUIContent("Text")), targ.mainText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
            //if (mainText != targ.mainText) { targ.mainText = mainText; changes = true; }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainText"), new GUIContent("Text"));
            EditorGUILayout.Space(2);

            //TextMeshProUGUI shadowText = EditorGUI.ObjectField(EditorElements.PrefixLabel(new GUIContent("Shadow Text")), targ.shadowText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
            //if (shadowText != targ.shadowText) { targ.shadowText = shadowText; changes = true; }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowText"), new GUIContent("Shadow Text"));
            EditorGUILayout.Space(2);

            //Vector2 shadowOffset = EditorGUI.Vector2Field(EditorElements.ControlRect(), new GUIContent("Shadow Offset"), targ.shadowOffset);
            //if (shadowOffset != targ.shadowOffset) { targ.shadowOffset = shadowOffset; changes = true; }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowOffset"), new GUIContent("Shadow Offset"));
            EditorGUILayout.Space(2);

            //Color shadowColor = EditorGUI.ColorField(EditorElements.PrefixLabel(new GUIContent("Shadow Colour")), targ.shadowColor);
            //if (shadowColor != targ.shadowColor) { targ.shadowColor = shadowColor; changes = true; }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowColor"), new GUIContent("Shadow Colour"));
            EditorGUILayout.Space(6);

            if (EditorElements.IconButton(btnRect, "refresh", "Force update components") || changes)
                targ.UpdateComponents();
        }
        EditorElements.EndHorizVert();
        if (serializedObject.hasModifiedProperties)
            targ.UpdateComponents();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
