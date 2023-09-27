using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class UIObjectSetContainer : UIObject
{
    public UIObjectSet set = new UIObjectSet();

    public int activeInSet
    {
        get { return set.visible ? set.active : -1; }
        set { set.SetActiveObject(value); }
    }

    protected override void Initialise()
    {
        base.Initialise();
        if (visibleOnStart)
            set.SetActiveObject(0);
    }

    public void SetActiveObject(int index) => set.SetActiveObject(index);

    public override GameObject[] GetContents()
    {
        Transform[] transforms = gameObject.transform.GetChildren();
        _contents_all = new GameObject[transforms.Length];
        List<UIObject> UIO = new List<UIObject>();
        List<GameObject> nonUIO = new List<GameObject>();
        GameObject obj; UIObject uiObj;
        for (int i = 0; i < transforms.Length; i++)
        {
            obj = transforms[i].gameObject;
            uiObj = obj.GetComponent<UIObject>();
            if (uiObj == null)
                nonUIO.Add(obj);
            else if (!set.Contains(uiObj))
                UIO.Add(uiObj);
        }
        _contents_UIO = UIO.ToArray();
        _contents_nonUIO = nonUIO.ToArray();
        return _contents_all;
    }

    public override void OnShow()
    {
        base.OnShow();
        set.ShowActive();
    }

    public override void OnHide()
    {
        base.OnHide();
        set.HideAll();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIObjectSetContainer))]
public class UIObjectSetContainerEditor : UIObjectEditor
{
    public new static void Header() => TypeHeader("UI Object Set", 14);

    public new static void DrawInspector(InheritEditor editor)
    {
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("set"), new GUIContent("Set Contents"));
    }
}
#endif
