using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : Core
{
    [SerializeField] Transform spawnPosAnchor;
    [SerializeField] Transform modelTransform;

    public void Trigger(WorldPlayer player)
    {
        player.transform.position = (spawnPosAnchor != null) ? spawnPosAnchor.position : transform.position;
        StartCoroutine(IModelDescend());
    }

    private IEnumerator IModelDescend()
    {
        yield return new WaitForSeconds(1.0f);
        Vector3 posStart = modelTransform.position, posTarget = modelTransform.position - 3.0f * Vector3.up;
        float t = 0.0f, tMax = 3.0f, delta;
        while (t < tMax)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / tMax;
            modelTransform.position = Vector3.Lerp(posStart, posTarget, delta);
        }
        modelTransform.position = posTarget;
    }
}
