namespace NeoCambion.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class GraphicColorCycle : MonoBehaviourExt
    {
        public Graphic target;
        public ColorCyclePoint[] cycle = new ColorCyclePoint[] { new ColorCyclePoint(Color.white, 5f) };
        public bool unscaledDeltaTime = true;
        public float waitAtPeaks = 0f;
        public bool randomTimes = false;
        public FloatRange randomRange = new FloatRange(0.4f, 2.0f);

        private int current, next;
        private float deltaTime => unscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
        private float t, tMax, delta;
        private bool cycleEnabled;

        void Awake()
        {
            cycleEnabled = cycle != null && cycle.Length > 1 && target != null;
            if (enabled)
            {
                current = 0;
                next = 1;
                t = 0f;
                tMax = randomTimes ? Random.Range(randomRange.lower, randomRange.upper) : cycle[current].timeToNext;
                delta = 0f;
            }
        }

        void Update()
        {
            if (cycleEnabled)
            {
                t += deltaTime;
                delta = (t < 0 ? 0 : t) / tMax;
                if (t >= tMax)
                    AdvanceCycle();
                else
                    target.color = Color.Lerp(cycle[current].color, cycle[next].color, delta);
            }
        }

        private void AdvanceCycle()
        {
            target.color = cycle[next].color;
            if (++current >= cycle.Length)
                current = 0;
            if (++next >= cycle.Length)
                next = 0;
            t = 0f - waitAtPeaks;
            tMax = tMax = randomTimes ? Random.Range(randomRange.lower, randomRange.upper) : cycle[current].timeToNext;
        }
    }

    [System.Serializable]
    public struct ColorCyclePoint
    {
        public Color color;
        public float timeToNext;

        public ColorCyclePoint(Color color, float timeToNext)
        {
            this.color = color;
            this.timeToNext = timeToNext;
        }
    }
}
