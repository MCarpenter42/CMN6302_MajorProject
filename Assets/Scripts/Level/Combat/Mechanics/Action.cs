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
    private static CombatManager CombatManager
    {
        get
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.Level != null)
                {
                    return GameManager.Instance.Level.Combat;
                }
            }
            return null;
        }
    }

    // Target options:
    //  - Self
    //  - 1 ally (prioritise others)
    //  - All allies
    //  - 1 enemy
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
    //  - Secondary effect
    //  - Summon
    //  - 
    //  - 
    //  - 
    //  - 
    //  - 
}

public enum TargetSelection { None, Self, Allied, AlliedAll, Opposed, OpposedAll, Any, AnyAll }
public class ActionTarget
{
    private static CombatManager CombatManager
    {
        get
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.Level != null)
                {
                    return GameManager.Instance.Level.Combat;
                }
            }
            return null;
        }
    }

    private TargetSelection selection;
    private TargetCondition condition;
    private bool tauntable;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static ActionTarget Self()
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Self,
            condition = TargetCondition.Random,
            tauntable = false
        };
    }

    public static ActionTarget Allied(TargetCondition condition)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Allied,
            condition = condition,
            tauntable = false
        };
    }
    
    public static ActionTarget AllAllied(TargetCondition condition)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Allied,
            condition = condition,
            tauntable = false
        };
    }
    
    public static ActionTarget Opposed(TargetCondition condition, bool tauntable = true)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Opposed,
            condition = condition,
            tauntable = tauntable
        };
    }
    
    public static ActionTarget AllOpposed(TargetCondition condition, bool tauntable = true)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.OpposedAll,
            condition = condition,
            tauntable = tauntable
        };
    }
    
    public static ActionTarget Any(TargetCondition condition, bool tauntable = true)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Any,
            condition = condition,
            tauntable = tauntable
        };
    }
    
    public static ActionTarget All(TargetCondition condition, bool tauntable = true)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.AnyAll,
            condition = condition,
            tauntable = tauntable
        };
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public int[] GetTargets(CombatantCore actor)
    {
        bool playerAlly = actor.isFriendly;
        int actorInd = actor.index, i;
        int selInd = -1, selInt = condition.invert ? int.MaxValue : int.MinValue, checkInd, checkInt;
        float selFlloat = condition.invert ? float.MaxValue : float.MinValue, checkFloat;
        int[] inds = new int[0], indsSorted, targInds = new int[0];
        if (actor != null && CombatManager != null)
        {
            switch (selection)
            {
                default:
                case TargetSelection.None:
                    break;

                case TargetSelection.Self:
                    targInds = new int[] { actorInd };
                    break;

                case TargetSelection.Allied:
                    if (playerAlly)
                        inds = CombatManager.allies;
                    else
                        inds = CombatManager.enemies;
                    break;

                case TargetSelection.AlliedAll:
                    if (playerAlly)
                        targInds = CombatManager.allies;
                    else
                        targInds = CombatManager.enemies;
                    break;

                case TargetSelection.Opposed:
                    if (playerAlly)
                        inds = CombatManager.enemies;
                    else
                        inds = CombatManager.allies;
                    break;

                case TargetSelection.OpposedAll:
                    if (playerAlly)
                        targInds = CombatManager.enemies;
                    else
                        targInds = CombatManager.allies;
                    break;

                case TargetSelection.Any:
                    inds = new int[CombatManager.combatants.Count].IncrementalPopulate();
                    break;

                case TargetSelection.AnyAll:
                    targInds = new int[CombatManager.combatants.Count].IncrementalPopulate();
                    break;
            }
        }
        if (inds.Length > 0)
        {
            if (tauntable && actor.tauntedBy > -1)
            {
                targInds = new int[] { actor.tauntedBy };
            }
            else if (inds.Length == 1)
            {
                targInds = new int[] { inds[0] };
            }
            else
            {
                if (condition.strength == TargConStrength.Full)
                {
                    switch (condition.type)
                    {
                        default:
                        case TargConType.None:
                            // RANDOM
                            break;


                    }
                }
                else
                {

                }
            }
        }
        return targInds;
    }
}

public enum TargConType { None, Threat, Health_Value, Health_Percent, Health_Max, StatusStacks }
public enum TargConStrength { Full, Strong, Weak }
public struct TargetCondition
{
    public TargConType type;
    public TargConStrength strength;
    public string internalName;
    public int threshold;
    public bool invert;

    public TargetCondition(TargConType type, TargConStrength strength = TargConStrength.Full, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        this.internalName = null;
        this.threshold = -1;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, int threshold, TargConStrength strength = TargConStrength.Full, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        this.internalName = null;
        this.threshold = threshold;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, string internalName, int threshold, TargConStrength strength = TargConStrength.Full, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        this.internalName = internalName;
        this.threshold = threshold;
        this.invert = invert;
    }
    
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static TargetCondition Random { get { return new TargetCondition(TargConType.None, TargConStrength.Full); } }
    public static TargetCondition HighestThreat { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestPercentHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestMaxHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
}
