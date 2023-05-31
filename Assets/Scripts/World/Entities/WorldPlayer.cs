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
using Unity.VisualScripting;

public class WorldPlayer : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]

    private List<InteractPoint> interactions = new List<InteractPoint>();
    public InteractPoint targetInteract { get { return targetInteractInd > -1 ? interactions[targetInteractInd] : null; } }

    #endregion

    #region [ PROPERTIES ]

    private Vector3 camDir { get { return UnityExt_Vector3.Flatten(GameManager.Instance.cameraW.facingVector); } }

    public float sprintMultiplier = 2.0f;
    [HideInInspector] public bool sprintActive = false;

    [SerializeField] float interactionRange = 3.0f;
    [SerializeField] float maxInteractAngle = 40.0f;
    private List<int> inRangeInteracts = new List<int>();
    private int targetInteractInd = -1;

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
        GetInRangeInteracts();
        int newTarget = GetTargetInteract();
        if (newTarget != targetInteractInd)
            UpdateTargetInteract(newTarget);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public override void Move(Vector3 velocity, bool turnToFace = true)
    {
        velocity = VelCamTransform(velocity * maxSpeed * (sprintActive ? sprintMultiplier : 1.0f));
        //transform.position += velocity;
        rb.velocity = velocity;
        if (turnToFace)
        {
            Vector2 vDir = new Vector2(velocity.x, velocity.z);
            float dir = vDir.Angle2D();
            RotateTo(dir, 0.1f);
        }
    }

    private Vector3 VelCamTransform(Vector3 velocity)
    {
        float m = velocity.magnitude;
        float velAngle = velocity.x >= 0.0f ? Vector3.Angle(Vector3.forward, velocity) : -Vector3.Angle(Vector3.forward, velocity);
        float camAngle = camDir.x >= 0.0f ? Vector3.Angle(Vector3.forward, camDir) : -Vector3.Angle(Vector3.forward, camDir);
        float angle = (velAngle + camAngle).WrapClamp(0.0f, 360.0f).ToRad();
        return new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * m;
    }

    public void AddInteraction(InteractPoint interaction)
    {
        interactions.Add(interaction);
    }

    public void RemoveInteraction(InteractPoint interaction)
    {
        if (interactions.Contains(interaction))
            interactions.Remove(interaction);
    }

    private void GetInRangeInteracts()
    {
        for (int i = 0; i < interactions.Count; i++)
        {
            if (interactions[i].inRange && interactions[i].distanceToPlayer > interactionRange)
            {
                inRangeInteracts.Remove(i);
                interactions[i].inRange = false;
            }
            else if (!interactions[i].inRange && interactions[i].distanceToPlayer <= interactionRange)
            {
                inRangeInteracts.Add(i);
                interactions[i].inRange = true;
            }
        }
    }

    private int GetTargetInteract()
    {
        float targetAngle = maxInteractAngle, angle;
        int targetIndex = -1;
        Vector3 dir;
        for (int i = 0; i < inRangeInteracts.Count; i++)
        {
            dir = (interactions[inRangeInteracts[i]].transform.position - transform.position).Flatten();
            angle = Vector3.Angle(dir, camDir);
            if (angle < targetAngle)
            {
                targetAngle = angle;
                targetIndex = inRangeInteracts[i];
            }
        }
        return targetIndex;
    }

    private void UpdateTargetInteract(int index)
    {
        if (index == -1)
        {
            GameManager.Instance.UIManager.hudManager.interactHLVisible = false;
        }
        else
        {
            GameManager.Instance.UIManager.hudManager.interactHLVisible = true;
        }
        targetInteractInd = index;
    }
}
