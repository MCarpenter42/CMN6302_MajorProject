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

public class WorldPlayer : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ PROPERTIES ]



    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public override void Move(Vector3 velocity, bool turnToFace = true)
    {
        string vStr = velocity.magnitude.ToString() + ", ";
        velocity = VelCamTransform(velocity * maxSpeed * Time.fixedDeltaTime);
        vStr += velocity.magnitude.ToString();
        transform.position += velocity;
        if (turnToFace)
        {
            Vector2 vDir = new Vector2(velocity.x, velocity.z);
            float dir = vDir.Angle2D();
            RotateTo(dir, 0.1f);
        }
        Debug.Log(vStr);
    }

    private Vector3 VelCamTransform(Vector3 velocity)
    {
        float m = velocity.magnitude;
        float velAngle = velocity.x >= 0.0f ? Vector3.Angle(Vector3.forward, velocity) : -Vector3.Angle(Vector3.forward, velocity);
        Vector3 camDir = UnityExt_Vector3.Flatten(GameManager.Instance.cameraW.facingVector);
        float camAngle = camDir.x >= 0.0f ? Vector3.Angle(Vector3.forward, camDir) : -Vector3.Angle(Vector3.forward, camDir);
        float angle = (velAngle + camAngle).WrapClamp(0.0f, 360.0f).ToRad();
        return new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * m;
    }
}
