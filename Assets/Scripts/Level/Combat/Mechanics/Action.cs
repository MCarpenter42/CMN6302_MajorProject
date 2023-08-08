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
using Unity.VisualScripting;

public enum CombatActionType { Attack, Heal, Inflict, Cleanse, Taunt, Summon, Mark, Dismiss, MultiAction }
[System.Serializable]
public class CombatAction
{
    protected static CombatManager CombatManager
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

    public struct BlastAttributes
    {
        public int width;
        public int falloffPercent;
        public float falloff { get { return (float)falloffPercent / 100.0f; } }

        public BlastAttributes(ushort width, int falloffPercent)
        {
            this.width = width;
            this.falloffPercent = Mathf.Clamp(falloffPercent, 0, 99);
        }

        public BlastAttributes None { get { return new BlastAttributes() { width = -1, falloffPercent = 100 }; } }
    }

    public struct ExecutionData
    {
        public bool succeeded;

        public static ExecutionData Failed
        {
            get
            {
                return new ExecutionData()
                {
                    succeeded = false
                };
            }
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public CombatActionType type;
    public ActionTarget targeting;
    public CombatantAttribute refAttribute;
    public BlastAttributes blast;

    public CombatAction[] subActions;

    public List<NamedCallback> onSuccess = new List<NamedCallback>();
    public List<NamedCallback> onFailure = new List<NamedCallback>();

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public CombatAction()
    {
        targeting = ActionTarget.Any(TargetCondition.Random);
    }
    
    public CombatAction(ActionTarget targeting)
    {
        this.targeting = targeting;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public ExecutionData Execute(CombatantCore actor)
    {
        switch (type)
        {
            default:
                return ExecutionData.Failed;

            case CombatActionType.Attack:
                return Attack(actor);

            case CombatActionType.Heal:
                return Heal(actor);

            case CombatActionType.Inflict:
                return Inflict(actor);

            case CombatActionType.Cleanse:
                return Cleanse(actor);

            case CombatActionType.Mark:
                return Mark(actor);

            case CombatActionType.Taunt:
                return Taunt(actor);

            case CombatActionType.Summon:
                return Summon(actor);

            case CombatActionType.Dismiss:
                return Dismiss(actor);

            case CombatActionType.MultiAction:
                return MultiAction(actor);
        }
    }

    public ExecutionData Attack(CombatantCore actor)
    {
        return Attack(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Attack(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Heal(CombatantCore actor)
    {
        return Heal(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Heal(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Inflict(CombatantCore actor)
    {
        return Inflict(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Inflict(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Cleanse(CombatantCore actor)
    {
        return Cleanse(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Cleanse(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Mark(CombatantCore actor)
    {
        return Mark(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Mark(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Taunt(CombatantCore actor)
    {
        return Taunt(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Taunt(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Summon(CombatantCore actor)
    {
        return Summon(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Summon(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Dismiss(CombatantCore actor)
    {
        return Dismiss(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Dismiss(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData MultiAction(CombatantCore actor)
    {
        return MultiAction(actor, targeting.GetTargets(actor));
    }
    public ExecutionData MultiAction(CombatantCore actor, int[,] targets)
    {
        if (targets.GetLength(0) > 1)
        {

        }
        else if (targets.GetLength(0) > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // Action effects:
    //  - Attack
    //      - Damage type
    //      - Actor ATK
    //      - Damage modifiers
    //      - Status effect
    //      - Inflict chance
    //  - Heal
    //      - 
    //  - Apply status effect
    //  - Remove status effect
    //  - Secondary effect
    //  - Summon
    //  - 
    //  - 
    //  - 
}

[System.Serializable]
public enum TargetSelection { None, Self, Allied, AlliedAll, Opposed, OpposedAll, Any, AnyAll }
[System.Serializable]
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
    private bool ignoreMarked;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static ActionTarget Self()
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Self,
            condition = TargetCondition.Random,
            tauntable = false,
            ignoreMarked = true
        };
    }

    public static ActionTarget Allied(TargetCondition condition)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Allied,
            condition = condition,
            tauntable = false,
            ignoreMarked = true
        };
    }
    
    public static ActionTarget AllAllied(TargetCondition condition)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Allied,
            condition = condition,
            tauntable = false,
            ignoreMarked = true
        };
    }
    
    public static ActionTarget Opposed(TargetCondition condition, bool tauntable = true, bool ignoreMarked = false)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Opposed,
            condition = condition,
            tauntable = tauntable,
            ignoreMarked = ignoreMarked
        };
    }
    
    public static ActionTarget AllOpposed(TargetCondition condition, bool tauntable = true, bool ignoreMarked = false)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.OpposedAll,
            condition = condition,
            tauntable = tauntable,
            ignoreMarked = ignoreMarked
        };
    }
    
    public static ActionTarget Any(TargetCondition condition, bool tauntable = true, bool ignoreMarked = false)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Any,
            condition = condition,
            tauntable = tauntable,
            ignoreMarked = ignoreMarked
        };
    }
    
    public static ActionTarget All(TargetCondition condition, bool tauntable = true, bool ignoreMarked = false)
    {
        return new ActionTarget()
        {
            selection = TargetSelection.AnyAll,
            condition = condition,
            tauntable = tauntable,
            ignoreMarked = ignoreMarked
        };
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private bool TargetTeam(bool onAllyTeam, bool targetingAllyTeam)
    {
        switch (selection)
        {
            default:
            case TargetSelection.None:
            case TargetSelection.Self:
            case TargetSelection.Allied:
            case TargetSelection.AlliedAll:
                return onAllyTeam == targetingAllyTeam;

            case TargetSelection.Opposed:
            case TargetSelection.OpposedAll:
                return onAllyTeam != targetingAllyTeam;

            case TargetSelection.Any:
            case TargetSelection.AnyAll:
                return true;
        }
    }

    public int[,] GetTargets(CombatantCore actor)
    {
        bool playerAlly = actor.brain.friendly;
        int i, actorInd = actor.index, tauntedBy = actor.brain.tauntedBy, markedTarget = actor.brain.markedTarget;
        if (selection == TargetSelection.Self)
        {
            return new int[,] { { playerAlly ? 0 : 1, actorInd } };
        }
        else if (selection == TargetSelection.AlliedAll)
        {
            int[,] output;
            if (playerAlly)
            {
                output = new int[CombatManager.playerTeam.Count, 2];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i, 0] = 0;
                    output[i, 0] = i;
                }
            }
            else
            {
                output = new int[CombatManager.enemyTeam.Count, 2];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i, 0] = 1;
                    output[i, 0] = i;
                }
            }
            return output;
        }
        else if (selection == TargetSelection.OpposedAll)
        {
            int[,] output;
            if (playerAlly)
            {
                output = new int[CombatManager.enemyTeam.Count, 2];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i, 0] = 1;
                    output[i, 0] = i;
                }
            }
            else
            {
                output = new int[CombatManager.playerTeam.Count, 2];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i, 0] = 0;
                    output[i, 0] = i;
                }
            }
            return output;
        }
        else if (selection == TargetSelection.AnyAll)
        {
            int a = CombatManager.playerTeam.Count, b = a + CombatManager.enemyTeam.Count;
            int[,] output = new int[b, 2];
            for (i = 0; i < a; i++)
            {
                output[i, 0] = 0;
                output[i, 0] = i;
            }
            for (i = a; i < b; i++)
            {
                output[i, 0] = 1;
                output[i, 0] = i;
            }
            return output;
        }
        else
        {
            if (tauntable && tauntedBy >= 0)
            {
                return new int[,] { { playerAlly ? 1 : 0, tauntedBy } };
            }
            else if (!ignoreMarked && markedTarget >= 0)
            {
                return new int[,] { { playerAlly ? 1 : 0, markedTarget } };
            }
            else
            {
                List<CombatantCore> targs = new List<CombatantCore>();
                List<CombatantReturnData> sortedTargs = new List<CombatantReturnData>();
                if (TargetTeam(playerAlly, true))
                    targs.AddRange(CombatManager.playerTeam);
                if (TargetTeam(playerAlly, false))
                    targs.AddRange(CombatManager.playerTeam);
                switch (condition.type)
                {
                    default:
                        break;

                    case TargConType.Threat:
                        sortedTargs = targs.SortedByThreat(condition.invert, condition.threshold);
                        break;

                    case TargConType.Health_Value:
                        sortedTargs = targs.SortedByCurrentHealth(condition.invert, condition.threshold);
                        break;

                    case TargConType.Health_Percent:
                        sortedTargs = targs.SortedByPercentHealth(condition.invert, condition.threshold);
                        break;

                    case TargConType.Health_Max:
                        sortedTargs = targs.SortedByMaxHealth(condition.invert, condition.threshold);
                        break;

                    case TargConType.StatusStacks:
                        sortedTargs = targs.SortedByStatusStacks(condition.internalName, condition.invert, condition.threshold);
                        break;

                    case TargConType.StatusLifetime:
                        sortedTargs = targs.SortedByStatusLifetime(condition.internalName, condition.invert, condition.threshold);
                        break;
                }
                if (condition.strength == TargConStrength.Full)
                {
                    if (condition.type == TargConType.None)
                    {
                        int r = Random.Range(0, sortedTargs.Count);
                        return new int[,] { { sortedTargs[r].isFriendly ? 0 : 1, sortedTargs[r].index } };
                    }
                    else
                    {
                        return new int[,] { { sortedTargs[0].isFriendly ? 0 : 1, sortedTargs[0].index } };
                    }
                }
                else if (condition.strength == TargConStrength.Strong)
                {
                    float total, threshold = 0.0f, r;
                    if (condition.type == TargConType.Health_Percent)
                    {
                        total = sortedTargs.TotalFloatValue();
                        r = Random.Range(0.0f, 1.0f);
                        for (i = 1; i < sortedTargs.Count; i++)
                        {
                            threshold += sortedTargs[i].floatValue / total;
                            if (r <= threshold)
                                return new int[,] { { sortedTargs[i].isFriendly ? 0 : 1, sortedTargs[i].index } };
                        }
                    }
                    else
                    {
                        total = sortedTargs.TotalIntValue();
                        r = Random.Range(0.0f, 1.0f);
                        for (i = 1; i < sortedTargs.Count; i++)
                        {
                            threshold += (float)sortedTargs[i].intValue / total;
                            if (r <= threshold)
                                return new int[,] { { sortedTargs[i].isFriendly ? 0 : 1, sortedTargs[i].index } };
                        }
                    }
                }
                else
                {
                    float total, f, threshold = 0.0f, r;
                    if (condition.type == TargConType.Health_Percent)
                    {
                        total = sortedTargs.TotalFloatValue();
                        f = total / (sortedTargs.Count * 2.0f);
                        r = Random.Range(0.0f, 1.0f);
                        for (i = 1; i < sortedTargs.Count; i++)
                        {
                            threshold += f + (sortedTargs[i].floatValue / (total * 2.0f));
                            if (r <= threshold)
                                return new int[,] { { sortedTargs[i].isFriendly ? 0 : 1, sortedTargs[i].index } };
                        }
                    }
                    else
                    {
                        total = sortedTargs.TotalIntValue();
                        f = total / (sortedTargs.Count * 2.0f);
                        r = Random.Range(0.0f, 1.0f);
                        for (i = 1; i < sortedTargs.Count; i++)
                        {
                            threshold += f + (sortedTargs[i].floatValue / (total * 2.0f));
                            if (r <= threshold)
                                return new int[,] { { sortedTargs[i].isFriendly ? 0 : 1, sortedTargs[i].index } };
                        }
                    }
                }
                
            }

            return new int[0, 0];
        }
    }
}

[System.Serializable]
public enum TargConType { None, Threat, Health_Value, Health_Percent, Health_Max, StatusStacks, StatusLifetime }
[System.Serializable]
public enum TargConStrength { Full, Strong, Weak }
[System.Serializable]
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
        this.threshold = int.MinValue;
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
