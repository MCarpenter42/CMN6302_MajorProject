using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Maths;
using NeoCambion.Unity;

public class WorldPlayer : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]

    private List<InteractPoint> interactions = new List<InteractPoint>();
    public InteractPoint targetInteract { get { return targetInteractInd > -1 ? interactions[targetInteractInd] : null; } }

    #endregion

    #region [ PROPERTIES ]

    private Vector3 camDir { get { return UnityExt_Vector3.Flatten(GameManager.Instance.cameraW.facingVector); } }

    [Header("Player")]
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

    private bool IsInteractVisible(InteractPoint interact)
    {
        Vector3 rayDir = interact.transform.position - transform.position;
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
                dir = (interactions[inRangeInteracts[i]].transform.position - transform.position).Flatten();
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
        if (index == -1)
        {
            GameManager.Instance.UI.HUD.interactHLVisible = false;
        }
        else
        {
            GameManager.Instance.UI.HUD.interactHLVisible = true;
        }
        targetInteractInd = index;
    }
}
