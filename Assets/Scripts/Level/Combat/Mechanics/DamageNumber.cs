using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class DamageNumber : Core
{
    public TMP_Text TextMesh;
    public string Text { get { return TextMesh.text; } set { TextMesh.text = value; } }
    public Color Color { get { return TextMesh.color; } set { TextMesh.color = value; } }
    public Color OutlineColor { get { return TextMesh.outlineColor; } set { TextMesh.outlineColor = value; } }

    public Transform lookTarg;
    public Vector3 lookAt => new Vector3(lookTarg.position.x, transform.position.y, lookTarg.position.z);

    private Coroutine c_Animate = null;

    void Update()
    {
        if (lookTarg != null)
            transform.LookAt(lookAt);
    }

    public void Animate(Color color, float tFadeIn, float tPause, float tFadeOut)
    {
        if (c_Animate != null)
            StopCoroutine(c_Animate);
        c_Animate = StartCoroutine(IAnimate(color, tFadeIn, tPause, tFadeOut));
    }
    private IEnumerator IAnimate(Color color, float tFadeIn, float tPause, float tFadeOut)
    {
        Color[] clrs = new Color[] { new Color(color.r, color.g, color.b, 0f), new Color(color.r, color.g, color.b, 1f) };
        float t, delta;

        t = 0f;
        while (t < tFadeIn)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / tFadeIn;
            Color = Color.Lerp(clrs[0], clrs[1], delta);
        }
        Color = clrs[1];

        yield return new WaitForSeconds(tPause);

        Vector3 posStart = transform.position, posEnd = posStart + Vector3.up;
        t = 0f;
        while (t < tFadeOut)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / tFadeIn;
            Color = Color.Lerp(clrs[1], clrs[0], delta);
            transform.position = Vector3.Lerp(posStart, posEnd, delta);
        }
        Color = clrs[0];
        Destroy(gameObject, 0.1f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DamageNumber))]
public class DamageNumberEditor : Editor
{
    DamageNumber targ => target as DamageNumber;
    SerializedProperty TextMeshProp;
    Object TextMesh { get { return TextMeshProp.objectReferenceValue; } set { TextMeshProp.objectReferenceValue = value; } }

    private void OnEnable()
    {
        TextMeshProp = serializedObject.FindProperty("TextMesh");
    }

    public override void OnInspectorGUI()
    {
        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding, EditorStyles.inspectorFullWidthMargins);
        {
            EditorGUILayout.PropertyField(TextMeshProp, new GUIContent("Text Mesh"));
        }
        EditorElements.EndHorizVert();
        if (TextMesh == null)
            TextMesh = targ.GetComponent<TMP_Text>();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
