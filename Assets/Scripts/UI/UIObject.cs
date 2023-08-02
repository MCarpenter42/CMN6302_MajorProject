using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NeoCambion;
using NeoCambion.Unity;

[RequireComponent(typeof(RectTransform))]
public class UIObject : Core
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

    #endregion

    #region [ PROPERTIES ]

    public bool visible { get; private set; }
    
    // ADD TAGGING SYSTEM?

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected virtual void Awake()
    {
        GetContents();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

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
        visible = show;
        if (show)
            OnShow();
        else
            OnHide();
    }

    public void Toggle()
    {
        Show(!visible);
    }

    protected virtual void OnShow()
    {
        foreach(GameObject obj in contents)
        {
            obj.SetActive(true);
        }
    }

    protected virtual void OnHide()
    {
        foreach (GameObject obj in contents)
        {
            obj.SetActive(false);
        }
    }
}
