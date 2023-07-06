using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;

public class InDevDoor : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] GameObject pivot;

    #endregion

    #region [ PROPERTIES ]

    [SerializeField] bool openClockwise = true;
    private bool isOpen = false;
    private bool moving = false;

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_rotate = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {

    }

    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Toggle()
    {
        SetOpen(!isOpen);
    }

    public void SetOpen(bool open)
    {
        if (!moving)
        {
            if (c_rotate != null)
                StopCoroutine(c_rotate);
            c_rotate = StartCoroutine(ISetOpen(open));
        }
    }

    private IEnumerator ISetOpen(bool open)
    {
        moving = true;
        float rotStart = pivot.transform.localEulerAngles.y, rotCurrent;
        float rotTarget = open ? (openClockwise ? 90.0f : -90.0f) : 0.0f;
        float rotDiff = rotTarget - rotStart;
        float t = 0.0f, delta;
        while (t <= 0.25f)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / 0.25f;
            rotCurrent = rotStart + rotDiff * delta;
            pivot.transform.localEulerAngles = new Vector3(0.0f, rotCurrent, 0.0f);
        }
        pivot.transform.localEulerAngles = new Vector3(0.0f, rotTarget, 0.0f);
        moving  = false;
        isOpen = open;
    }
}
