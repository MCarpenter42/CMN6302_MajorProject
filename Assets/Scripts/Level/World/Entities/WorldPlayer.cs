using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Maths;
using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class WorldPlayer : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]

    [HideInInspector] public InteractPoint[] interactions = new InteractPoint[0];
    [HideInInspector] public bool[] inRange = new bool[0];
    public InteractPoint targetInteract => targetInteractInd > -1 ? interactions[targetInteractInd] : null;

    #endregion

    #region [ PROPERTIES ]

    private Camera cam => GameManager.Instance.WorldCam;
    private Vector3 camDir => new Vector3(cam.transform.forward.x, 0f, cam.transform.forward.z).normalized;

    [Header("Player")]
    [SerializeField] Vector3 interactCentre;

    public Vector3 posInteract => transform.position + interactCentre;

    public Vector3 flatPosition { get { return new Vector3(transform.position.x, 0f, transform.position.z); } }

    public float sprintMultiplier = 2.0f;
    [HideInInspector] public bool sprintActive = false;

    public bool posLocked { get { return GameManager.lockPlayerPosition; } }

    [SerializeField] float interactionRange = 3.0f;
    [SerializeField] float maxInteractAngle = 40.0f;
    private int targetInteractInd = int.MaxValue;

    [HideInInspector] public CombatantData[] playerCharacters = null;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (interactions.Length > 0)
            HandleInteractions();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public override void Move(Vector3 velocity, bool turnToFace = true)
    {
        if (!posLocked)
        {
            velocity = VelCamTransform(velocity * maxSpeed * (sprintActive ? sprintMultiplier : 1.0f));
            rb.velocity = velocity;
            if (turnToFace)
            {
                Vector2 vDir = new Vector2(velocity.x, velocity.z);
                float dir = vDir.Angle2D();
                RotateTo(dir, 0.1f);
            }
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

    private Vector3 DirectionTo(InteractPoint interact) => interact.flatPosition - flatPosition;
    public float DistanceTo(InteractPoint interact) => (interact.flatPosition - flatPosition).magnitude;

    private bool IsInteractVisible(InteractPoint interact)
    {
        Vector3 rayDir = interact.transform.position - posInteract;
        return !Physics.Raycast(transform.position, rayDir.normalized, rayDir.magnitude);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void FindInteractions()
    {
        StartCoroutine(IFindInteractions());
    }
    private IEnumerator IFindInteractions()
    {
        yield return null;
        interactions = FindObjectsOfType<InteractPoint>(false);
        inRange = new bool[interactions.Length];
    }
    public void FindInteractions(float delay)
    {
        StartCoroutine(IFindInteractions(delay));
    }
    private IEnumerator IFindInteractions(float delay)
    {
        yield return new WaitForSeconds(delay);
        interactions = FindObjectsOfType<InteractPoint>(false);
        inRange = new bool[interactions.Length];
    }

    public void ClearInteractions()
    {
        interactions = new InteractPoint[0];
        targetInteractInd = -1;
        UIManager.HUD.SetInteractHightlightVis(false);
    }

    public void HandleInteractions()
    {
        GetInRangeInteracts();
        int newTarget = GetTargetInteract();
        if (newTarget != targetInteractInd)
        {
            /*if (newTarget > -1)
                Debug.Log("Target interact: " + newTarget + " | Distance to target: " + DistanceTo(interactions[newTarget]));*/
            UpdateTargetInteract(newTarget);
        }
    }

    private void GetInRangeInteracts()
    {
        for (int i = 0; i < interactions.Length; i++)
        {
            inRange[i] = DistanceTo(interactions[i]) <= interactionRange;
        }
    }

    private int GetTargetInteract()
    {
        float targetAngle = maxInteractAngle, angle;
        int targetIndex = -1;
        Vector3 dir;
        for (int i = 0; i < interactions.Length; i++)
        {
            if (inRange[i])
            {
                if (IsInteractVisible(interactions[i]))
                {
                    dir = DirectionTo(interactions[i]);
                    angle = Vector3.Angle(dir, camDir);
                    if (angle < targetAngle)
                    {
                        targetAngle = angle;
                        targetIndex = i;
                    }
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

    public void TriggerTargetInteract() { if (targetInteract != null) targetInteract.Trigger(); }

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

#if UNITY_EDITOR
[CustomEditor(typeof(WorldPlayer))]
public class WorldPlayerEditor : Editor
{
    private WorldPlayer targ { get { return target as WorldPlayer; } }
    private Rect rect;

    private string entityName;
    private float distance;

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < targ.interactions.Length; i++)
            {
                entityName = targ.interactions[i].transform.parent.gameObject.name;
                rect = EditorElements.PrefixLabel(EditorElements.ControlRect(), entityName, 160, EditorStylesExtras.LabelStyle(TextAnchor.MiddleRight));
                rect.x += 8;
                rect.width -= 8;
                rect = EditorElements.PrefixLabel(rect, targ.inRange[i].ToString(), 40);
                EditorGUI.LabelField(rect, targ.DistanceTo(targ.interactions[i]).ToString());
            }
            EditorGUILayout.Space(10);
        }
        base.OnInspectorGUI();
    }
}
#endif
