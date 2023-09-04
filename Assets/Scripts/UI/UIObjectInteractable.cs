using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using NeoCambion;
using NeoCambion.Unity;


[System.Serializable]
public class UIObjectInteractable : UIObject, IMoveHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
{
    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ PROPERTIES ]

    [HideInInspector] public bool clickable = false;
    [HideInInspector] public bool movable = false;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */


}
