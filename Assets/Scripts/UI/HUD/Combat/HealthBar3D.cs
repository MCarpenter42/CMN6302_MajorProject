using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion.Interpolation;
using NeoCambion.Interpolation.Unity;

[ExecuteAlways]
public class HealthBar3D : HealthBar
{
    [HideInInspector] public Transform rotateTarget;

    [SerializeField] SpriteRenderer background;
    [SerializeField] SpriteRenderer main;
    [SerializeField] SpriteMask mask;
    [SerializeField] Color clrDefault;
    protected float maskSpriteUnitWidth { get { return mask.sprite.rect.width / mask.sprite.pixelsPerUnit; } }

    void Update()
    {
        if (Application.isPlaying)
        {
            if (rotateTarget != null)
                transform.LookAt(rotateTarget.position);
        }
        else
        {
            main.color = clrDefault;
        }
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
        {
            Vector3 scaleVec = mask.transform.localScale;
            scaleVec.x = percentValue;
            mask.transform.localScale = scaleVec;
            mask.transform.localPosition = ((1f - percentValue) * (maskSpriteUnitWidth / 2f)) * Vector3.left;
        }
    }
    protected override IEnumerator ISetValue(float percentValue, float duration)
    {
        Vector3 vecStart = mask.transform.localScale, vecTarget = mask.transform.localScale;
        Vector3 posStart = mask.transform.localPosition, posTarget = (1f - percentValue) * (maskSpriteUnitWidth / 2f) * Vector3.left;
        vecTarget.x = percentValue;
        float t = 0f, delta;
        while (t <= duration)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / duration;
            mask.transform.localScale = vecStart.Interp(vecTarget, delta, InterpType.CosSpeedUp);
            mask.transform.localPosition = posStart.Interp(posTarget, delta, InterpType.CosSpeedUp);
        }
        mask.transform.localScale = vecTarget;
        mask.transform.localPosition = posTarget;
    }

    public override void ColourFlash(Color clr, float duration)
    {
        if (c_ColourFlash != null)
            StopCoroutine(c_ColourFlash);
        c_ColourFlash = StartCoroutine(IColourFlash(clr, duration));
    }
    protected override IEnumerator IColourFlash(Color clr, float duration)
    {
        Color clrStart = main.color;
        float t = 0f, tMax = duration / 2f, delta;
        while (t <= tMax)
        {
            yield return null;
            t += Time.deltaTime;
            delta = InterpDelta.CosSlowDown(t / tMax);
            main.color = Color.Lerp(clrStart, clr, delta);
        }
        main.color = clr;
        t = 0f;
        while (t <= tMax)
        {
            yield return null;
            t += Time.deltaTime;
            delta = InterpDelta.CosSpeedUp(t / tMax);
            main.color = Color.Lerp(clr, clrDefault, delta);
        }
        main.color = clrDefault;
    }
}
