using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using NeoCambion.Interpolation;
using NeoCambion.Interpolation.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class PopupPrompt : UIObject
{
    public RectTransform[] secondary;
    public TMP_Text title;
    public TMP_Text description;

    public UIObjectInteractable[] buttons;

    public Vector2 posShow = Vector3.zero;
    public Vector2 posHide = new Vector3(0f, -200f, 0f);
    public Vector2 scaleHide = Vector3.zero;

    private Coroutine c_ShowHide = null;

    public void Animate(bool show, float durationA = 0.75f, float durationB = 0.25f)
    {
        if (c_ShowHide != null)
            StopCoroutine(c_ShowHide);
        c_ShowHide = StartCoroutine(show ? AnimateShow(posHide, posShow, durationA, durationB) : AnimateHide(posShow, posHide, durationA, durationB));
    }
    private IEnumerator AnimateShow(Vector3 posStart, Vector3 posTarget, float durationA = 0.75f, float durationB = 0.25f)
    {
        GameManager.SetControlState(ControlState.None);
        Vector3 scaleStart = Vector3.zero, scaleTarget = Vector3.one;
        Vector3 textScaleStart = new Vector3(1f, 0f, 1f), textScaleTarget = Vector3.one;
        foreach (RectTransform rTran in secondary)
        {
            rTran.localScale = textScaleStart;
        }
        float t = 0, delta;
        while (t < durationA)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / durationA;
            rTransform.anchoredPosition = posStart.Interp(posTarget, delta, InterpType.CosSlowDown);
            rTransform.localScale = scaleStart.Interp(scaleTarget, delta, InterpType.CosSpeedUp);
        }
        rTransform.anchoredPosition = posTarget;
        rTransform.localScale = scaleTarget;
        t = 0;
        while (t < durationB)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / durationB;
            foreach (RectTransform rTran in secondary)
            {
                rTran.localScale = textScaleStart.Interp(textScaleTarget, delta, InterpType.CosCurve);
            }
        }
        foreach (RectTransform rTran in secondary)
        {
            rTran.localScale = textScaleTarget;
        }
        GameManager.SetControlState(ControlState.Menu);
    }
    private IEnumerator AnimateHide(Vector3 posStart, Vector3 posTarget, float durationA = 0.75f, float durationB = 0.25f)
    {
        GameManager.SetControlState(ControlState.None);
        Vector3 scaleStart = Vector3.one, scaleTarget = Vector3.zero;
        Vector3 textScaleStart = Vector3.one, textScaleTarget = new Vector3(1f, 0f, 1f);
        foreach (RectTransform rTran in secondary)
        {
            rTran.localScale = textScaleStart;
        }
        float t = 0, delta;
        while (t < durationB)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / durationB;
            foreach (RectTransform rTran in secondary)
            {
                rTran.localScale = textScaleStart.Interp(textScaleTarget, delta, InterpType.CosCurve);
            }
        }
        foreach (RectTransform rTran in secondary)
        {
            rTran.localScale = textScaleTarget;
        }
        t = 0;
        while (t < durationA)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / durationA;
            rTransform.anchoredPosition = posStart.Interp(posTarget, delta, InterpType.CosSpeedUp);
            rTransform.localScale = scaleStart.Interp(scaleTarget, delta, InterpType.CosSlowDown);
        }
        rTransform.anchoredPosition = posTarget;
        rTransform.localScale = scaleTarget;
        GameManager.SetControlState(ControlState.World);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PopupPrompt))]
public class PopupPromptEditor : UIObjectEditor
{
    public new static void Header() => TypeHeader("Popup Prompt", 14);

    public new static void DrawInspector(InheritEditor editor)
    {
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("posShow"), new GUIContent("Position When Shown"));
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("posHide"), new GUIContent("Position When Hidden"));
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("scaleHide"), new GUIContent("Scale When Hidden"));
        EditorGUILayout.Space(6f);
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("title"), new GUIContent("Title Text"));
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("description"), new GUIContent("Description Text"));
        EditorGUILayout.Space(6f);
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("buttons"), new GUIContent("Interaction Buttons"));
        EditorGUILayout.Space(6f);
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("secondary"), new GUIContent("Secondary Transforms"));
    }
}
#endif
