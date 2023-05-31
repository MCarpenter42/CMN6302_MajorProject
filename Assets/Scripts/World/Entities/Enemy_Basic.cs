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
public class Enemy_Basic : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]

    private NavMeshAgent navAgent;
    private PathingHandler pathHandler;

    #endregion

    #region [ PROPERTIES ]

    [SerializeField] bool move = true;
    private float moveDir = 0.0f;

    private List<int> availableRooms = new List<int>();
    private int targetRoom = -1, targetPoint = -1;
    [HideInInspector] public bool navigating = false;
    private bool gettingDestination = false;

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
        if (move)
        {
            if (!navAgent.hasPath && !gettingDestination)
                GetNewTarget();
            else
                UpdateRotation();
        }
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
            bool xInRange = transform.position.x >= roomMin.x && transform.position.x <= roomMin.x;
            bool yInRange = transform.position.y >= roomMin.y && transform.position.y <= roomMin.y;
            bool zInRange = transform.position.z >= roomMin.z && transform.position.z <= roomMin.z;
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

    private float rotFactor = 15.0f;

    private void UpdateRotation()
    {
        Vector3 v = navAgent.nextPosition - transform.position;
        Vector2 vDir = new Vector2(v.x, v.z);
        float vFacing = vDir.Angle2D();
        facing = transform.eulerAngles.y.WrapClamp(-180.0f, 180.0f);
        float fDiff = (vFacing - facing);
        Debug.Log(vDir + ", " + vFacing + ", " + facing + ", " + fDiff);
        Rotate(fDiff > rotFactor * Time.fixedDeltaTime ? rotFactor * Time.fixedDeltaTime : fDiff);
    }

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
}
