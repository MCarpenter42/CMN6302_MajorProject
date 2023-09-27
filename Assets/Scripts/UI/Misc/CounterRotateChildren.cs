using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterRotateChildren : UIObject
{
    private List<RectTransform> children;

    protected override void Initialise()
    {
        base.Initialise();
        children = new List<RectTransform>();
        for (int i = 0; i < rTransform.childCount; i++)
        {
            children.Add(rTransform.GetChild(i).GetComponent<RectTransform>());
        }
    }

    public void SetRotation(float clockwise)
    {
        clockwise *= -1;
        float diff = clockwise - rTransform.localEulerAngles.z;
        rTransform.localEulerAngles = Vector3.forward * clockwise;
        foreach (RectTransform child in children)
        {
            child.localEulerAngles -= Vector3.forward * diff;
        }
    }
}
