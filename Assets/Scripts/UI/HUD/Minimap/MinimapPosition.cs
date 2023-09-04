using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapPosition : Core
{
    WorldPlayer player;
    Vector3 pos = Vector3.zero;

    void Start()
    {
        player = GameManager.Instance.playerW;
    }

    void Update()
    {
        pos.x = player.transform.position.x;
        pos.z = player.transform.position.z;
        transform.localPosition = pos;
    }
}
