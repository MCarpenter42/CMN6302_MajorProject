using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;

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
        float rotStart = pivot.transform.eulerAngles.y, rotCurrent;
        float rotTarget = open ? (openClockwise ? 90.0f : -90.0f) : 0.0f;
        float rotDiff = rotTarget - rotStart;
        float t = 0.0f, delta;
        while (t <= 0.25f)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / 0.25f;
            rotCurrent = rotStart + rotDiff * delta;
            pivot.transform.eulerAngles = new Vector3(0.0f, rotCurrent, 0.0f);
        }
        pivot.transform.eulerAngles = new Vector3(0.0f, rotTarget, 0.0f);
        moving  = false;
        isOpen = open;
    }
}
