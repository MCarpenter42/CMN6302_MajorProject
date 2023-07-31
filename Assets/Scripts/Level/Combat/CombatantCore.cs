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
using UnityEngine.UIElements;

public class CombatantCore : Core
{
    #region [ OBJECTS / COMPONENTS ]

    public CombatantData baseData;
    public EntityModel modelObj;

    #endregion

    #region [ PROPERTIES ]

    public bool gotData;
    public bool isFriendly;

    public int size { get { return modelObj == null ? 0 : modelObj.size; } }

    public ushort level;
    public CombatValue health = null;
    public CombatValue attack = null;
    public CombatValue defence = null;
    public CombatSpeed speed = null;

    public CombatEquipment equipment;

    public bool alive = true;

    public float lastActionTime = 0.0f;
    public float actionInterval
    {
        get
        {
            return speed == null ? float.MaxValue : CombatManager.TimeFactor / speed.Current;
        }
    }
    public float nextActionTime
    {
        get
        {
            return speed == null ? float.MaxValue : lastActionTime + actionInterval;
        }
    }

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
        gotData = data != null;
        if (gotData)
        {
            baseData = data;

            string modelPath = EntityModel.GetModelPathFromUID(data.modelHexUID);
            if (modelPath != null)
            {
                GameObject modelTemplate = Resources.Load<GameObject>(modelPath);
                modelObj = modelTemplate == null ? null : Instantiate(modelTemplate, transform).GetComponent<EntityModel>();
            }

            isFriendly = baseData.isFriendly;

            health = new CombatValue(this, baseData.baseHealth, baseData.healthScaling);
            attack = new CombatValue(this, baseData.baseAttack, baseData.attackScaling);
            defence = new CombatValue(this, baseData.baseDefence, baseData.defenceScaling);
            speed = new CombatSpeed(this, baseData.speeds);
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
    private ushort _level;
    public ushort level
    {
        get { return combatant == null ? _level : combatant.level; }
        set { _level = value; }
    }

    private int _base = -1;
    public int Base
    {
        get
        {
            if (_base < 0)
                _base = 0;
            return _base;
        }
        set
        {
            _base = value;
        }
    }
    private float _scaled = -1;
    public float ScaledAsFloat
    {
        get
        {
            if (_scaled < 0)
                _scaled = modifiers.Modify(ScaledFloat(Base, level, (float)Scaling / 100.0f));
            return _scaled;
        }
    }
    public int Scaled
    {
        get
        {
            return Mathf.RoundToInt(ScaledAsFloat);
        }
    }
    private int _current = -1;
    public int Current
    {
        get
        {
            if (_current < 0)
                _current = 0;
            return _current;
        }
        set { _current = value < 0 ? 0 : value; }
    }
    public int _scaling;
    public int Scaling
    {
        get => _scaling;
        set { _scaling = value < 0 ? 0 : (value > 100 ? 100 : value); }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public CombatValue(CombatantCore combatant, int Base = 1, int Scaling = 0)
    {
        this.combatant = combatant;
        this.Base = Base;
        this.Scaling = Scaling;
        level = 1;
    }
    
    public CombatValue(ushort level, int Base = 1, int Scaling = 0)
    {
        combatant = null;
        this.Base = Base;
        this.Scaling = Scaling;
        this.level = level;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private ModifiersFloat _modifiers = null;
    public ModifiersFloat modifiers
    {
        get
        {
            if (_modifiers == null)
                _modifiers = new ModifiersFloat(ModifierChanged);
            return _modifiers;
        }
    }

    public void ModifierChanged()
    {
        _scaled = modifiers.Modify(ScaledFloat(Base, level, (float)Scaling / 100.0f));
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
    public CombatantCore combatant = null;
    private ushort _level;
    public ushort level
    {
        get { return combatant == null ? _level : combatant.level; }
        set { _level = value; }
    }

    private List<SpeedAtLevel> speeds = new List<SpeedAtLevel>();

    public CombatSpeed(CombatantCore combatant)
    {
        this.combatant = combatant;
        level = 1;
        speeds.Add(SpeedAtLevel.Default);
    }
    
    public CombatSpeed(CombatantCore combatant, SpeedAtLevel[] speeds)
    {
        this.combatant = combatant;
        level = 1;
        Overwrite(speeds);
    }
    
    public CombatSpeed(CombatantCore combatant, List<SpeedAtLevel> speeds)
    {
        this.combatant = combatant;
        level = 1;
        Overwrite(speeds);
    }
    
    public CombatSpeed(ushort level)
    {
        combatant = null;
        this.level = level;
        speeds.Add(SpeedAtLevel.Default);
    }
    
    public CombatSpeed(ushort level, SpeedAtLevel[] speeds)
    {
        combatant = null;
        this.level = level;
        Overwrite(speeds);
    }
    
    public CombatSpeed(ushort level, List<SpeedAtLevel> speeds)
    {
        combatant = null;
        this.level = level;
        Overwrite(speeds);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool IndexInBounds(int ind)
    {
        return speeds.InBounds(ind);
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
    
    public bool RemoveAt(int index)
    {
        if (speeds.InBounds(index))
        {
            speeds.RemoveAt(index);
            return true;
        }
        return false;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public int GetAtLevel(ushort level)
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

    public SpeedAtLevel this[int index]
    {
        get
        {
            if (speeds.InBounds(index))
                return speeds[index];
            else
                return null;
        }
    }

    public SpeedAtLevel[] GetList()
    {
        return speeds.ToArray();
    }

    public int valueCount { get { return speeds.Count; } }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private int _current = 0;
    public int Current
    {
        get
        {
            if (_current < 1)
                _current = 1;
            return _current;
        }
        set
        {
            _current = value < 1 ? 1 : value;
        }
    }

    private ModifiersFloat _modifiers = null;
    public ModifiersFloat modifiers
    {
        get
        {
            if (_modifiers == null)
                _modifiers = new ModifiersFloat(ModifierChanged);
            return _modifiers;
        }
    }

    public void ModifierChanged()
    {
        Current = Mathf.RoundToInt(modifiers.Modify(GetAtLevel(level)));
    }
}

public class CombatEquipment
{

}

[System.Serializable]
public class CombatantBrain
{

}
