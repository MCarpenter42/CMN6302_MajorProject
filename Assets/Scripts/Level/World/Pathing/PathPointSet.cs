using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Unity;

public class PathPointSet : Core
{
    #region [ OBJECTS / COMPONENTS ]

    private List<GameObject> pathPointObjects = new List<GameObject>();
    [HideInInspector] public List<Vector3> pathPoints = new List<Vector3>();

    #endregion

    #region [ PROPERTIES ]



    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        pathPointObjects.Clear();
        pathPoints.Clear();
        pathPointObjects = gameObject.GetChildrenWithComponent<PathPoint>();
        foreach (GameObject obj in pathPointObjects)
        {
            pathPoints.Add(obj.transform.position);
        }
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


}
