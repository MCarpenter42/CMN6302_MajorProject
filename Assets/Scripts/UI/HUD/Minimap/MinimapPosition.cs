using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion.Unity;

public class MinimapPosition : Core
{
    [SerializeField] Transform viewCone;
    private WorldPlayer player;
    private Transform camPivot;
    private Vector3 pos = Vector3.zero;
    private Vector3 rot = Vector3.zero;
    private bool initialised = false;

    protected override void Initialise()
    {
        player = GameManager.Instance.Player.ExceptionIfNotFound(ObjectSearchException.Attribute);
        camPivot = GameManager.Instance.WorldCamPivot.pivot;
        initialised = true;
    }

    void Update()
    {
        if (initialised)
        {
            pos.x = player.transform.position.x;
            pos.z = player.transform.position.z;
            transform.localPosition = pos;
            rot.y = camPivot.localEulerAngles.y;
            viewCone.localEulerAngles = rot;
        }
    }
}
