using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion.Interpolation;
using NeoCambion.Interpolation.Unity;
using TMPro;

[ExecuteAlways]
public class HealthBarCanvas : HealthBar
{
    [SerializeField] UIObject background;
    [SerializeField] UIObject main;
    [SerializeField] RectTransform maskTransform;
    [SerializeField] Color clrDefault;
    [SerializeField] TMP_Text currentValue;
    public string currentValueText { set { currentValue.text = value; } }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (maskTransform == null)
                maskTransform = GetComponent<RectTransform>();
            if (main != null)
            {
                main.Color = clrDefault;
                background.rTransform.sizeDelta = new Vector2(main.rTransform.sizeDelta.x, background.rTransform.sizeDelta.y);
                maskTransform.sizeDelta = new Vector2(main.rTransform.sizeDelta.x, maskTransform.sizeDelta.y);
            }
        }
    }

    private Vector2 MaskSize(float percentValue)
    {
        if (main == null)
            Debug.Log("Main bar is null");
        float w = main.rTransform.sizeDelta.x * percentValue;
        return new Vector2(w, maskTransform.sizeDelta.y);
    }

    public void SetValue(int current, int max, float duration = 0f)
    {
        currentValueText = current.ToString() + " / " + max.ToString();
        float perc = (float)current / max;
        SetValue(perc, duration);
    }
    public override void SetValue(float percentValue, float duration = 0f)
    {
        if (percentValue < 0f)
            percentValue = 0f;
        else if (percentValue > 1f)
            percentValue = 1f;
        if (duration > 0f)
            StartCoroutine(ISetValue(percentValue, duration));
        else
            maskTransform.sizeDelta = MaskSize(percentValue);
    }
    protected override IEnumerator ISetValue(float percentValue, float duration)
    {
        Vector3 vecStart = maskTransform.sizeDelta, vecTarget = MaskSize(percentValue);
        float t = 0f, delta;
        while (t <= duration)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / duration;
            maskTransform.sizeDelta = vecStart.Interp(vecTarget, delta, InterpType.CosSpeedUp);
        }
        maskTransform.sizeDelta = vecTarget;
    }

    public override void ColourFlash(Color clr, float duration)
    {
        if (c_ColourFlash != null)
            StopCoroutine(c_ColourFlash);
        c_ColourFlash = StartCoroutine(IColourFlash(clr, duration));
    }
    protected override IEnumerator IColourFlash(Color clr, float duration)
    {
        Color clrStart = main.Color;
        float t = 0f, tMax = duration / 2f, delta;
        while (t <= tMax)
        {
            yield return null;
            t += Time.deltaTime;
            delta = InterpDelta.CosSlowDown(t / tMax);
            main.Color = Color.Lerp(clrStart, clr, delta);
        }
        main.Color = clr;
        t = 0f;
        while (t <= tMax)
        {
            yield return null;
            t += Time.deltaTime;
            delta = InterpDelta.CosSpeedUp(t / tMax);
            main.Color = Color.Lerp(clr, clrDefault, delta);
        }
        main.Color = clrDefault;
    }

    public void SetValueWithFlash(int current, int max, bool damaged, float duration)
    {
        currentValueText = current.ToString() + " / " + max.ToString();
        float perc = (float)current / max;
        SetValueWithFlash(perc, damaged, duration);
    }
}
