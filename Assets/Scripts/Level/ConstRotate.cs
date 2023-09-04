using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion.Maths;

public class ConstRotate : Core
{
    [SerializeField] float rotSpeed;
    private Vector3 rot;

    void Update()
    {
        rot = transform.localEulerAngles;
        rot.y = (rot.y + rotSpeed * Time.deltaTime).WrapClamp(0f, 360f);
        transform.localEulerAngles = rot;
    }
}
