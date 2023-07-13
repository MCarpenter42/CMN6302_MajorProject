using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.IO;
using NeoCambion.IO.Unity;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Random.Unity;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;
using NeoCambion.Unity.Interpolation;

public class CombatantCore : Core
{
    public struct HealthValues
    {
        public int baseValue;
        public int max;
        public int current;
        public float scalingPercent;

        public HealthValues(int baseValue, float scalingPercent = 0.2f)
        {
            this.baseValue = baseValue;
            max = baseValue;
            current = baseValue;
            this.scalingPercent = scalingPercent;
        }

        private static float ScaledFloat(int baseValue, ushort level, float scalingPercent)
        {
            float scaling = Mathf.Clamp(scalingPercent, 0.0f, 1.0f) * 0.06f;
            level -= (ushort)(level > 0 ? 1 : 0);
            return (float)baseValue * Mathf.Exp(0.01f * scaling * (float)level);
        }
        
        public static int ScaledValue(int baseValue, ushort level, float scalingPercent)
        {
            level -= (ushort)(level > 0 ? 1 : 0);
            return Mathf.RoundToInt(ScaledFloat(baseValue, level, scalingPercent));
        }

        public int RecalculateMax(ushort level, ItemCore[] equipment = null)
        {
            level -= (ushort)(level > 0 ? 1 : 0);
            float multiply = 1.0f;
            int add = 0;
            if (equipment != null && equipment.Length > 0)
            {
                
            }
            max = Mathf.RoundToInt(ScaledFloat(baseValue, level, scalingPercent) * multiply) + add;
            return max;
        }
    }

    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ PROPERTIES ]

    public HealthValues health;

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

    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */


}
