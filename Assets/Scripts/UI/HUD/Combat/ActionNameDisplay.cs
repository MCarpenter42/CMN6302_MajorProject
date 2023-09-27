using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using NeoCambion;
using NeoCambion.Interpolation;

public class ActionNameDisplay : UIObject
{
    public Vector3 shownOffset;
    public TMP_Text textObject;
    public Graphic[] attachedGraphics;

    protected override void Initialise()
    {
        base.Initialise();
        SetAllAlphas(0f);
    }

    public void SetAllAlphas(float value)
    {
        Color clr;
        foreach (Graphic graphic in attachedGraphics)
        {
            clr = graphic.color;
            clr.a = value;
            graphic.color = clr;
        }
    }

    public void Display(string text, float duration, float hangRatio)
    {
        if (duration < 0.1f)
            duration = 0.1f;
        if (hangRatio < 0f)
            hangRatio = 0f;
        else if (hangRatio > 1f)
            hangRatio = 1f;

        float tShow = (duration * (1f - hangRatio)) * 0.3f, tHang = duration * hangRatio, tHide = (duration * (1f - hangRatio)) * 0.7f;
        textObject.text = text;
        StartCoroutine(IDisplay(tShow, tHang, tHide));
    }

    private IEnumerator IDisplay(float tShow, float tHang, float tHide)
    {
        Vector3 posHidden = transform.position;
        Vector3 posShown = transform.position + shownOffset;
        float t = 0f, delta;
        while (t < tShow)
        {
            yield return null;
            t += Time.deltaTime;
            delta = InterpDelta.CosSlowDown(t / tShow);
            transform.position = Vector3.Lerp(posHidden, posShown, delta);
            SetAllAlphas(delta);
        }
        transform.position = posShown;
        yield return new WaitForSeconds(tHang);
        t = 0f;
        while (t < tHide)
        {
            yield return null;
            t += Time.deltaTime;
            delta = InterpDelta.CosSpeedUp(t / tHide);
            transform.position = Vector3.Lerp(posShown, posHidden, delta);
            SetAllAlphas(1f - delta);
        }
        transform.position = posHidden;
    }
}
