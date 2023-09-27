using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NeoCambion;
using NeoCambion.Collections;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class GenericMenu : UIObjectSetContainer
{
    public UIObjectSet frames { get { return set; } set { set = value; } }
    public int activeFrame { get { return activeInSet; } set { activeInSet = value; } }
    public void SetFrame(int index) => set.SetActiveObject(index);

    public bool resetFrameOnHide;

    protected override void Initialise()
    {
        base.Initialise();
    }

    public override void OnHide()
    {
        if (resetFrameOnHide)
            activeFrame = 0;
        base.OnHide();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GenericMenu))]
public class GenericMenuEditor : UIObjectSetContainerEditor
{
    public new static void Header() => TypeHeader("Generic Menu", 14);

    public new static void DrawInspector(InheritEditor editor)
    {
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("resetFrameOnHide"), new GUIContent("Reset On Hide"));
    }
}
#endif
