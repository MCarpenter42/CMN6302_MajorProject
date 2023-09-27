using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using NeoCambion;
using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

[System.Serializable]
public class UIObjectInteractable : UIObject, IMoveHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
{
    #region [ OBJECTS / COMPONENTS ]

    private Button _button = null;
    public Button Button => Application.isPlaying ? (_button ?? (_button = GetComponent<Button>())) : GetComponent<Button>();

    public bool highlighted = false;

    public bool interactable => Button != null ? Button.interactable : true;

    #endregion

    #region [ PROPERTIES ]

    [HideInInspector] public bool clickable = false;
    [HideInInspector] public bool movable = false;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Initialise()
    {
        base.Initialise();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ INTERFACE FUNCTIONS ]

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

    }

    public virtual void OnMove(AxisEventData eventData)
    {

    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {

    }
    
    public virtual void OnPointerUp(PointerEventData eventData)
    {

    }

    public virtual void OnSelect(BaseEventData eventData)
    {

    }
    
    public virtual void OnDeselect(BaseEventData eventData)
    {

    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {

    }

    public virtual void OnSubmit(BaseEventData eventData)
    {

    }

    #endregion

    public void SelectThis(bool select)
    {
        if (select)
        {
            if (EventSystem.currentSelectedGameObject != gameObject)
                if (Button == null)
                    EventSystem.SetSelectedGameObject(gameObject);
                else
                    Button.OnSelect(new PointerEventData(EventSystem));
        }
        else
        {
            if (EventSystem.currentSelectedGameObject == gameObject)
                EventSystem.SetSelectedGameObject(null);
            if (Button != null)
                Button.OnDeselect(new PointerEventData(EventSystem));
        }   
    }

    public void HighlightThis(bool highlight)
    {
        if (highlight != highlighted)
        {
            highlighted = highlight;
            if (highlighted)
                OnControllerHighlight();
            else
                OnControllerDeHighlight();
        }
    }

    public void OnControllerHighlight()
    {
        highlighted = true;
        if (Button != null)
            Button.OnPointerEnter(null);
    }
    
    public void OnControllerDeHighlight()
    {
        highlighted = false;
        if (Button != null)
            Button.OnPointerExit(null);
    }

    public void OnControllerPress()
    {
        if (Button != null)
            Button.OnPointerClick(new PointerEventData(EventSystem));
        /*if (selectOnPressed)
            SelectThis(true);*/
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public override void OnHide()
    {
        base.OnHide();
        OnControllerDeHighlight();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIObjectInteractable))]
public class UIObjectInteractableEditor : UIObjectEditor
{
    public new static void Header() => TypeHeader("UI Object (Interactable)", 14);

    public new static void DrawInspector(InheritEditor editor)
    {
        //UIObjectInteractable targ = editor.target as UIObjectInteractable;
        //EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("selectOnPressed"), new GUIContent("Select On Pressed"));
    }
}
#endif

[System.Serializable]
public struct ControllerSelectWrapper
{
    public UIObjectInteractable uiObject;
    public Vector2Int selectionPos;
    public bool isNull => uiObject == null && selectionPos == minVec;
    public bool interactable => !isNull ? uiObject.interactable : false;
    public bool visible => uiObject.visible;

    public ControllerSelectWrapper(UIObjectInteractable uiObject, Vector2Int selectionPos)
    {
        this.uiObject = uiObject;
        this.selectionPos = selectionPos;
    }

    public static ControllerSelectWrapper Null => new ControllerSelectWrapper(null, minVec);
    private static Vector2Int minVec = new Vector2Int(int.MinValue, int.MinValue);

    public void HighlightThis(bool select)
    {
        if (uiObject != null)
            uiObject.HighlightThis(select);
    }

    public void PressThis()
    {
        if (uiObject != null)
            uiObject.OnControllerPress();
    }

    public void Show(bool show) => uiObject.Show(show);
}
