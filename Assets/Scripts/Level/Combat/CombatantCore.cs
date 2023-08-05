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

// BASIC THREAT AND TAUNT SYSTEMS NEEDED
// MEMORY (ATTACKED BY + ETC) NEEDED

public class CombatantCore : Core
{
    #region [ OBJECTS / COMPONENTS ]

    public CombatantData baseData;
    public EntityModel modelObj;

    #endregion

    #region [ PROPERTIES ]

    public Vector3 pos { get { return gameObject.transform.position; } set { gameObject.transform.position = value; } }
    public Vector3 rot { get { return gameObject.transform.eulerAngles; } set { gameObject.transform.eulerAngles = value; } }

    public bool gotData;
    public bool isFriendly;
    public bool playerControlled = false;
    public int index = -1;

    public int size { get { return modelObj == null ? 0 : modelObj.size; } }

    public string displayName;

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

    public int threat = 100;
    public int tauntedBy = -1;

    public ActiveEffects statusEffects = new ActiveEffects();

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

            displayName = data.displayName;

            isFriendly = baseData.isFriendly;
            playerControlled = baseData.playerControlled;

            health = new CombatValue(this, baseData.baseHealth, baseData.healthScaling);
            attack = new CombatValue(this, baseData.baseAttack, baseData.attackScaling);
            defence = new CombatValue(this, baseData.baseDefence, baseData.defenceScaling);
            speed = new CombatSpeed(this, baseData.speeds);
        }
    }
}

public static class CombatantUtility
{
    public static List<KeyValuePair<int, int>> SortedByThreat(this List<CombatantCore> combatants, bool descending = true, int includeThreshold = int.MinValue)
    {
        List<KeyValuePair<int, int>> lOut = new List<KeyValuePair<int, int>>();
        int i, j, threat;
        lOut.Add(new KeyValuePair<int, int>(0, combatants[0] == null ? 0 : combatants[0].threat));
        if (descending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                threat = combatants[i] == null ? 0 : combatants[i].threat;
                if (threat >= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>(i, threat));
                        else if (threat >= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, threat));
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (includeThreshold == int.MinValue)
                includeThreshold = int.MaxValue;
            for (i = 1; i < combatants.Count; i++)
            {
                threat = combatants[1] == null ? 0 : combatants[1].threat;
                if (threat <= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>());
                        else if (threat <= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, threat));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }
    
    public static List<KeyValuePair<int, int>> SortedByCurrentHealth(this List<CombatantCore> combatants, bool descending = true, int includeThreshold = int.MinValue)
    {
        List<KeyValuePair<int, int>> lOut = new List<KeyValuePair<int, int>>();
        int i, j, health;
        lOut.Add(new KeyValuePair<int, int>(0, combatants[0] == null ? 0 : combatants[0].health.Current));
        if (descending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[i] == null ? 0 : combatants[i].health.Current;
                if (health >= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>(i, health));
                        else if (health >= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, health));
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (includeThreshold == int.MinValue)
                includeThreshold = int.MaxValue;
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[1] == null ? 0 : combatants[1].health.Current;
                if (health <= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>());
                        else if (health <= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, health));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }
    
    public static List<KeyValuePair<int, float>> SortedByPercentHealth(this List<CombatantCore> combatants, bool descending = true, float includeThreshold = float.MinValue)
    {
        List<KeyValuePair<int, float>> lOut = new List<KeyValuePair<int, float>>();
        int i, j;
        float health;
        if (includeThreshold == float.MinValue && !descending)
            includeThreshold = float.MaxValue;
        includeThreshold = Mathf.Clamp(includeThreshold, 0.0f, 100.0f);
        lOut.Add(new KeyValuePair<int, float>(0, combatants[0] == null ? 0 : (float)combatants[0].health.Current / combatants[0].health.ScaledAsFloat));
        if (descending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[i] == null ? 0 : (float)combatants[i].health.Current / combatants[i].health.ScaledAsFloat;
                if (health >= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, float>(i, health));
                        else if (health >= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, float>(i, health));
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[1] == null ? 0 : combatants[1].health.Current;
                if (health <= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, float>(i, health));
                        else if (health <= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, float>(i, health));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }

    public static List<KeyValuePair<int, int>> SortedByMaxHealth(this List<CombatantCore> combatants, bool descending = true, int includeThreshold = int.MinValue)
    {
        List<KeyValuePair<int, int>> lOut = new List<KeyValuePair<int, int>>();
        int i, j, health;
        lOut.Add(new KeyValuePair<int, int>(0, combatants[0] == null ? 0 : combatants[0].health.Scaled));
        if (descending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[i] == null ? 0 : combatants[i].health.Scaled;
                if (health >= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>(i, health));
                        else if (health >= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, health));
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (includeThreshold == int.MinValue)
                includeThreshold = int.MaxValue;
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[1] == null ? 0 : combatants[1].health.Scaled;
                if (health <= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>());
                        else if (health <= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, health));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }

    public static List<KeyValuePair<int, int>> SortedByStacks(this List<CombatantCore> combatants, string effectName, bool descending = true, int includeThreshold = int.MinValue)
    {
        List<KeyValuePair<int, int>> lOut = new List<KeyValuePair<int, int>>();
        int i, j, stacks;
        lOut.Add(new KeyValuePair<int, int>(0, combatants[0] == null ? 0 : combatants[0].threat));
        if (descending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                threat = combatants[i] == null ? 0 : combatants[i].threat;
                if (threat >= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>(i, threat));
                        else if (threat >= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, threat));
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (includeThreshold == int.MinValue)
                includeThreshold = int.MaxValue;
            for (i = 1; i < combatants.Count; i++)
            {
                threat = combatants[1] == null ? 0 : combatants[1].threat;
                if (threat <= includeThreshold)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new KeyValuePair<int, int>());
                        else if (threat <= lOut[j].Value)
                        {
                            lOut.Insert(j, new KeyValuePair<int, int>(i, threat));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
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

[System.Serializable]
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
        Current = Mathf.RoundToInt(modifiers.Modify(GetAtLevel(level)));
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
        Current = Mathf.RoundToInt(modifiers.Modify(GetAtLevel(level)));
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
        Current = Mathf.RoundToInt(modifiers.Modify(GetAtLevel(level)));
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
        Current = Mathf.RoundToInt(modifiers.Modify(GetAtLevel(level)));
        return false;
    }
    
    public bool RemoveAt(int index)
    {
        if (speeds.InBounds(index))
        {
            speeds.RemoveAt(index);
            return true;
        }
        Current = Mathf.RoundToInt(modifiers.Modify(GetAtLevel(level)));
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

public class CombatantBrain
{
    // - Bool to enable/disable

    // - List of possible actions
    // - Priority ranking
    // - Associated conditions
    // - Percentage chances
}
