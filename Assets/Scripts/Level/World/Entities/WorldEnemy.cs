using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.AI;
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
//using NeoCambion.Random;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;

[RequireComponent(typeof(NavMeshAgent))]
public class WorldEnemy : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]

    private NavMeshAgent navAgent;
    private PathingHandler pathHandler;

    #endregion

    #region [ PROPERTIES ]

    [Header("World Options")]
    [SerializeField] bool roam = true;
    private float moveDir = 0.0f;

    private Vector3 lastPos = Vector3.zero;
    private float targetRot = 0.0f;

    private List<int> availableRooms = new List<int>();
    private int targetRoom = -1, targetPoint = -1;
    [HideInInspector] public bool navigating = false;
    private bool gettingDestination = false;

    [Header("Combat Options")]
    [Range(1, 30)]
    public int level = 1;
    public EnemyType[] enemyTypes;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Awake()
    {
        base.Awake();
        navAgent = GetComponent<NavMeshAgent>();
        pathHandler = FindObjectOfType<PathingHandler>();
    }

    protected override void Start()
    {
        base.Start();
        GameManager.Instance.enemyListW.Add(this);
        int initialRoom = GetInitialRoom();
        int startPoint = Random.Range(0, pathHandler.pointSets[initialRoom].pathPoints.Count);
        transform.position = pathHandler.pointSets[initialRoom].pathPoints[startPoint];
        availableRooms.Add(initialRoom);
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (roam && !navAgent.isStopped)
        {
            if (!navAgent.hasPath && !gettingDestination)
                GetNewTarget();
            /*else
                UpdateRotation();*/
        }
        lastPos = transform.position;
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private int GetInitialRoom()
    {
        int initialRoom = -1;
        Vector3 roomMin, roomMax;
        for (int i = 0; i < pathHandler.pointSets.Count; i++)
        {
            roomMin = pathHandler.pointSets[i].boundaryArea.position - pathHandler.pointSets[i].boundaryArea.localScale / 2.0f;
            roomMax = roomMin + pathHandler.pointSets[i].boundaryArea.localScale;
            bool xInRange = transform.position.x >= roomMin.x && transform.position.x <= roomMax.x;
            bool yInRange = transform.position.y >= roomMin.y && transform.position.y <= roomMax.y;
            bool zInRange = transform.position.z >= roomMin.z && transform.position.z <= roomMax.z;
            if (xInRange && yInRange && zInRange)
            {
                return i;
            }
        }
        if (initialRoom == -1)
        {
            initialRoom = Random.Range(0, pathHandler.pointSets.Count);
        }
        return initialRoom;
    }

    /*private void UpdateRotation()
    {
        Vector3 v = transform.position - lastPos;
        Vector2 vDir = new Vector2(v.x, v.z);
        float vFacing = vDir.Angle2D();
        if (Mathf.Abs(vFacing - targetRot.WrapClamp(-180.0f, 180.0f)) > 2.0f && Mathf.Abs(vFacing.WrapClamp(0.0f, 360.0f) - targetRot) > 2.0f)
        {
            targetRot = vFacing;
            RotateTo(vFacing, 0.1f);
        }
    }*/

    private void GetNewTarget()
    {
        gettingDestination = true;
        StartCoroutine(IGetNewTarget());
    }

    private IEnumerator IGetNewTarget()
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 3.0f));
        targetRoom = Random.Range(0, availableRooms.Count);
        targetPoint = Random.Range(0, pathHandler.pointSets[availableRooms[targetRoom]].pathPoints.Count);
        //Debug.Log(availableRooms[targetRoom] + ", " + targetPoint);
        navAgent.SetDestination(pathHandler.pointSets[availableRooms[targetRoom]].pathPoints[targetPoint]);
        gettingDestination = false;
    }

    public void GoToCombat()
    {
        GameManager.Instance.OnCombatStart(gameObject.GetComponent<WorldEnemy>());
    }

    public void PauseBehaviour(bool pause)
    {
        if (pause)
        {
            navAgent.isStopped = true;
        }
        else
        {
            navAgent.isStopped = false;
        }
    }
}
