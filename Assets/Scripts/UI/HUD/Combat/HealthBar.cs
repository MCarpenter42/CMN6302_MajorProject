using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion.Unity.Interpolation;

[ExecuteAlways]
public class HealthBar : Core
{
    [HideInInspector] public Transform rotateTarget;

    [SerializeField] SpriteRenderer background;
    [SerializeField] SpriteRenderer main;
    [SerializeField] SpriteMask mask;
    [SerializeField] Color clrDefault;
    private float maskSpriteUnitWidth { get { return mask.sprite.rect.width / mask.sprite.pixelsPerUnit; } }

    public static Color clrDamaged = new Color(1.000f, 0.050f, 0.050f, 1.000f);
    public static Color clrHealed = new Color(0.050f, 1.000f, 0.050f, 1.000f);

    private Coroutine c_ColourFlash = null;

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

    public void SetValue(float percentValue, float delay = 0f)
    {
        if (delay > 0f)
            StartCoroutine(ISetValue(percentValue, delay));
        else
        {
            if (percentValue < 0f)
                percentValue = 0f;
            else if (percentValue > 1f)
                percentValue = 1f;
            Vector3 scaleVec = mask.transform.localScale;
            scaleVec.x = percentValue;
            mask.transform.localScale = scaleVec;
            mask.transform.localPosition = ((1f - percentValue) * (maskSpriteUnitWidth / 2f)) * Vector3.left;
        }
    }
    private IEnumerator ISetValue(float percentValue, float delay)
    {
        if (percentValue < 0f)
            percentValue = 0f;
        else if (percentValue > 1f)
            percentValue = 1f;
        Vector3 scaleVec = mask.transform.localScale;
        scaleVec.x = percentValue;
        yield return new WaitForSeconds(delay);
        mask.transform.localScale = scaleVec;
        mask.transform.localPosition = ((1f - percentValue) * (maskSpriteUnitWidth / 2f)) * Vector3.left;
    }

    public void ColourFlash(Color clr, float duration)
    {
        if (c_ColourFlash != null)
            StopCoroutine(c_ColourFlash);
        c_ColourFlash = StartCoroutine(IColourFlash(clr, duration));
    }
    private IEnumerator IColourFlash(Color clr, float duration)
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
    public void DamagedFlash(float duration) => ColourFlash(clrDamaged, duration);
    public void HealedFlash(float duration) => ColourFlash(clrHealed, duration);

    public void SetValueWithFlash(float percentValue, bool damaged, float duration)
    {
        if (damaged)
            DamagedFlash(duration);
        else
            HealedFlash(duration);
        SetValue(percentValue, duration / 2f);
    }
}
