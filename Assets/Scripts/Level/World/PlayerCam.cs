using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Maths;
using NeoCambion.Unity;

public class PlayerCam : PivotArmCamera
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] GameObject toFollow;

    #endregion

    #region [ PROPERTIES ]

    private Vector3 rot = Vector3.zero;
    private Vector3 rotSpeed = Vector3.zero;
    public Vector3 facingVector { get { return GetFacingVector(); } }

    private float defaultDist;

    [Range(0.0f, 10.0f)]
    [SerializeField] float returnEasing;
    private float maxDist;
    private float currentDist;
    private float targetDist;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Awake()
    {
        base.Awake();
        defaultDist = camOffset;
        currentDist = camOffset;
    }

    protected override void Update()
    {
        base.Update();
        if (Application.isPlaying)
        {
            if (toFollow != null)
                transform.position = toFollow.transform.position;
            if (Cursor.lockState == CursorLockMode.Locked)
                Rotate(rotSpeed);

            Collision(pivot.TransformDirection(cameraTransform.localPosition));
            if (maxDist < currentDist)
                camOffset = maxDist;
            else if (maxDist > camOffset)
            {
                targetDist = camOffset + Time.deltaTime * returnEasing;
                camOffset = targetDist > maxDist ? maxDist : targetDist;
            }
        }
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public Vector3 GetFacingVector()
    {
        return pivot.TransformDirection(Vector3.forward);
    }

    public void Rotate(Vector3 angle)
    {
        rot.x = Mathf.Clamp(rot.x + angle.x, -40.0f, 80.0f);
        rot.y = (rot.y + angle.y).WrapClamp(0.0f, 360.0f);
        eulerAngles = rot;
    }

    public void SetRotSpeed(Vector3 rotation)
    {
        rotSpeed = rotation;
    }

    private void Collision(Vector3 raycastDir)
    {
        RaycastHit hit;
        Physics.SphereCast(pivot.position, 0.3f, raycastDir, out hit, 30.0f);
        if (hit.collider != null && hit.distance <= defaultDist)
            maxDist = hit.distance;
        else
            maxDist = defaultDist;
    }
}
