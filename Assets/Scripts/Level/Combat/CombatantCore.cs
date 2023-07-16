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
using System.Runtime.CompilerServices;

public class CombatantCore : Core
{
    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ PROPERTIES ]

    public ushort level;
    private CombatValue[] values = new CombatValue[3];
    public CombatValue health
    {
        get
        {
            if (values[0] == null)
                values[0] = new CombatValue(this);
            return values[0];
        }
    }
    public CombatValue attack
    {
        get
        {
            if (values[1] == null)
                values[1] = new CombatValue(this);
            return values[1];
        }
    }
    public CombatValue defence
    {
        get
        {
            if (values[1] == null)
                values[1] = new CombatValue(this);
            return values[1];
        }
    }

    public CombatEquipment equipment;

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

public class CombatValue
{
    public static float ScaledFloat(int baseValue, ushort level, float scalingPercent)
    {
        float scaling = Mathf.Clamp(scalingPercent, 0.0f, 1.0f) * 0.06f;
        level -= (ushort)(level > 0 ? 1 : 0);
        return (float)baseValue * Mathf.Exp(scaling * (float)level);
    }

    public static int ScaledInt(int baseValue, ushort level, float scalingPercent)
    {
        level -= (ushort)(level > 0 ? 1 : 0);
        return Mathf.RoundToInt(ScaledFloat(baseValue, level, scalingPercent));
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public CombatantCore combatant;
    private int _level;
    public int level
    {
        get { return combatant == null ? _level : combatant.level; }
        set { _level = value; }
    }

    private int[] values = new int[3] { -1, -1, -1 };
    public int valueBase
    {
        get
        {
            return values[0];
        }
        set
        {
            values[0] = value;
            values[1] = modifiers.Modify(valueBase);
        }
    }
    public int valueScaled
    {
        get
        {
            if (modifiers.newChanges)
            {
                values[1] = modifiers.Modify(valueBase);
            }
            return values[1];
        }
    }
    public int valueCurrent
    {
        get { return values[2]; }
        set { values[2] = value; }
    }
    public int scaling;

    public ModifiersInt modifiers = new ModifiersInt();

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public CombatValue(CombatantCore combatant)
    {
        this.combatant = combatant;
        level = 1;
    }
    
    public CombatValue(int level)
    {
        combatant = null;
        this.level = level;
    }
}

public class CombatEquipment
{

}

[System.Serializable]
public class CombatantBrain
{

}
