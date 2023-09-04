using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

using NeoCambion;
using NeoCambion.Unity;

[System.Serializable]
[RequireComponent(typeof(RectTransform))]
public class UIObject : Core, IPointerEnterHandler, IPointerExitHandler
{
    #region [ OBJECTS / COMPONENTS ]

    protected GameObject[] contents;
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

    protected Image imageComponent;
    protected TMP_Text textComponent;

    #endregion

    #region [ PROPERTIES ]

    [SerializeField] bool visibleOnStart = true;
    public bool visible { get; private set; }

    // ADD TAGGING SYSTEM?

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected virtual void Awake()
    {
        GetContents();
        imageComponent = GetComponent<Image>();
        textComponent = GetComponent<TMP_Text>();
    }

    protected virtual void Start()
    {
        visible = visibleOnStart;
        if (!visibleOnStart)
            OnHide();
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

    protected void GetContents()
    {
        Transform[] transforms = gameObject.transform.GetChildren();
        contents = new GameObject[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            contents[i] = transforms[i].gameObject;
        }
    }

    public void Show(bool show = true)
    {
        if (show/* && !visible*/)
            OnShow();
        else/* if (!show && visible)*/
            OnHide();
        visible = show;
    }

    [MenuItem("Component/UI/UI Object", false, 10)]
    private static void AddAsComponent()
    {
        Selection.activeGameObject.AddComponent<UIObject>();
    }
    
    [MenuItem("Component/UI/UI Object", true)]
    private static bool AddAsComponentValidation()
    {
        return Selection.activeGameObject != null;
    }

    public virtual void Toggle()
    {
        Show(!visible);
    }

    protected virtual void OnShow()
    {
        foreach(GameObject obj in contents)
        {
            obj.SetActive(true);
        }
        if (imageComponent != null)
            imageComponent.enabled = true;
    }

    protected virtual void OnHide()
    {
        foreach (GameObject obj in contents)
        {
            obj.SetActive(false);
        }
        if (imageComponent != null)
            imageComponent.enabled = false;
    }
}
