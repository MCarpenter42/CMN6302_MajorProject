using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Maths;
using NeoCambion.Unity;
using System;

public class WorldPlayer : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]

    private List<InteractPoint> interactions = new List<InteractPoint>();
    public InteractPoint targetInteract { get { return targetInteractInd > -1 ? interactions[targetInteractInd] : null; } }

    #endregion

    #region [ PROPERTIES ]

    private Vector3 camDir { get { return UnityExt_Vector3.Flatten(GameManager.Instance.cameraW.facingVector); } }

    [Header("Player")]
    [SerializeField] Vector3 interactCentre;

    public Vector3 posInteract => transform.position + interactCentre;

    public float sprintMultiplier = 2.0f;
    [HideInInspector] public bool sprintActive = false;

    [SerializeField] float interactionRange = 3.0f;
    [SerializeField] float maxInteractAngle = 40.0f;
    private List<int> inRangeInteracts = new List<int>();
    private int targetInteractInd = int.MaxValue;

    [HideInInspector] public CombatantData[] playerCharacters = null;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Awake()
    {
        base.Awake();
        //playerCharacters = INDEV_PlayerCharacters();
    }

    protected override void Start()
    {
        base.Start();
        GetInRangeInteracts();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleInteractions();
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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void HandleInteractions()
    {
        GetInRangeInteracts();
        int newTarget = GetTargetInteract();
        if (newTarget != targetInteractInd)
            UpdateTargetInteract(newTarget);
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

    private bool IsInteractVisible(InteractPoint interact)
    {
        Vector3 rayDir = interact.transform.position - posInteract;
        return !Physics.Raycast(transform.position, rayDir.normalized, rayDir.magnitude);
    }

    private int GetTargetInteract()
    {
        float targetAngle = maxInteractAngle, angle;
        int targetIndex = -1;
        Vector3 dir;
        for (int i = 0; i < inRangeInteracts.Count; i++)
        {
            if (IsInteractVisible(interactions[inRangeInteracts[i]]))
            {
                dir = (interactions[inRangeInteracts[i]].transform.position - posInteract).Flatten();
                angle = Vector3.Angle(dir, camDir);
                if (angle < targetAngle)
                {
                    targetAngle = angle;
                    targetIndex = inRangeInteracts[i];
                }
            }
        }
        return targetIndex;
    }

    private void UpdateTargetInteract(int index)
    {
        UIManager.HUD.SetInteractHightlightVis(index > -1 ? true : false);
        targetInteractInd = index;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private CombatantData[] INDEV_PlayerCharacters()
    {
        CombatantData[] data = new CombatantData[3]
        {
            new CombatantData() { displayName = "Player Character 1" },
            new CombatantData() { displayName = "Player Character 2" },
            new CombatantData() { displayName = "Player Character 3" }
        };

        data[0].modelHexUID = "8663CC29";
        data[1].modelHexUID = "8663CC29";
        data[2].modelHexUID = "8663CC29";
        
        data[0].friendly = true;
        data[1].friendly = true;
        data[2].friendly = true;
        data[0].playerControlled = true;
        data[1].playerControlled = true;
        data[2].playerControlled = true;

        data[0].baseHealth = 80;
        data[0].healthScaling = 40;
        data[1].baseHealth = 80;
        data[1].healthScaling = 40;
        data[2].baseHealth = 80;
        data[2].healthScaling = 40;

        data[0].baseAttack = 30;
        data[0].attackScaling = 40;
        data[1].baseAttack = 30;
        data[1].attackScaling = 40;
        data[2].baseAttack = 30;
        data[2].attackScaling = 40;

        data[0].baseDefence = 40;
        data[0].defenceScaling = 40;
        data[1].baseDefence = 40;
        data[1].defenceScaling = 40;
        data[2].baseDefence = 40;
        data[2].defenceScaling = 40;

        data[0].speeds = new SpeedAtLevel[]
        {
            new SpeedAtLevel(0, 130)
        };
        data[1].speeds = new SpeedAtLevel[]
        {
            new SpeedAtLevel(0, 120)
        };
        data[2].speeds = new SpeedAtLevel[]
        {
            new SpeedAtLevel(0, 110)
        };

        return data;
    }
}
