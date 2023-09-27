using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : UIObject
{
    public bool disableInit = false;

    protected override void Initialise()
    {
        if (!disableInit)
            base.Initialise();
    }

    public void FadeShow(bool show, float delay, bool useRealtime = false)
    {
        Debug.Log(visible + " --> " + show);
        if (show != visible)
            StartCoroutine(IOnShow(show, delay, useRealtime));
    }

    private IEnumerator IOnShow(bool show, float delay, bool useRealtime = false)
    {
        Alpha = show ? 0f : 1f;
        Graphic.enabled = true;
        AlphaFade(Alpha, (show ? 1f : 0f), delay, useRealtime);
        yield return useRealtime ? new WaitForSecondsRealtime(delay) : new WaitForSeconds(delay);
        if (show)
            OnShow();
        else
            OnHide();
        visible = show;
    }
}
