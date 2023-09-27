using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion.Interpolation;
using NeoCambion.Interpolation.Unity;

public class StageStart : Core
{
    [System.Serializable]
    public class RotatorRing
    {
        public Transform transform;
        public Material material { get { return transform.GetComponent<MeshRenderer>().sharedMaterial; } set { transform.GetComponent<MeshRenderer>().sharedMaterial = value; } }

        public Vector3 scale { get { return transform.localScale; } set { transform.localScale = value; } }
        public Vector3 baseScale { get; private set; }

        public float rotRate;
        public float baseRate { get; private set; }

        public Vector3 position { get { return transform.localPosition; } set { transform.localPosition = value; } }
        public Vector3 basePosition { get; private set; }

        public float rotation { get { return transform.localEulerAngles.y; } set { transform.localEulerAngles = value * Vector3.up; } }

        public RotatorRing(Transform transform, float rotRate)
        {
            this.transform = transform;
            this.rotRate = rotRate;
            GetBase();
        }

        public void GetBase()
        {
            baseRate = rotRate;
            baseScale = transform.localScale;
            basePosition = transform.localPosition;
        }
    }

    [SerializeField] Transform spawnPosAnchor;
    [SerializeField] float animDelay = 1f;
    [SerializeField] float animDuration = 4f;
    [SerializeField] RotatorRing[] rings;

    private bool rotate = true;

    private Material localMat;
    private Color matClr;
    private Color matClrTransp;
    private Color emitClr;
    private Color emitClrTransp;

    protected override void Initialise()
    {
        foreach (RotatorRing ring in rings)
        {
            ring.GetBase();
        }
        localMat = new Material(rings[0].material);
        matClr = localMat.color;
        matClrTransp = new Color() { r = matClr.r, g = matClr.g, b = matClr.b, a = 0f };
        emitClr = localMat.GetColor("_EmissionColor");
        matClrTransp = new Color() { r = emitClr.r, g = emitClr.g, b = emitClr.b, a = 0f };
        foreach (RotatorRing ring in rings)
            ring.material = localMat;
    }

    void Update()
    {
        if (rotate)
        {
            foreach (RotatorRing ring in rings)
            {
                ring.rotation += ring.rotRate * Time.deltaTime;
            }
        }
    }

    public void Trigger(WorldPlayer player, float delayOverride = -1f)
    {
        float delay = delayOverride >= 0f ? delayOverride : animDelay;
        player.transform.position = (spawnPosAnchor != null) ? spawnPosAnchor.position : transform.position;
        StartCoroutine(SpawnSequence(delay, animDuration));
    }

    private IEnumerator SpawnSequence(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        Vector3 scaleTarget = new Vector3(1f, 0f, 1f);
        float t = 0f, delta;
        while (t <= duration)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / duration;
            foreach (RotatorRing ring in rings)
            {
                localMat.color = Color.Lerp(matClr, matClrTransp, InterpDelta.CosSpeedUp(delta));
                localMat.SetColor("_EmissionColor", Color.Lerp(emitClr, emitClrTransp, InterpDelta.CosSpeedUp(delta)));
                ring.scale = ring.baseScale.Lerp(scaleTarget, delta);
            }
        }
        rotate = false;
        yield return null;
        foreach (RotatorRing ring in rings)
        {
            ring.transform.gameObject.SetActive(false);
        }

        GameManager.lockPlayerPosition = false;

        /*yield return new WaitForSeconds(delay);

        Vector3 scaleTarget = new Vector3(1f, 0f, 1f);
        float t = 0f, delta, deltaB;
        while (t <= duration)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / duration;
            deltaB = delta > 0.5f ? (delta - 0.5f) / 0.5f : 0f;
            foreach (RotatorRing ring in rings)
            {
                ring.rotRate = ring.baseRate.Lerp(0f, delta);
                ring.scale = ring.baseScale.Lerp(scaleTarget, deltaB);
            }
        }
        rotate = false;
        yield return null;
        foreach (RotatorRing ring in rings)
        {
            ring.transform.gameObject.SetActive(false);
        }

        GameManager.lockPlayerPosition = false;*/
    }
}
