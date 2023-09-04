using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Maths;
using UnityEngine.InputSystem.HID;

public class PlayerCam : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] GameObject toFollow;
    [SerializeField] Transform pivot;
    public Camera cam;
    public Transform camTransform { get { return cam.transform; } }

    #endregion

    #region [ PROPERTIES ]

    private Vector3 rot = Vector3.zero;
    private Vector3 rotSpeed = Vector3.zero;
    public Vector3 facingVector { get { return GetFacingVector(); } }

    private Vector3 defaultDisp;
    private Vector3 defaultDir;
    private float defaultDist;

    [Range(0.0f, 10.0f)]
    [SerializeField] float returnEasing;
    private float maxDist;
    private float currentDist;
    private float targetDist;

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_returnToDefault = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        defaultDisp = camTransform.localPosition;
        defaultDir = camTransform.localPosition.normalized;
        defaultDist = camTransform.localPosition.magnitude;
        currentDist = camTransform.localPosition.magnitude;
    }

    void Start()
    {

    }

    void Update()
    {
        if (toFollow != null)
            transform.position = toFollow.transform.position;
        if (Cursor.lockState == CursorLockMode.Locked)
            Rotate(rotSpeed);

        Collision(pivot.TransformDirection(defaultDir));
        if (maxDist < currentDist)
            currentDist = maxDist;
        else if (maxDist > currentDist)
        {
            targetDist = currentDist + Time.deltaTime * returnEasing;
            currentDist = targetDist > maxDist ? maxDist : targetDist;
        }
        camTransform.localPosition = defaultDir * currentDist;
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

    private void Collision(Vector3 raycastDir)
    {
        RaycastHit hit;
        Physics.SphereCast(pivot.position, 0.3f, raycastDir, out hit, 30.0f);
        //Vector3 hitDisp = pivot.InverseTransformPoint(hit.point);
        if (hit.collider != null && hit.distance <= defaultDist)
            maxDist = hit.distance;
        else
            maxDist = defaultDist;
    }
}
