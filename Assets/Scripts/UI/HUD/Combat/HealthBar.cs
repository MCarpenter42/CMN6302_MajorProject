using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HealthBar : Core
{
    public static Color clrDamaged = new Color(1.000f, 0.050f, 0.050f, 1.000f);
    public static Color clrHealed = new Color(0.050f, 1.000f, 0.050f, 1.000f);

    protected Coroutine c_ColourFlash = null;

    public abstract void SetValue(float percentValue, float duration = 0f);
    protected abstract IEnumerator ISetValue(float percentValue, float duration);

    public abstract void ColourFlash(Color clr, float duration);
    protected abstract IEnumerator IColourFlash(Color clr, float duration);
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
