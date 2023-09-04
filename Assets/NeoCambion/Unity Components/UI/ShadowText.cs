using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

using NeoCambion.Unity;
using NeoCambion.Unity.Editor;

[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteInEditMode]
public class ShadowText : UIObject
{
    [Header("Component Objects")]
    public TextMeshProUGUI mainText;
    public RectTransform textTransform { get { return mainText.gameObject.GetOrAddComponent<RectTransform>(); } }
    public TextMeshProUGUI shadowText;
    public RectTransform shadowTransform { get { return shadowText.gameObject.GetOrAddComponent<RectTransform>(); } }
    public TextMeshProUGUI TMP { get { return GetComponent<TextMeshProUGUI>(); } }

    [Header("Settings")]
    public Vector2 shadowOffset;
    public Color shadowColor = Color.black;

    private static Vector2 anchorMin = new Vector2(0, 0);
    private static Vector2 anchorMax = new Vector2(1, 1);
    private Vector2 shadowOffsetMin { get { return shadowOffset; } }
    private Vector2 shadowOffsetMax { get { return shadowOffset; } }

    public void UpdateComponents()
    {
        TMP.enabled = false;
        if (mainText != null && mainText.gameObject != gameObject)
        {
            textTransform.anchorMin = anchorMin;
            textTransform.anchorMax = anchorMax;
            textTransform.offsetMin = Vector2.zero;
            textTransform.offsetMax = Vector2.zero;
            textTransform.pivot = 0.5f * Vector2.one;

            mainText.text = TMP.text;
            mainText.font = TMP.font;
            mainText.material = TMP.material ?? TMP.defaultMaterial;
            //mainText.fontMaterial = TMP.fontMaterial;
            mainText.fontStyle = TMP.fontStyle;
            mainText.fontSize = TMP.fontSize;
            mainText.enableAutoSizing = TMP.enableAutoSizing;
            mainText.fontSizeMin = TMP.fontSizeMin;
            mainText.fontSizeMax = TMP.fontSizeMax;
            mainText.characterWidthAdjustment = TMP.characterWidthAdjustment;
            mainText.lineSpacingAdjustment = TMP.lineSpacingAdjustment;
            mainText.color = TMP.color;
            mainText.colorGradient = TMP.colorGradient;
            mainText.colorGradientPreset = TMP.colorGradientPreset;
            mainText.faceColor = TMP.faceColor;
            //mainText.outlineColor = TMP.outlineColor;
            mainText.overrideColorTags = TMP.overrideColorTags;
            mainText.characterSpacing = TMP.characterSpacing;
            mainText.wordSpacing = TMP.wordSpacing;
            mainText.lineSpacing = TMP.lineSpacing;
            mainText.paragraphSpacing = TMP.paragraphSpacing;
            mainText.alignment = TMP.alignment;
            mainText.verticalAlignment = TMP.verticalAlignment;
            mainText.enableWordWrapping = TMP.enableWordWrapping;
            mainText.overflowMode = TMP.overflowMode;
            mainText.horizontalMapping = TMP.horizontalMapping;
            mainText.verticalMapping = TMP.verticalMapping;
        }
        if (shadowText != null && shadowText.gameObject != gameObject)
        {
            shadowTransform.anchorMin = anchorMin;
            shadowTransform.anchorMax = anchorMax;
            shadowTransform.offsetMin = shadowOffsetMin;
            shadowTransform.offsetMax = shadowOffsetMax;
            textTransform.pivot = 0.5f * Vector2.one;

            shadowText.text = TMP.text;
            shadowText.font = TMP.font;
            shadowText.material = TMP.material ?? TMP.defaultMaterial;
            //shadowText.fontMaterial = TMP.fontMaterial;
            shadowText.fontStyle = TMP.fontStyle;
            shadowText.fontSize = TMP.fontSize;
            shadowText.enableAutoSizing = TMP.enableAutoSizing;
            shadowText.fontSizeMin = TMP.fontSizeMin;
            shadowText.fontSizeMax = TMP.fontSizeMax;
            shadowText.characterWidthAdjustment = TMP.characterWidthAdjustment;
            shadowText.lineSpacingAdjustment = TMP.lineSpacingAdjustment;
            shadowText.color = shadowColor; // <--
            shadowText.colorGradient = new VertexGradient(shadowColor); // <--
            shadowText.colorGradientPreset = null; // <--
            shadowText.faceColor = shadowColor; // <--
            //shadowText.outlineColor = Color.clear; // <--
            shadowText.overrideColorTags = false; // <--
            shadowText.characterSpacing = TMP.characterSpacing;
            shadowText.wordSpacing = TMP.wordSpacing;
            shadowText.lineSpacing = TMP.lineSpacing;
            shadowText.paragraphSpacing = TMP.paragraphSpacing;
            shadowText.alignment = TMP.alignment;
            shadowText.verticalAlignment = TMP.verticalAlignment;
            shadowText.enableWordWrapping = TMP.enableWordWrapping;
            shadowText.overflowMode = TMP.overflowMode;
            shadowText.horizontalMapping = TMP.horizontalMapping;
            shadowText.verticalMapping = TMP.verticalMapping;
        }
    }
}

[CustomEditor(typeof(ShadowText))]
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

            TextMeshProUGUI mainText = EditorGUI.ObjectField(EditorElements.PrefixLabel(new GUIContent("Text")), targ.mainText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
            if (mainText != targ.mainText) { targ.mainText = mainText; changes = true; }
            EditorGUILayout.Space(2);

            TextMeshProUGUI shadowText = EditorGUI.ObjectField(EditorElements.PrefixLabel(new GUIContent("Shadow Text")), targ.shadowText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
            if (shadowText != targ.shadowText) { targ.shadowText = shadowText; changes = true; }
            EditorGUILayout.Space(2);

            Vector2 shadowOffset = EditorGUI.Vector2Field(EditorElements.ControlRect(), new GUIContent("Shadow Offset"), targ.shadowOffset);
            if (shadowOffset != targ.shadowOffset) { targ.shadowOffset = shadowOffset; changes = true; }
            EditorGUILayout.Space(2);

            Color shadowColor = EditorGUI.ColorField(EditorElements.PrefixLabel(new GUIContent("Shadow Colour")), targ.shadowColor);
            if (shadowColor != targ.shadowColor) { targ.shadowColor = shadowColor; changes = true; }
            EditorGUILayout.Space(6);

            if (EditorElements.IconButton(btnRect, "refresh", "Force update components") || changes)
                targ.UpdateComponents();
        }
        EditorElements.EndHorizVert();
    }
}
