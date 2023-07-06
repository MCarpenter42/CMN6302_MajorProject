using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Maths;

public class PlayerCam : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] GameObject toFollow;
    [SerializeField] GameObject pivot;
    public Camera cam;

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
