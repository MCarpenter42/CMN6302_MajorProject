using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Interpolation;
using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

[System.Serializable]
[RequireComponent(typeof(RectTransform))]
public class UIObject : Core, IPointerEnterHandler, IPointerExitHandler
{
    #region [ OBJECTS / COMPONENTS ]

    public GameObject[] Contents { get { return _contents_all ?? GetContents(); } }
    protected GameObject[] _contents_all;
    protected UIObject[] _contents_UIO;
    protected GameObject[] _contents_nonUIO;

    protected RectTransform _rTransform = null;
    public RectTransform rTransform
    {
        get
        {
            if (_rTransform == null)
                _rTransform = GetComponent<RectTransform>();
            return _rTransform;
        }
    }

    private Image _image = null;
    public Image Image => Application.isPlaying ? (_image ?? (_image = GetComponent<Image>())) : GetComponent<Image>();
    private TMP_Text _textMesh = null;
    public TMP_Text TextMesh => Application.isPlaying ? (_textMesh ?? (_textMesh = GetComponent<TMP_Text>())) : GetComponent<TMP_Text>();
    public Graphic Graphic => Image != null ? Image : (TextMesh != null ? TextMesh : null);

    public bool separateLabel = false;
    public TMP_Text label;

    public bool selectionContainer;
    public List<ControllerSelectWrapper> selectables;

    #endregion

    #region [ PROPERTIES ]

    public Sprite Sprite
    {
        get { return (Image != null) ? Image.sprite : null; }
        set { if (Image != null) Image.sprite = value; }
    }
    public void SetSprite(Sprite sprite) => Sprite = sprite;

    public string Text
    {
        get { return TextMesh != null ? TextMesh.text : null; }
        set { if (TextMesh != null) TextMesh.text = value; }
    }
    public void SetText(string text) => Text = text;
    public TMP_FontAsset Font
    {
        get { return TextMesh != null ? TextMesh.font : null; }
        set { if (TextMesh != null) TextMesh.font = value; }
    }
    public void SetFont(TMP_FontAsset font) => Font = font;

    public Color ImageColor
    {
        get { return Image != null ? Image.color : Color.black; }
        set { if (Image != null) Image.color = value; }
    }
    public void SetImageColor(Color color) => ImageColor = color;
    public void SetImageColorFrom(Image source) => ImageColor = source.color;
    public Color TextColor
    {
        get { return TextMesh != null ? TextMesh.color : Color.black; }
        set { if (TextMesh != null) TextMesh.color = value; }
    }
    public void SetTextColor(Color color) => TextColor = color;
    public void SetTextColorFrom(TMP_Text source) => TextColor = source.color;
    public Color Color
    {
        get { return Graphic != null ? Graphic.color : Color.black; }
        set { if (Graphic != null) Graphic.color = value; }
    }
    public void SetColor(Color color) => Color = color;
    public void SetColorFrom(UIObject source) => Color = source.Color;
    public float Alpha
    {
        get { return Color.a; }
        set { Color clr = Color; clr.a = value; Color = clr; }
    }

    public bool visibleOnStart = true;
    public bool visible { get; protected set; } = true;

    // ADD TAGGING SYSTEM?

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_ColourFade;
    private Coroutine c_AlphaFade;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    /*protected new void Awake()
    {
        if (!GameManager.InSceneTransition)
            Initialise();
        else
            GameManager.Initialisers.Add(Initialise);
    }*/
    protected override void Initialise()
    {
        base.Initialise();
        _image = GetComponent<Image>();
        _textMesh = GetComponent<TMP_Text>();
        GetContents();

        if (!visible || !visibleOnStart)
            Show(false);

    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ INTERFACE FUNCTIONS ]

