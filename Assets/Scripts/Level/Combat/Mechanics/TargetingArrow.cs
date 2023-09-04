using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetArrowState { None, Direct, Blast, AOE }
public class TargetingArrow : MonoBehaviour
{

    [SerializeField] TargetingArrowComponent[] components;

    public void ClearState() => SetState(TargetArrowState.None);
    public void SetState(TargetArrowState state)
    {
        switch (state)
        {
            default:
                foreach (TargetingArrowComponent component in components)
                {
                    component.obj.SetActive(false);
                }
                break;

            case TargetArrowState.Direct:
                foreach (TargetingArrowComponent component in components)
                {
                    if (component.showOnDirect)
                        component.obj.SetActive(true);
                    else
                        component.obj.SetActive(false);
                }
                break;

            case TargetArrowState.Blast:
                foreach (TargetingArrowComponent component in components)
                {
                    if (component.showOnBlast)
                        component.obj.SetActive(true);
                    else
                        component.obj.SetActive(false);
                }
                break;

            case TargetArrowState.AOE:
                foreach (TargetingArrowComponent component in components)
                {
                    if (component.showOnAOE)
                        component.obj.SetActive(true);
                    else
                        component.obj.SetActive(false);
                }
                break;
        }
    }
}

[System.Serializable]
public struct TargetingArrowComponent
{
    public GameObject obj;
    public bool showOnDirect;
    public bool showOnBlast;
    public bool showOnAOE;
}
