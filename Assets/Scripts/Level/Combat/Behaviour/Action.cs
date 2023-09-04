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
using static UnityEngine.UI.Image;

[System.Serializable]
public enum CombatActionType { Attack, Heal, Shield, ApplyStatus, RemoveStatus, Mark, Taunt, Summon, Dismiss, MultiAction, Charge }
[System.Serializable]
public enum MultiTargetType { Blast, Bounce }
[System.Serializable]
public class CombatAction
{
    protected static CombatManager CombatManager
    {
        get
        {
            if (Core.LevelManager != null)
            {
                return Core.LevelManager.Combat;
            }
            return null;
        }
    }

    public struct ExecutionData
    {
        public bool succeeded;
        public float duration;

        public bool empty => duration < -1;

        public ExecutionData(bool succeeded, float duration = -10f)
        {
            this.succeeded = succeeded;
            this.duration = duration;
        }

        public static ExecutionData Empty { get { return new ExecutionData(false, -10f); } }
        public static ExecutionData Failed { get { return new ExecutionData(false, 0f); } }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public string displayName = "";
    public string iconPath = null;

    public CombatActionType type;
    public int damageType;
    public ActionTarget targeting;
    public CombatantAttribute baseAttribute;
    public MultiTargetAttributes multiTarget = MultiTargetAttributes.None;

    public bool markForAll = false;

    public float actionMultiplier = 1.0f;
    public float selfMultiplier = 1.0f;
    public TargetCondition selfMultCondition = TargetCondition.None;
    public float targMultiplier = 1.0f;
    public TargetCondition targMultCondition = TargetCondition.None;

    public CombatAction[] subActions;

    public List<NamedCallback> onSuccess = new List<NamedCallback>();
    public List<NamedCallback> onFailure = new List<NamedCallback>();

    public float animDurWindup = 3.0f;
    public float animDurCompletion = 2.0f;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public CombatAction()
    {
        targeting = ActionTarget.Any(TargetCondition.None);
    }
    
    public CombatAction(ActionTarget targeting)
    {
        this.targeting = targeting;
    }

    public CombatAction Copy(string newName = null, string newIcon = null)
    {
        return new CombatAction()
        {
            displayName = newName == null ? displayName : newName,
            iconPath = newIcon == null ? iconPath : newIcon,

            type = type,
            damageType = damageType,
            targeting = targeting,
            baseAttribute = baseAttribute,
            multiTarget = multiTarget,

            markForAll = markForAll,

            actionMultiplier = actionMultiplier,
            selfMultiplier = selfMultiplier,
            selfMultCondition = selfMultCondition,
            targMultiplier = targMultiplier,
            targMultCondition = targMultCondition,

            subActions = subActions,

            onSuccess = onSuccess,
            onFailure = onFailure,
        };
    }
    