    public virtual void OnPointerEnter(PointerEventData eventData)
    {

    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public virtual GameObject[] GetContents()
    {
        Transform[] transforms = gameObject.transform.GetChildren();
        _contents_all = new GameObject[transforms.Length];
        List<UIObject> UIO = new List<UIObject>();
        List<GameObject> nonUIO = new List<GameObject>();
        GameObject obj;
        for (int i = 0; i < transforms.Length; i++)
        {
            obj = transforms[i].gameObject;
            _contents_all[i] = obj;
            if (!UIO.AddUnlessNull(obj.GetComponent<UIObject>()))
                nonUIO.Add(obj);
        }
        _contents_UIO = UIO.ToArray();
        _contents_nonUIO = nonUIO.ToArray();
        return _contents_all;
    }

    public void Show(bool show = true)
    {
        if (show/* && !visible*/)
            OnShow();
        else if (!show/* && visible*/)
            OnHide();
        visible = show;
    }

#if UNITY_EDITOR
    [MenuItem("Component/UI/UI Object", false, -10)]
    private static void AddAsComponent()
    {
        Selection.activeGameObject.AddComponent<UIObject>();
    }
    
    [MenuItem("Component/UI/UI Object", true)]
    private static bool AddAsComponentValidation()
    {
        return Selection.activeGameObject != null;
    }
#endif

    public virtual void Toggle()
    {
        Show(!visible);
    }

    public virtual void OnShow()
    {
        if (_contents_UIO == null || _contents_nonUIO == null)
            GetContents();
        foreach (UIObject uiObj in _contents_UIO)
            if (uiObj != null)
                uiObj.Show(true);
        foreach (GameObject obj in _contents_nonUIO)
            if (obj != null)
                obj.SetActive(true);
        if (Image != null)
            Image.enabled = true;
        if (TextMesh != null)
            TextMesh.enabled = true;
        if (selectionContainer)
            AddSelectablesToRegistry();
    }

    public virtual void OnHide()
    {
        if (_contents_UIO == null || _contents_nonUIO == null)
            GetContents();
        foreach (UIObject uiObj in _contents_UIO)
            if (uiObj != null)
                uiObj.Show(false);
        foreach (GameObject obj in _contents_nonUIO)
            if (obj != null)
                obj.SetActive(false);
        if (Image != null)
            Image.enabled = false;
        if (TextMesh != null)
            TextMesh.enabled = false;
        if (selectionContainer)
            RemoveSelectablesFromRegistry();
    }

    public void DeselectSelf()
    {
        if (EventSystem.currentSelectedGameObject == gameObject)
            EventSystem.SetSelectedGameObject(null);
    }

    protected void AddSelectablesToRegistry() => UIManager.SetSelectables(this);
    protected void RemoveSelectablesFromRegistry() => UIManager.ClearSelectables(this);

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void ChangeParent(RectTransform parent)
    {
        rTransform.SetParent(parent, false);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void ColourFade(Color clrStart, Color clrEnd, float duration, bool affectAlpha = true, bool useRealtime = false)
    {
        if (c_ColourFade != null)
            StopCoroutine(c_ColourFade);
        c_ColourFade = StartCoroutine(IColourFade(clrStart, clrEnd, duration, affectAlpha, useRealtime));
    }
    private IEnumerator IColourFade(Color clrStart, Color clrEnd, float duration, bool affectAlpha, bool useRealtime)
    {
        float t = 0f, delta;
        while (t < duration)
        {
            yield return null;
            t += useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
            delta = InterpDelta.CosCurve(t / duration);
            if (!affectAlpha)
                clrStart.a = clrEnd.a = Color.a;
            Color = Color.Lerp(clrStart, clrEnd, delta);
        }
        Color = clrEnd;
    }

    public void ColourPulse(Color clrStart, Color clrPulse, float duration, float pause = 0f, bool affectAlpha = true, bool useRealtime = false)
    {
        if (c_ColourFade != null)
            StopCoroutine(c_ColourFade);
        c_ColourFade = StartCoroutine(IColourPulse(clrStart, clrPulse, duration, pause, affectAlpha, useRealtime));
    }
    private IEnumerator IColourPulse(Color clrStart, Color clrPulse, float duration, float pause, bool affectAlpha, bool useRealtime)
    {
        duration -= pause;
        duration /= 2f;
        StartCoroutine(IColourFade(clrStart, clrPulse, duration, affectAlpha, useRealtime));
        yield return useRealtime ? new WaitForSecondsRealtime(duration) : new WaitForSeconds(duration);
        if (!affectAlpha)
            clrStart.a = clrPulse.a = Color.a;
        Color = clrPulse;
        yield return useRealtime ? new WaitForSecondsRealtime(pause) : new WaitForSeconds(pause);
        StartCoroutine(IColourFade(clrPulse, clrStart, duration, affectAlpha, useRealtime));
        yield return useRealtime ? new WaitForSecondsRealtime(duration) : new WaitForSeconds(duration);
        if (!affectAlpha)
            clrStart.a = clrPulse.a = Color.a;
        Color = clrStart;
    }

    public void AlphaFade(float valStart, float valEnd, float duration, bool useRealtime = false)
    {
        if (c_AlphaFade != null)
            StopCoroutine(c_AlphaFade);
        c_AlphaFade = StartCoroutine(IAlphaFade(valStart, valEnd, duration, useRealtime));
    }
    private IEnumerator IAlphaFade(float valStart, float valEnd, float duration, bool useRealtime)
    {
        float t = 0f, delta, f;
        while (t < duration)
        {
            yield return null;
            t += useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
            delta = InterpDelta.CosCurve(t / duration);
            f = Mathf.Lerp(valStart, valEnd, delta);
            Alpha = f;
        }
        Alpha = valEnd;
    }

    public void AlphaPulse(float valStart, float valEnd, float duration, float pause = 0f, bool useRealtime = false)
    {
        if (c_ColourFade != null)
            StopCoroutine(c_ColourFade);
        c_ColourFade = StartCoroutine(IAlphaPulse(valStart, valEnd, duration, pause, useRealtime));
    }
    private IEnumerator IAlphaPulse(float valStart, float valPulse, float duration, float pause, bool useRealtime)
    {
        duration -= pause;
        duration /= 2f;
        StartCoroutine(IAlphaFade(valStart, valPulse, duration, useRealtime));
        yield return useRealtime ? new WaitForSecondsRealtime(duration) : new WaitForSeconds(duration);
        Alpha = valPulse;
        yield return useRealtime ? new WaitForSecondsRealtime(pause) : new WaitForSeconds(pause);
        StartCoroutine(IAlphaFade(valPulse, valStart, duration, useRealtime));
        yield return useRealtime ? new WaitForSecondsRealtime(duration) : new WaitForSeconds(duration);
        Alpha = valStart;
    }

    public void AlphaPulse(float valStart, float valEnd, float duration, System.Func<bool> waitFor, bool useRealtime = false)
    {
        if (c_ColourFade != null)
            StopCoroutine(c_ColourFade);
        c_ColourFade = StartCoroutine(IAlphaPulse(valStart, valEnd, duration, waitFor, useRealtime));
    }
    private IEnumerator IAlphaPulse(float valStart, float valPulse, float duration, System.Func<bool> waitFor, bool useRealtime)
    {
        duration /= 2f;
        StartCoroutine(IAlphaFade(valStart, valPulse, duration, useRealtime));
        yield return useRealtime ? new WaitForSecondsRealtime(duration) : new WaitForSeconds(duration);
        Alpha = valPulse;
        yield return new WaitUntil(waitFor);
        StartCoroutine(IAlphaFade(valPulse, valStart, duration, useRealtime));
        yield return useRealtime ? new WaitForSecondsRealtime(duration) : new WaitForSeconds(duration);
        Alpha = valStart;
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(UIObject))]
public class UIObjectEditor : InheritEditor
{
    public new static void Header() => TypeHeader("UI Object", 14);

    public new static void DrawInspector(InheritEditor editor)
    {
        UIObject targ = editor.target as UIObject;
        Rect rect, toggleRect;
        if (Application.isPlaying)
        {
            rect = EditorElements.LabelPrefixIcon(EditorElements.ControlRect(), targ.visible ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff");
            rect.x += 4; rect.width = 50;
            if (GUI.Button(rect, new GUIContent(targ.visible ? "Visible" : "Hidden")))
                targ.Toggle();
            EditorGUILayout.Space(2f);
            EditorElements.SeparatorBar();
            EditorGUILayout.Space(2f);
        }
        EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("visibleOnStart"), new GUIContent("Visible On Start"));
        SerializedProperty useLabel = editor.serializedObject.FindProperty("separateLabel");
        rect = EditorElements.PrefixLabel(EditorElements.ControlRect(), new GUIContent("Label", "Toggle on if this UI object uses a separate label"));
        rect.x += 2f; rect.width -= 2f;
        useLabel.boolValue = EditorGUI.Toggle(rect, GUIContent.none, useLabel.boolValue);
        rect.x += rect.height; rect.width -= rect.height;
        if (useLabel.boolValue)
            EditorGUI.PropertyField(rect, editor.serializedObject.FindProperty("label"), GUIContent.none);
        else
            editor.serializedObject.FindProperty("label").objectReferenceValue = null;
        SerializedProperty selectionContainer = editor.serializedObject.FindProperty("selectionContainer");
        EditorGUILayout.PropertyField(selectionContainer, new GUIContent("Contains Selectable"));
        if (selectionContainer.boolValue == true)
            EditorGUILayout.PropertyField(editor.serializedObject.FindProperty("selectables"), new GUIContent("Selectable UI Objects"));
    }
}
#endif

[System.Serializable]
public class UIObjectSet
{
    public UIObject[] items = null;
    [HideInInspector] public int active = 0;
    [HideInInspector] public bool visible;
    public int Size => items == null ? 0 : items.Length;

    public UIObjectSet()
    {
        items = new UIObject[0];
    }
    public UIObjectSet(UIObject[] items)
    {
        this.items = items;
        if (items.Length > 0 && items[0] != null)
            SetActiveObject(0);
    }

    public bool Contains(UIObject item) => item != null && items.Contains(item);
    public bool Contains(GameObject item)
    {
        if (item == null)
            return false;
        UIObject uiObj = item.GetComponent<UIObject>();
        if (uiObj == null)
            return false;
        else
            return items.Contains(uiObj);
    }

    public void ShowActive()
    {
        visible = true;
        for (int i = 0; i < items.Length; i++)
        {
            if (i == active)
                items[i].OnShow();
            else
                items[i].OnHide();
        }
    }
    public void HideAll()
    {
        visible = false;
        foreach (UIObject item in items)
            item.OnHide();
    }

    public void SetActiveObject(int index)
    {
        if (index < items.Length)
        {
            if (visible = index >= 0)
                active = index;
            for (int i = 0; i < items.Length; i++)
            {
                if (i == index && visible)
                    items[i].OnShow();
                else
                    items[i].OnHide();
            }
        }
    }
}

[System.Serializable]
public struct RelativeUIPosition
{
    public RectTransform anchoredTo;
    public Vector2 anchoredPosition;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
    public Vector2 sizeDelta;
    public Vector3 localEulerAngles;
}
