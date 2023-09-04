using UnityEngine;
using NeoCambion.Maths;
using UnityEditor;
using NeoCambion.Unity;
using static UnityEngine.GraphicsBuffer;
using NeoCambion.Unity.Editor;
using NeoCambion;

[System.Serializable]
[RequireComponent(typeof(RectTransform))]
public class UIConstRotate : MonoBehaviour
{
    public bool autoSelect = true;
    public RotationTarget rotTarget = new RotationTarget(null, 0);
    public RectTransform rTransform { get { return rotTarget.rTransform; } set { rotTarget.rTransform = value; } }

    void Awake()
    {
        if (autoSelect)
            rotTarget.rTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rotTarget.Rotate(Time.deltaTime);
    }

    public void SetTargetTransform(RectTransform rectTransform)
    {
        rotTarget.rTransform = rectTransform;
    }
}

[CustomEditor(typeof(UIConstRotate))]
[CanEditMultipleObjects]
public class UIConstRotateEditor : Editor
{
    UIConstRotate targ { get { return target as UIConstRotate; } }

    public override void OnInspectorGUI()
    {
        targ.autoSelect = EditorGUILayout.ToggleLeft(new GUIContent("Auto-Select Self As Target"), targ.autoSelect);
        EditorGUILayout.Space(0);
        if (targ.autoSelect)
        {
            targ.rotTarget.rotSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation Speed"), targ.rotTarget.rotSpeed);
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotTarget"), new GUIContent("Rotation Target"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}

[System.Serializable]
public class RotationTarget
{
    public RectTransform rTransform;
    public float rotSpeed;
    [HideInInspector] public Vector3 rot;

    public RotationTarget(RectTransform rTransform, float rotSpeed)
    {
        this.rTransform = rTransform;
        this.rotSpeed = rotSpeed;
        rot = Vector3.zero;
    }

    public void Rotate(float tDelta)
    {
        rot.z = (rot.z -= rotSpeed * tDelta).WrapClamp(0.0f, 360.0f);
        if (rTransform != null/* && rTransform.gameObject.activeInHierarchy*/)
        {
            rTransform.eulerAngles = rot;
        }
    }
}

[CustomPropertyDrawer(typeof(RotationTarget))]
public class RotationTargetDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h1 = base.GetPropertyHeight(property, label);
        float h2 = (label == GUIContent.none ? 2.0f : 3.0f) * EditorGUIUtility.singleLineHeight;
        return h1 < h2 ? h2 : h1;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool labelEmpty = label == GUIContent.none;
        if (labelEmpty)
        {
            label.text = property.name;
            labelEmpty = false;
        }
        float slHeight = EditorGUIUtility.singleLineHeight, offset = 12, wLabel = EditorGUIUtility.labelWidth - offset;

        GUIStyle subLabel = EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.fontSize - 1));

        EditorGUI.BeginProperty(position, label, property);
        
        position.height = slHeight;
        if (!labelEmpty)
        {
            EditorGUI.LabelField(position, label);
            position.y += slHeight;
        }
        position.x += offset;
        position.width -= offset;
        EditorGUI.LabelField(new Rect(position) { width = wLabel }, new GUIContent("Target Transform"), subLabel);
        EditorGUI.PropertyField(new Rect(position) { width = position.width - wLabel, x = position.x + wLabel }, property.FindPropertyRelative("rTransform"), GUIContent.none);
        position.y += slHeight;
        EditorGUI.LabelField(new Rect(position) { width = wLabel }, new GUIContent("Rotation Speed"), subLabel);
        EditorGUI.PropertyField(new Rect(position) { width = position.width - wLabel, x = position.x + wLabel }, property.FindPropertyRelative("rotSpeed"), GUIContent.none);
        
        EditorGUI.EndProperty();
    }
}
