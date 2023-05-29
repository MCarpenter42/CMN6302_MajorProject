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

public class WorldEntityCore : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] GameObject model;

    #endregion

    #region [ PROPERTIES ]

    public float maxSpeed = 1.0f;
    [HideInInspector] public Vector3 velScale;
    [HideInInspector] public float facing = 0.0f;

    #endregion

    #region [ COROUTINES ]

    protected Coroutine c_modelRot = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {
        if (velScale.magnitude > 0.0f)
            Move(velScale);
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public virtual void Move(Vector3 velocity, bool turnToFace = true)
    {
        transform.position += velocity * maxSpeed * Time.fixedDeltaTime;
        if (turnToFace)
        {
            Vector2 vDir = new Vector2(velocity.x, velocity.z);
            float dir = vDir.Angle2D();
            RotateTo(dir);
        }
    }

    public virtual void SetRot(float orientation)
    {
        facing = orientation.WrapClamp(0.0f, 360.0f);
        Vector3 rot = model.transform.eulerAngles;
        rot.y = facing;
        model.transform.eulerAngles = rot;
    }

    public virtual void Turn(float angle)
    {
        SetRot(facing + angle);
    }

    protected virtual void RotateTo(float angle, float duration = 0.2f)
    {
        if (facing != angle)
        {
            float fPrev = facing;
            facing = angle;
            if (c_modelRot != null)
                StopCoroutine(c_modelRot);
            c_modelRot = StartCoroutine(IRotateTo(fPrev, angle, duration));
        }
    }

    protected virtual IEnumerator IRotateTo(float start, float end, float duration)
    {
        float rotDiff = (end - start).WrapClamp(-180.0f, 180.0f);
        float time = 0.0f;
        float delta;
        while (time <= duration)
        {
            yield return null;
            time += Time.deltaTime;
            delta = time / duration;
            SetRot(start + rotDiff * delta);
        }
        SetRot(end);
    }
}
