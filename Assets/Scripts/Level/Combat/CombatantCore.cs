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

    public CombatantData baseData;
    public EntityModel modelObj;

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
    public CombatSpeed speed = new CombatSpeed();

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

    public virtual void GetData(CombatantData data)
    {
        if (data != null)
        {
            baseData = data;

            GameObject modelTemplate = Resources.Load<GameObject>(EntityModel.GetModelPathFromUID(data.modelHexUID));
            modelObj = Instantiate(modelTemplate, transform).GetComponent<EntityModel>();

            health.valueBase = baseData.baseHealth;
            health.scaling = baseData.healthScaling;
            attack.valueBase = baseData.baseAttack;
            attack.scaling = baseData.attackScaling;
            defence.valueBase = baseData.baseDefence;
            defence.scaling = baseData.defenceScaling;
        }
    }
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

public class SpeedAtLevel
{
    public ushort levelThreshold;
    public int value;

    public SpeedAtLevel(ushort levelThreshold, int value)
    {
        this.levelThreshold = levelThreshold;
        this.value = value;
    }

    public static SpeedAtLevel Default { get { return new SpeedAtLevel(0, 1); } }
}

public class CombatSpeed
{
    private List<SpeedAtLevel> speeds = new List<SpeedAtLevel>();

    public CombatSpeed()
    {
        speeds.Add(SpeedAtLevel.Default);
    }
    
    public int Overwrite(SpeedAtLevel[] newSpeeds)
    {
        int i;
        speeds.Clear();
        speeds.Add(SpeedAtLevel.Default);
        if (newSpeeds != null && newSpeeds.Length > 0)
        {
            foreach (SpeedAtLevel speed in newSpeeds)
            {
                if (speeds.Count > 0)
                {
                    for (i = 0; i < speeds.Count; i++)
                    {
                        if (speed.levelThreshold < speeds[i].levelThreshold)
                        {
                            speeds.Insert(i, speed);
                            break;
                        }
                        else if (speed.levelThreshold == speeds[i].levelThreshold)
                        {
                            if (speed.value > speeds[i].value)
                                speeds[i] = speed;
                            break;
                        }
                        else if (i == speeds.Count - 1)
                        {
                            speeds.Add(speed);
                        }
                    }
                }
                else
                {
                    speeds.Add(speed);
                }
            }
        }
        return speeds.Count;
    }
    
    public int Overwrite(List<SpeedAtLevel> newSpeeds)
    {
        int i;
        speeds.Clear();
        speeds.Add(SpeedAtLevel.Default);
        if (newSpeeds != null && newSpeeds.Count > 0)
        {
            foreach (SpeedAtLevel speed in newSpeeds)
            {
                if (speeds.Count > 0)
                {
                    for (i = 0; i < speeds.Count; i++)
                    {
                        if (speed.levelThreshold < speeds[i].levelThreshold)
                        {
                            speeds.Insert(i, speed);
                            break;
                        }
                        else if (speed.levelThreshold == speeds[i].levelThreshold)
                        {
                            if (speed.value > speeds[i].value)
                                speeds[i] = speed;
                            break;
                        }
                        else if (i == speeds.Count - 1)
                        {
                            speeds.Add(speed);
                        }
                    }
                }
                else
                {
                    speeds.Add(speed);
                }
            }
        }
        return speeds.Count;
    }

    public CombatSpeed(SpeedAtLevel[] speeds)
    {
        Overwrite(speeds);
    }
    
    public CombatSpeed(List<SpeedAtLevel> speeds)
    {
        Overwrite(speeds);
    }

    public bool Add(SpeedAtLevel newSpeed)
    {
        for (int i = 0; i < speeds.Count; i++)
        {
            if (newSpeed.levelThreshold < speeds[i].levelThreshold)
            {
                speeds.Insert(i, newSpeed);
                return false;
            }
            else if (newSpeed.levelThreshold == speeds[i].levelThreshold)
            {
                if (newSpeed.value > speeds[i].value)
                {
                    speeds[i] = newSpeed;
                    return true;
                }
                break;
            }
            else if (i == speeds.Count - 1)
            {
                speeds.Add(newSpeed);
                return false;
            }
        }
        return false;
    }

    public bool Remove(ushort levelThreshold)
    {
        for (int i = speeds.Count - 1; i > 0; i--)
        {
            if (levelThreshold == speeds[i].levelThreshold)
            {
                speeds.RemoveAt(i);
                return true;
            }
            else if (levelThreshold < speeds[i].levelThreshold)
            {
                break;
            }
        }
        return false;
    }

    public int GetValue(ushort level)
    {
        for (int i = 0; i < speeds.Count; i++)
        {
            if (level >= speeds[i].levelThreshold)
            {
                return speeds[i].value;
            }
        }
        return 1;
    }

    public SpeedAtLevel[] GetList()
    {
        return speeds.ToArray();
    }
}

public class CombatEquipment
{

}

[System.Serializable]
public class CombatantBrain
{

}
