using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;

using NeoCambion.Unity;
#if UNITY_EDITOR
using NeoCambion.Unity.Editor;
#endif
using NeoCambion.Interpolation;

public class EndScreen : UIObject
{
    public TMP_Text text;
    public Image backgroundA;
    public Image backgroundB;
    private Coroutine c_Animation = null;

    protected override void Initialise()
    {
        base.Initialise();
        backgroundA.color = backgroundA.color.AdjustAlpha(0f);
        text.color = text.color.AdjustAlpha(0f);
        backgroundB.transform.localScale = new Vector3(0f, 1f, 1f);
    }

    public void Animation(float wait)
    {
        if (c_Animation != null)
            StopCoroutine(c_Animation);
        c_Animation = StartCoroutine(IAnimation(wait));
    }
    private IEnumerator IAnimation(float wait)
    {
        backgroundA.color = backgroundA.color.AdjustAlpha(0f);
        text.color = text.color.AdjustAlpha(0f);
        backgroundB.transform.localScale = new Vector3(0f, 1f, 1f);

        float t = 0f, tMax = 0.4f, delta;
        while (t < tMax)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / tMax;
            backgroundA.color = backgroundA.color.AdjustAlpha(delta);
            text.color = text.color.AdjustAlpha(delta);
        }
        backgroundA.color = backgroundA.color.AdjustAlpha(1f);
        text.color = text.color.AdjustAlpha(1f);

        t = 0f; tMax = 0.6f;
        while (t < tMax)
        {
            yield return null;
            t += Time.deltaTime;
            delta = InterpDelta.CosCurve(t / tMax);
            backgroundB.transform.localScale = new Vector3(delta, 1f, 1f);
        }
        backgroundB.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(wait);

        backgroundA.color = backgroundA.color.AdjustAlpha(0f);
        text.color = text.color.AdjustAlpha(0f);
        backgroundB.transform.localScale = new Vector3(0f, 1f, 1f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EndScreen))]
public class EndScreenEditor : UIObjectEditor
{
    public new static void Header() => TypeHeader("End Screen", 14);

    public new static void DrawInspector(InheritEditor editor)
    {
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("text"), new GUIContent("Text"));
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("backgroundA"), new GUIContent("Base Background"));
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("backgroundB"), new GUIContent("Overlay Background"));
    }
}
#endif
