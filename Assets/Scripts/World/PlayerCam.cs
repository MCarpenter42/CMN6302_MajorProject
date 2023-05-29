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
using System;

public class PlayerCam : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] GameObject toFollow;
    [SerializeField] GameObject pivot;
    public GameObject cam;

    #endregion

    #region [ PROPERTIES ]

    private Vector3 rot = Vector3.zero;
    private Vector3 rotSpeed = Vector3.zero;
    public Vector3 facingVector { get { return GetFacingVector(); } }

    #endregion

    #region [ COROUTINES ]



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
        if (toFollow != null)
            transform.position = toFollow.transform.position;
        Rotate(rotSpeed);
    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public Vector3 GetFacingVector()
    {
        return pivot.transform.forward;
    }

    public void Rotate(Vector3 angle)
    {
        rot.x = Mathf.Clamp(rot.x + angle.x, -20.0f, 70.0f);
        rot.y = (rot.y + angle.y).WrapClamp(0.0f, 360.0f);
        pivot.transform.eulerAngles = rot;
    }

    public void SetRotSpeed(Vector3 rotation)
    {
        rotSpeed = rotation;
    }
}
