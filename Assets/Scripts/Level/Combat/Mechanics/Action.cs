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

public class CombatAction
{
    // Target options:
    //  - Self
    //  - 1 or more allies (self excluded)
    //  - All allies
    //  - 1 or more enemies
    //  - All enemies

    // Target conditions:
    //  - Threat
    //  - Health value
    //  - Health %
    //  - Status effect stacks
    //  - Weighted random (requires 2nd condition)
    //  - Pure random

    // Action effects:
    //  - Attack
    //  - Heal
    //  - Apply status effect
    //  - 
    //  - 
    //  - 
    //  - 
    //  - 
    //  - 
    //  - 
}

public enum TargetSelection { None, Self, Allied_Count, Allied_All, Opposed_Count, Opposed_All, Any_Count, Any_All }
public class ActionTarget
{
    private TargetSelection selection;
    private TargetCondition condition;
    private int maxTargets;
    private bool tauntable;

    public static ActionTarget Self()
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Self,
            maxTargets = 1
        };
    }

    public static ActionTarget Allied(int maxTargets)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Allied_Count,
            maxTargets = maxTargets > 0 ? maxTargets : 1
        };
    }
}

public enum TargConType { None, Threat, Health_Value, Health_Percent, Health_Max, Attack, Defence, Speed, StatusStacks }
public enum TargConStrength { Full, Strong, Weak }
public struct TargetCondition
{
    public TargConType type;
    public TargConStrength strength;
    public int ID;
    public int threshold;
    public bool isOver;

    public TargetCondition(TargConType type, TargConStrength strength = TargConStrength.Full, int ID = -1, int threshold = -1, bool isOver = true)
    {
        this.type = type;
        this.strength = strength;
        this.ID = ID;
        this.threshold = threshold;
        this.isOver = isOver;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static TargetCondition Random { get { return new TargetCondition(TargConType.None, TargConStrength.Full); } }
    public static TargetCondition HighestThreat { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestPercentHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestMaxHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestAttack { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestDefence { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestSpeed { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
}