    public CombatAction Copy(ActionCopyRef copyRef)
    {
        return new CombatAction()
        {
            displayName = copyRef.newName == null ? displayName : copyRef.newName,
            iconPath = copyRef.newIcon == null ? iconPath : copyRef.newIcon,

            type = type,
            damageType = damageType,
            targeting = targeting,
            baseAttribute = baseAttribute,
            multiTarget = multiTarget,

            markForAll = markForAll,

            actionMultiplier = actionMultiplier,
            selfMultiplier = selfMultiplier,
            selfMultCondition = selfMultCondition,
            targMultiplier = targMultiplier,
            targMultCondition = targMultCondition,

            subActions = subActions,

            onSuccess = onSuccess,
            onFailure = onFailure,
        };
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private bool RollForCrit()
    {
        return Core.RandTuning.valDmg_critRate < Random.Range(0.0f, 1.0f);
    }

    public ExecutionData Execute(CombatantCore actor)
    {
        Debug.Log("Actor \"" + actor.displayName + "\" executed action " + displayName + " (" + type.ToString() + ")");
        switch (type)
        {
            default:
                return ExecutionData.Failed;

            case CombatActionType.Attack:
                return Attack(actor);

            case CombatActionType.Heal:
                return Heal(actor);
                
            case CombatActionType.Shield:
                return Shield(actor);

            case CombatActionType.ApplyStatus:
                return ApplyStatus(actor);

            case CombatActionType.RemoveStatus:
                return RemoveStatus(actor);

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
    
    public ExecutionData Execute(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        Debug.Log("Actor \"" + actor.displayName + "\" executed action " + displayName);
        switch (type)
        {
            default:
                return ExecutionData.Failed;

            case CombatActionType.Attack:
                return Attack(actor, targets);

            case CombatActionType.Heal:
                return Heal(actor, targets);
                
            case CombatActionType.Shield:
                return Shield(actor, targets);

            case CombatActionType.ApplyStatus:
                return ApplyStatus(actor, targets);

            case CombatActionType.RemoveStatus:
                return RemoveStatus(actor, targets);

            case CombatActionType.Mark:
                return Mark(actor, targets);

            case CombatActionType.Taunt:
                return Taunt(actor, targets);

            case CombatActionType.Summon:
                return Summon(actor);

            case CombatActionType.Dismiss:
                return Dismiss(actor);

            case CombatActionType.MultiAction:
                return MultiAction(actor, targets);
        }
    }

    // COMPLETE
    public ExecutionData Attack(CombatantCore actor)
    {
        return Attack(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Attack(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float dmgOut = actor.DamageOut(baseAttribute, actionMultiplier * (selfCondMet ? selfMultiplier : 1.0f), damageType);
        bool allyTeam = actor.brain.friendly;
        CombatantTeamIndex actorTeamIndex = actor.teamIndex;
        if (targets.Length > 1)
        {
            CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0].playerTeam, animDurWindup, animDurCompletion);
            foreach (CombatantTeamIndex target in targets)
            {
                if (target.playerTeam)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[target.teamIndex]);
                    CombatManager.allyTeam[target.teamIndex].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, targCondMet ? dmgOut * targMultiplier : dmgOut, damageType);
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.teamIndex]);
                    CombatManager.enemyTeam[target.teamIndex].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, targCondMet ? dmgOut * targMultiplier : dmgOut, damageType, allyTeam && !target.playerTeam && RollForCrit());
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (multiTarget.type == MultiTargetType.Blast)
                CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0].playerTeam, animDurWindup, animDurCompletion);
            else
                CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0], animDurWindup, animDurCompletion);
            if (targets[0].playerTeam)
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[tInd]);
                CombatManager.allyTeam[tInd].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, targCondMet ? dmgOut * targMultiplier : dmgOut, damageType);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.allyTeam.Count;
                            if (blastL)
                            {
                                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[lInd]);
                                CombatManager.allyTeam[lInd].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, (targCondMet ? dmgOut * targMultiplier : dmgOut) * power, damageType);
                            }
                            if (blastR)
                            {
                                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[rInd]);
                                CombatManager.allyTeam[rInd].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, (targCondMet ? dmgOut * targMultiplier : dmgOut) * power, damageType);
                            }
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(true, Random.Range(0, CombatManager.allyTeam.Count));
                        }
                        AttackBounceSequence(actor, bounceTargets, dmgOut, falloffFactor);
                    }
                }
            }
            else
            {
                int tInd = targets[0].teamIndex;
                CombatManager.enemyTeam[tInd].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, dmgOut, damageType, allyTeam && RollForCrit());
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.enemyTeam.Count;
                            if (blastL)
                                CombatManager.enemyTeam[lInd].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, dmgOut * power, damageType, allyTeam && RollForCrit());
                            if (blastR)
                                CombatManager.enemyTeam[rInd].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, dmgOut * power, damageType, allyTeam && RollForCrit());
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(false, Random.Range(0, CombatManager.enemyTeam.Count));
                        }
                        AttackBounceSequence(actor, bounceTargets, dmgOut, falloffFactor);
                    }
                }
            }
            return new ExecutionData() { succeeded = true, duration = (animDurWindup + animDurCompletion) };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }
    private void AttackBounceSequence(CombatantCore actor, CombatantTeamIndex[] targets, float baseValue, float falloffFactor)
    {
        actor.StartCoroutine(IAttackBounceSequence(actor.teamIndex, targets, baseValue, falloffFactor, 0.4f));
    }
    private IEnumerator IAttackBounceSequence(CombatantTeamIndex actorTeamIndex, CombatantTeamIndex[] targets, float baseValue, float falloffFactor, float bounceTime)
    {
        float power = falloffFactor;
        bool targCondMet;
        for (int i = 0; i < targets.Length; i++, power *= falloffFactor)
        {
            if (targets[i].playerTeam)
            {
                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[targets[i].teamIndex]);
                CombatManager.allyTeam[targets[i].teamIndex].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, baseValue * power, damageType);
            }
            else
            {
                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[targets[i].teamIndex]);
                CombatManager.enemyTeam[targets[i].teamIndex].DamageTaken(animDurWindup, animDurCompletion, actorTeamIndex, baseValue * power, damageType, actorTeamIndex.playerTeam && RollForCrit());
            }
            yield return new WaitForSeconds(bounceTime);
        }
    }

    // COMPLETE
    public ExecutionData Heal(CombatantCore actor)
    {
        return Heal(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Heal(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float healOut = actor.HealingOut(baseAttribute, actionMultiplier * (selfCondMet ? selfMultiplier : 1.0f));
        bool allyTeam = actor.brain.friendly;
        CombatantTeamIndex actorTeamIndex = actor.teamIndex;
        if (targets.Length > 1)
        {
            CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0].playerTeam, animDurWindup, animDurCompletion);
            foreach (CombatantTeamIndex target in targets)
            {
                if (target.playerTeam)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[target.teamIndex]);
                    CombatManager.allyTeam[target.teamIndex].Healed(animDurWindup, animDurCompletion, actorTeamIndex, healOut);
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.teamIndex]);
                    CombatManager.enemyTeam[target.teamIndex].Healed(animDurWindup, animDurCompletion, actorTeamIndex, healOut);
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (multiTarget.type == MultiTargetType.Blast)
                CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0].playerTeam, animDurWindup, animDurCompletion);
            else
                CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0], animDurWindup, animDurCompletion);
            if (targets[0].playerTeam)
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.allyTeam.Count;
                            if (blastL)
                                CombatManager.allyTeam[lInd].Healed(animDurWindup, animDurCompletion, actorTeamIndex, healOut * power);
                            if (blastR)
                                CombatManager.allyTeam[rInd].Healed(animDurWindup, animDurCompletion, actorTeamIndex, healOut * power);
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(false, Random.Range(0, CombatManager.allyTeam.Count));
                        }
                        HealBounceSequence(actor, bounceTargets, healOut, falloffFactor);
                    }
                }
            }
            else
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.enemyTeam.Count;
                            if (blastL)
                                CombatManager.enemyTeam[lInd].Healed(actorTeamIndex, healOut * power);
                            if (blastR)
                                CombatManager.enemyTeam[rInd].Healed(actorTeamIndex, healOut * power);
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(false, Random.Range(0, CombatManager.enemyTeam.Count));
                        }
                        HealBounceSequence(actor, bounceTargets, healOut, falloffFactor);
                    }
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }
    private void HealBounceSequence(CombatantCore actor, CombatantTeamIndex[] targets, float baseValue, float falloffFactor)
    {
        actor.StartCoroutine(IHealBounceSequence(actor.teamIndex, targets, baseValue, falloffFactor, 0.4f));
    }
    private IEnumerator IHealBounceSequence(CombatantTeamIndex actorTeamIndex, CombatantTeamIndex[] targets, float baseValue, float falloffFactor, float bounceTime)
    {
        float power = falloffFactor;
        for (int i = 0; i < targets.Length; i++, power *= falloffFactor)
        {
            if (targets[i].playerTeam)
                CombatManager.allyTeam[targets[i].teamIndex].Healed(animDurWindup, animDurCompletion, actorTeamIndex, baseValue * power);
            else
                CombatManager.enemyTeam[targets[i].teamIndex].Healed(animDurWindup, animDurCompletion, actorTeamIndex, baseValue * power);
            yield return new WaitForSeconds(bounceTime);
        }
    }

    // COMPLETE
    public ExecutionData Shield(CombatantCore actor)
    {
        return Shield(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Shield(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float shieldOut = actor.HealingOut(baseAttribute, actionMultiplier * (selfCondMet ? selfMultiplier : 1.0f));
        bool allyTeam = actor.brain.friendly;
        CombatantTeamIndex actorTeamIndex = actor.teamIndex;
        if (targets.Length > 1)
        {
            CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0].playerTeam, animDurWindup, animDurCompletion);
            foreach (CombatantTeamIndex target in targets)
            {
                if (target.playerTeam)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[target.teamIndex]);
                    CombatManager.allyTeam[target.teamIndex].Shielded(animDurWindup, animDurCompletion, actorTeamIndex, shieldOut);
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.teamIndex]);
                    CombatManager.enemyTeam[target.teamIndex].Shielded(animDurWindup, animDurCompletion, actorTeamIndex, shieldOut);
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (multiTarget.type == MultiTargetType.Blast)
                CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0].playerTeam, animDurWindup, animDurCompletion);
            else
                CombatManager.ActionAnim(displayName, actorTeamIndex, targets[0], animDurWindup, animDurCompletion);
            if (targets[0].playerTeam)
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.allyTeam.Count;
                            if (blastL)
                                CombatManager.allyTeam[lInd].Shielded(animDurWindup, animDurCompletion, actorTeamIndex, shieldOut * power);
                            if (blastR)
                                CombatManager.allyTeam[rInd].Shielded(animDurWindup, animDurCompletion, actorTeamIndex, shieldOut * power);
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                }
            }
            else
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.enemyTeam.Count;
                            if (blastL)
                                CombatManager.enemyTeam[lInd].Shielded(animDurWindup, animDurCompletion, actorTeamIndex, shieldOut * power);
                            if (blastR)
                                CombatManager.enemyTeam[rInd].Shielded(animDurWindup, animDurCompletion, actorTeamIndex, shieldOut * power);
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData ApplyStatus(CombatantCore actor)
    {
        return ApplyStatus(actor, targeting.GetTargets(actor));
    }
    public ExecutionData ApplyStatus(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float effectScaleOut;
        bool allyTeam = actor.brain.friendly;
        if (targets.Length > 1)
        {
            foreach (CombatantTeamIndex target in targets)
            {
                if (target.playerTeam)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[target.teamIndex]);
                    /* SINGLE-TARGET FUNCTION */
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.teamIndex]);
                    /* SINGLE-TARGET FUNCTION */
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (targets[0].playerTeam)
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.allyTeam.Count;
                            if (blastL)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (blastR)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(false, Random.Range(0, CombatManager.allyTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            else
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.enemyTeam.Count;
                            if (blastL)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (blastR)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(false, Random.Range(0, CombatManager.enemyTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData RemoveStatus(CombatantCore actor)
    {
        return RemoveStatus(actor, targeting.GetTargets(actor));
    }
    public ExecutionData RemoveStatus(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        bool allyTeam = actor.brain.friendly;
        if (targets.Length > 1)
        {
            foreach (CombatantTeamIndex target in targets)
            {
                if (target.playerTeam)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[target.teamIndex]);
                    /* SINGLE-TARGET FUNCTION */
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.teamIndex]);
                    /* SINGLE-TARGET FUNCTION */
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (targets[0].playerTeam)
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.allyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.allyTeam.Count;
                            if (blastL)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (blastR)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(false, Random.Range(0, CombatManager.allyTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            else
            {
                int tInd = targets[0].teamIndex;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].teamIndex]);
                if (multiTarget.enabled)
                {
                    if (multiTarget.type == MultiTargetType.Blast)
                    {
                        int lInd = tInd - 1, rInd = tInd + 1;
                        bool blastL, blastR;
                        float falloffFactor = 1.0f - multiTarget.falloff, power = 1.0f - multiTarget.falloff;
                        for (int i = 0; i < multiTarget.count; i++, power *= falloffFactor, lInd--, rInd++)
                        {
                            blastL = lInd >= 0;
                            blastR = rInd < CombatManager.enemyTeam.Count;
                            if (blastL)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (blastR)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */
                                ;
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        CombatantTeamIndex[] bounceTargets = new CombatantTeamIndex[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new CombatantTeamIndex(false, Random.Range(0, CombatManager.enemyTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData Mark(CombatantCore actor)
    {
        return Mark(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Mark(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        if (targets.Length > 0 && targets[0].playerTeam != actor.brain.friendly)
        {
            /* APPLY "MARKED" STATUS EFFECT HERE */
            if (markForAll)
            {
                if (actor.brain.friendly)
                {
                    foreach (CombatantCore combatant in CombatManager.allyTeam)
                    {
                        combatant.brain.markedTarget = targets[0].teamIndex;
                    }
                }
                else
                {
                    foreach (CombatantCore combatant in CombatManager.enemyTeam)
                    {
                        combatant.brain.markedTarget = targets[0].teamIndex;
                    }
                }
            }
            else
            {
                actor.brain.markedTarget = targets[0].teamIndex;
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData Taunt(CombatantCore actor)
    {
        return Taunt(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Taunt(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        if (targets.Length > 1)
        {

        }
        else if (targets.Length > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData Summon(CombatantCore actor)
    {
        
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData Dismiss(CombatantCore actor)
    {
        if (actor.brain.summons.Count > 0)
        {
            foreach (CombatantCore summon in actor.brain.summons)
            {
                /* DISMISS FUNCTION */
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData MultiAction(CombatantCore actor)
    {
        return MultiAction(actor, targeting.GetTargets(actor));
    }
    public ExecutionData MultiAction(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        if (targets.Length > 1)
        {

        }
        else if (targets.Length > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    // INCOMPLETE
    public ExecutionData Charge(CombatantCore actor)
    {
        return MultiAction(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Charge(CombatantCore actor, CombatantTeamIndex[] targets)
    {
        if (targets.Length > 1)
        {

        }
        else if (targets.Length > 0)
        {

        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }
}

public struct MultiTargetAttributes
{
    public int count;
    public MultiTargetType type;
    public int falloffPercent;
    public float falloff { get { return falloffPercent / 100f; } }

    public MultiTargetAttributes(ushort count, int falloffPercent = 50)
    {
        this.count = count;
        this.type = MultiTargetType.Blast;
        this.falloffPercent = Mathf.Clamp(falloffPercent, 0, 100);
    }

    public MultiTargetAttributes(ushort count, MultiTargetType type, int falloffPercent = 50)
    {
        this.count = count;
        this.type = type;
        this.falloffPercent = Mathf.Clamp(falloffPercent, 0, 100);
    }

    public static MultiTargetAttributes None { get { return new MultiTargetAttributes() { count = -1, falloffPercent = 100 }; } }
    public bool enabled { get { return count > 0; } }

    public static MultiTargetAttributes Basic { get { return new MultiTargetAttributes() { count = 1, falloffPercent = 40 }; } }
}

public struct ActionCopyRef
{
    public int index;
    public string newName;
    public string newIcon;

    public ActionCopyRef(int index, string newName, string newIcon = null)
    {
        this.index = index;
        this.newName = newName;
        this.newIcon = newIcon;
    }
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

    public TargetSelection selection;
    public TargetCondition condition;
    public bool tauntable;
    public bool ignoreMarked;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static ActionTarget Self()
    {
        return new ActionTarget()
        {
            selection = TargetSelection.Self,
            condition = TargetCondition.None,
            tauntable = false,
            ignoreMarked = true
        };
    }

    public static ActionTarget Allied() { return Allied(TargetCondition.None); }
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

    public static ActionTarget AllAllied() { return AllAllied(TargetCondition.None); }
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

    public static ActionTarget Opposed(bool tauntable = true, bool ignoreMarked = false) { return Opposed(TargetCondition.None, tauntable, ignoreMarked); }
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

    public static ActionTarget AllOpposed(bool tauntable = true, bool ignoreMarked = false) { return AllOpposed(TargetCondition.None, tauntable, ignoreMarked); }
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

    public static ActionTarget Any(bool tauntable = true, bool ignoreMarked = false) { return Any(TargetCondition.None, tauntable, ignoreMarked); }
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

    public static ActionTarget All(bool tauntable = true, bool ignoreMarked = false) { return All(TargetCondition.None, tauntable, ignoreMarked); }
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

    public CombatantTeamIndex[] GetTargets(CombatantCore actor)
    {
        bool actorIsAlly = actor.brain.friendly;
        int i, tauntedBy = actor.brain.tauntedBy, markedTarget = actor.brain.markedTarget;
        int maxTargStr = Core.RandTuning.valBhv_targeting;
        TargConStrength strength = (int)condition.strength > maxTargStr ? (TargConStrength)maxTargStr : condition.strength;

        if (selection == TargetSelection.Self)
        {
            return new CombatantTeamIndex[] { actor.teamIndex };
        }
        else if (selection == TargetSelection.AlliedAll)
        {
            CombatantTeamIndex[] output;
            if (actorIsAlly)
            {
                output = new CombatantTeamIndex[CombatManager.allyTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.allyTeam[i].teamIndex;
                }
            }
            else
            {
                output = new CombatantTeamIndex[CombatManager.enemyTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.enemyTeam[i].teamIndex;
                }
            }
            return output;
        }
        else if (selection == TargetSelection.OpposedAll)
        {
            CombatantTeamIndex[] output;
            if (actorIsAlly)
            {
                output = new CombatantTeamIndex[CombatManager.enemyTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.enemyTeam[i].teamIndex;
                }
            }
            else
            {
                output = new CombatantTeamIndex[CombatManager.allyTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.allyTeam[i].teamIndex;
                }
            }
            return output;
        }
        else if (selection == TargetSelection.AnyAll)
        {
            int a = CombatManager.allyTeam.Count, b = a + CombatManager.enemyTeam.Count;
            CombatantTeamIndex[] output = new CombatantTeamIndex[b];
            for (i = 0; i < a; i++)
            {
                output[i] = new CombatantTeamIndex(true, i);
            }
            for (i = a; i < b; i++)
            {
                output[i] = new CombatantTeamIndex(false, i - a);
            }
            return output;
        }
        else
        {
            if (tauntable && tauntedBy >= 0)
            {
                return new CombatantTeamIndex[] { new CombatantTeamIndex(!actorIsAlly, tauntedBy) };
            }
            else if (!ignoreMarked && markedTarget >= 0)
            {
                return new CombatantTeamIndex[] { new CombatantTeamIndex(!actorIsAlly, markedTarget) };
            }
            else
            {
                List<CombatantCore> targs = new List<CombatantCore>();
                List<CombatantReturnData> sortedTargs = new List<CombatantReturnData>();
                if (selection == TargetSelection.Allied)
                {
                    if (actorIsAlly)
                        targs.AddRange(CombatManager.allyTeam);
                    else
                        targs.AddRange(CombatManager.enemyTeam);

                }
                else if (selection == TargetSelection.Opposed)
                {
                    if (actorIsAlly)
                        targs.AddRange(CombatManager.enemyTeam);
                    else
                        targs.AddRange(CombatManager.allyTeam);
                }
                else
                {
                    targs.AddRange(CombatManager.allyTeam);
                    targs.AddRange(CombatManager.enemyTeam);
                }
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
                        sortedTargs = targs.SortedByStatusStacks(condition.statusEffect, condition.invert, condition.threshold);
                        break;

                    case TargConType.StatusLifetime:
                        sortedTargs = targs.SortedByStatusLifetime(condition.statusEffect, condition.invert, condition.threshold);
                        break;
                }
                if (strength == TargConStrength.None)
                {
                    int r = Random.Range(0, sortedTargs.Count);
                    return new CombatantTeamIndex[] { sortedTargs[r].teamIndex };
                }
                else if (strength == TargConStrength.Full)
                {
                    if (condition.type == TargConType.None)
                    {
                        int r = Random.Range(0, sortedTargs.Count);
                        return new CombatantTeamIndex[] { sortedTargs[r].teamIndex };
                    }
                    else
                    {
                        return new CombatantTeamIndex[] { sortedTargs[0].teamIndex };
                    }
                }
                else if (strength == TargConStrength.Strong)
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
                                return new CombatantTeamIndex[] { sortedTargs[i].teamIndex };
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
                                return new CombatantTeamIndex[] { sortedTargs[i].teamIndex };
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
                                return new CombatantTeamIndex[] { sortedTargs[i].teamIndex };
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
                                return new CombatantTeamIndex[] { sortedTargs[i].teamIndex };
                        }
                    }
                }
            }

            return new CombatantTeamIndex[0];
        }
    }
}

[System.Serializable]
public enum TargConType { None, Threat, Health_Value, Health_Percent, Health_Max, StatusStacks, StatusLifetime, Random }
[System.Serializable]
public enum TargConStrength { None, Weak, Strong, Full }
[System.Serializable]
public struct TargetCondition
{
    public TargConType type;
    public TargConStrength strength;
    public StatusEffectReturnData statusEffect;
    public int threshold;
    public bool invert;

    public TargetCondition(TargConType type, bool invert = false)
    {
        this.type = type;
        strength = TargConStrength.None;
        statusEffect = new StatusEffectReturnData();
        threshold = int.MinValue;
        this.invert = invert;
    }

    public TargetCondition(TargConType type, TargConStrength strength, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData();
        this.threshold = int.MinValue;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, int threshold, TargConStrength strength, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData();
        this.threshold = threshold;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, string effectInternalName, int threshold, TargConStrength strength, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData(effectInternalName, false);
        this.threshold = threshold;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, string effectInternalName, bool effectSpecial, int threshold, TargConStrength strength, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData(effectInternalName, effectSpecial);
        this.threshold = threshold;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, StatusEffectReturnData statusEffect, int threshold, TargConStrength strength, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        this.statusEffect = statusEffect;
        this.threshold = threshold;
        this.invert = invert;
    }
    
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool Valid { get { return type != TargConType.None; } }

    public bool MetBy(CombatantCore combatant)
    {
        int thr = invert && threshold == int.MinValue ? int.MaxValue : threshold;
        switch (type)
        {
            default:
                return false;

            case TargConType.Threat:
                if (!invert)
                    return combatant.threat >= thr;
                else
                    return combatant.threat < thr;

            case TargConType.Health_Max:
                if (!invert)
                    return combatant.health.Scaled >= thr;
                else
                    return combatant.health.Scaled < thr;

            case TargConType.Health_Percent:
                if (!invert)
                    return ((float)combatant.health.Current / combatant.health.ScaledAsFloat) >= thr;
                else
                    return ((float)combatant.health.Current / combatant.health.ScaledAsFloat) < thr;

            case TargConType.Health_Value:
                if (!invert)
                    return combatant.health.Current >= thr;
                else
                    return combatant.health.Current < thr;

            case TargConType.StatusStacks:
                if (statusEffect.Null)
                    return false;
                else
                {
                    if (!invert)
                        return combatant.statusEffects.Stacks(statusEffect.internalName, statusEffect.special, true) >= thr;
                    else
                        return combatant.statusEffects.Stacks(statusEffect.internalName, statusEffect.special, false) < thr;
                }

            case TargConType.StatusLifetime:
                if (statusEffect.Null)
                    return false;
                else
                {
                    if (!invert)
                        return combatant.statusEffects.Lifetime(statusEffect.internalName, statusEffect.special, true) >= thr;
                    else
                        return combatant.statusEffects.Lifetime(statusEffect.internalName, statusEffect.special, false) < thr;
                }
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static TargetCondition None { get { return new TargetCondition(TargConType.None, TargConStrength.None); } }
    public static TargetCondition HighestThreat { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestPercentHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
    public static TargetCondition HighestMaxHealth { get { return new TargetCondition(TargConType.Threat, TargConStrength.Full); } }
}
