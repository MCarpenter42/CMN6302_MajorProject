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

    public struct MultiTargetAttributes
    {
        public int count;
        public MultiTargetType type;
        public int falloffPercent;
        public float falloff { get { return (float)falloffPercent / 100.0f; } }

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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public CombatAction()
    {
        targeting = ActionTarget.Any(TargetCondition.None);
    }
    
    public CombatAction(ActionTarget targeting)
    {
        this.targeting = targeting;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private bool RollForCrit()
    {
        return GameManager.Instance.RandTuning.value_damage_critRate < Random.Range(0.0f, 1.0f);
    }

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

    public ExecutionData Attack(CombatantCore actor)
    {
        return Attack(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Attack(CombatantCore actor, KeyValuePair<bool, int>[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float dmgOut = actor.DamageOut(baseAttribute, actionMultiplier * (selfCondMet ? selfMultiplier : 1.0f), damageType);
        bool playerTeam = actor.brain.friendly;
        KeyValuePair<bool, int> actorTeamIndex = actor.teamIndex;
        if (targets.Length > 1)
        {
            foreach (KeyValuePair<bool, int> target in targets)
            {
                if (target.Key)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[target.Value]);
                    CombatManager.playerTeam[target.Value].DamageTaken(actorTeamIndex, targCondMet ? dmgOut * targMultiplier : dmgOut, damageType);
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.Value]);
                    CombatManager.enemyTeam[target.Value].DamageTaken(actorTeamIndex, targCondMet ? dmgOut * targMultiplier : dmgOut, damageType, playerTeam && !target.Key && RollForCrit());
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (targets[0].Key)
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[tInd]);
                CombatManager.playerTeam[tInd].DamageTaken(actorTeamIndex, targCondMet ? dmgOut * targMultiplier : dmgOut, damageType);
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
                            blastR = rInd < CombatManager.playerTeam.Count;
                            if (blastL)
                            {
                                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[lInd]);
                                CombatManager.playerTeam[lInd].DamageTaken(actorTeamIndex, (targCondMet ? dmgOut * targMultiplier : dmgOut) * power, damageType);
                            }
                            if (blastR)
                            {
                                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[rInd]);
                                CombatManager.playerTeam[rInd].DamageTaken(actorTeamIndex, (targCondMet ? dmgOut * targMultiplier : dmgOut) * power, damageType);
                            }
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(true, Random.Range(0, CombatManager.playerTeam.Count));
                        }
                        AttackBounceSequence(actor, bounceTargets, dmgOut, falloffFactor);
                    }
                }
            }
            else
            {
                int tInd = targets[0].Value;
                CombatManager.enemyTeam[tInd].DamageTaken(actorTeamIndex, dmgOut, damageType, playerTeam && RollForCrit());
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
                                CombatManager.enemyTeam[lInd].DamageTaken(actorTeamIndex, dmgOut * power, damageType, playerTeam && RollForCrit());
                            if (blastR)
                                CombatManager.enemyTeam[rInd].DamageTaken(actorTeamIndex, dmgOut * power, damageType, playerTeam && RollForCrit());
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.enemyTeam.Count));
                        }
                        AttackBounceSequence(actor, bounceTargets, dmgOut, falloffFactor);
                    }
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }
    private void AttackBounceSequence(CombatantCore actor, KeyValuePair<bool, int>[] targets, float baseValue, float falloffFactor)
    {
        actor.StartCoroutine(IAttackBounceSequence(actor.teamIndex, targets, baseValue, falloffFactor, 0.4f));
    }
    private IEnumerator IAttackBounceSequence(KeyValuePair<bool, int> actorTeamIndex, KeyValuePair<bool, int>[] targets, float baseValue, float falloffFactor, float bounceTime)
    {
        float power = falloffFactor;
        bool targCondMet;
        for (int i = 0; i < targets.Length; i++, power *= falloffFactor)
        {
            if (targets[i].Key)
            {
                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[targets[i].Value]);
                CombatManager.playerTeam[targets[i].Value].DamageTaken(actorTeamIndex, baseValue * power, damageType);
            }
            else
            {
                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[targets[i].Value]);
                CombatManager.enemyTeam[targets[i].Value].DamageTaken(actorTeamIndex, baseValue * power, damageType, actorTeamIndex.Key && RollForCrit());
            }
            yield return new WaitForSeconds(bounceTime);
        }
    }

    public ExecutionData Heal(CombatantCore actor)
    {
        return Heal(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Heal(CombatantCore actor, KeyValuePair<bool, int>[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float healOut;
        bool playerTeam = actor.brain.friendly;
        if (targets.Length > 1)
        {
            foreach (KeyValuePair<bool, int> target in targets)
            {
                if (target.Key)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (targets[0].Key)
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[targets[0].Value]);
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
                            blastR = rInd < CombatManager.playerTeam.Count;
                            if (blastL)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */;
                            if (blastR)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */;
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.playerTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            else
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].Value]);
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
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */;
                            if (blastR)
                                /* SINGLE-TARGET FUNCTION ACCOUNTING FOR FALLOFF */;
                            if (!blastL && !blastR)
                                break;
                        }
                    }
                    else if (multiTarget.type == MultiTargetType.Bounce)
                    {
                        float falloffFactor = 1.0f - multiTarget.falloff;
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.enemyTeam.Count));
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
    private void HealBounceSequence(CombatantCore actor, KeyValuePair<bool, int>[] targets, float baseValue, float falloffFactor)
    {
        actor.StartCoroutine(IHealBounceSequence(actor.teamIndex, targets, baseValue, falloffFactor, 0.4f));
    }
    private IEnumerator IHealBounceSequence(KeyValuePair<bool, int> actorTeamIndex, KeyValuePair<bool, int>[] targets, float baseValue, float falloffFactor, float bounceTime)
    {
        float power = falloffFactor;
        for (int i = 0; i < targets.Length; i++, power *= falloffFactor)
        {
            if (targets[i].Key)
                CombatManager.playerTeam[targets[i].Value].Healed(actorTeamIndex, baseValue * power);
            else
                CombatManager.enemyTeam[targets[i].Value].Healed(actorTeamIndex, baseValue * power);
            yield return new WaitForSeconds(bounceTime);
        }
    }

    public ExecutionData Shield(CombatantCore actor)
    {
        return Shield(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Shield(CombatantCore actor, KeyValuePair<bool, int>[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float shieldOut;
        bool playerTeam = actor.brain.friendly;
        if (targets.Length > 1)
        {
            foreach (KeyValuePair<bool, int> target in targets)
            {
                if (target.Key)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (targets[0].Key)
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[targets[0].Value]);
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
                            blastR = rInd < CombatManager.playerTeam.Count;
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
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.playerTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            else
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].Value]);
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
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.enemyTeam.Count));
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

    public ExecutionData ApplyStatus(CombatantCore actor)
    {
        return ApplyStatus(actor, targeting.GetTargets(actor));
    }
    public ExecutionData ApplyStatus(CombatantCore actor, KeyValuePair<bool, int>[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        float effectScaleOut;
        bool playerTeam = actor.brain.friendly;
        if (targets.Length > 1)
        {
            foreach (KeyValuePair<bool, int> target in targets)
            {
                if (target.Key)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (targets[0].Key)
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[targets[0].Value]);
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
                            blastR = rInd < CombatManager.playerTeam.Count;
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
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.playerTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            else
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].Value]);
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
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.enemyTeam.Count));
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

    public ExecutionData RemoveStatus(CombatantCore actor)
    {
        return RemoveStatus(actor, targeting.GetTargets(actor));
    }
    public ExecutionData RemoveStatus(CombatantCore actor, KeyValuePair<bool, int>[] targets)
    {
        bool selfCondMet = selfMultCondition.MetBy(actor), targCondMet;
        bool playerTeam = actor.brain.friendly;
        if (targets.Length > 1)
        {
            foreach (KeyValuePair<bool, int> target in targets)
            {
                if (target.Key)
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
                else
                {
                    targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[target.Value]);
                    /* SINGLE-TARGET FUNCTION */
                }
            }
            return new ExecutionData() { succeeded = true };
        }
        else if (targets.Length > 0)
        {
            if (targets[0].Key)
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.playerTeam[targets[0].Value]);
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
                            blastR = rInd < CombatManager.playerTeam.Count;
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
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.playerTeam.Count));
                        }
                        /* START BOUNCE SEQUENCE COROUTINE */
                    }
                }
            }
            else
            {
                int tInd = targets[0].Value;
                targCondMet = targMultCondition.MetBy(CombatManager.enemyTeam[targets[0].Value]);
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
                        KeyValuePair<bool, int>[] bounceTargets = new KeyValuePair<bool, int>[multiTarget.count];
                        for (int i = 0; i < bounceTargets.Length; i++)
                        {
                            bounceTargets[i] = new KeyValuePair<bool, int>(false, Random.Range(0, CombatManager.enemyTeam.Count));
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

    public ExecutionData Mark(CombatantCore actor)
    {
        return Mark(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Mark(CombatantCore actor, KeyValuePair<bool, int>[] targets)
    {
        if (targets.Length > 0 && targets[0].Key != actor.brain.friendly)
        {
            /* APPLY "MARKED" STATUS EFFECT HERE */
            if (markForAll)
            {
                if (actor.brain.friendly)
                {
                    foreach (CombatantCore combatant in CombatManager.playerTeam)
                    {
                        combatant.brain.markedTarget = targets[0].Value;
                    }
                }
                else
                {
                    foreach (CombatantCore combatant in CombatManager.enemyTeam)
                    {
                        combatant.brain.markedTarget = targets[0].Value;
                    }
                }
            }
            else
            {
                actor.brain.markedTarget = targets[0].Value;
            }
            return new ExecutionData() { succeeded = true };
        }
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

    public ExecutionData Taunt(CombatantCore actor)
    {
        return Taunt(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Taunt(CombatantCore actor, KeyValuePair<bool, int>[] targets)
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

    /*public ExecutionData Summon(CombatantCore actor)
    {
        return Summon(actor);
    }*/
    public ExecutionData Summon(CombatantCore actor)
    {
        
        onFailure.Invoke();
        return ExecutionData.Failed;
    }

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

    public ExecutionData MultiAction(CombatantCore actor)
    {
        return MultiAction(actor, targeting.GetTargets(actor));
    }
    public ExecutionData MultiAction(CombatantCore actor, KeyValuePair<bool, int>[] targets)
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
    
    public ExecutionData Charge(CombatantCore actor)
    {
        return MultiAction(actor, targeting.GetTargets(actor));
    }
    public ExecutionData Charge(CombatantCore actor, KeyValuePair<bool, int>[] targets)
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

    // Action effects:
    //  - Attack
    //      - Damage type
    //      - Actor ATK
    //      - Damage modifiers
    //      - Status effect
    //      - ApplyStatus chance
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

    public KeyValuePair<bool, int>[] GetTargets(CombatantCore actor)
    {
        bool playerAlly = actor.brain.friendly;
        int i, tauntedBy = actor.brain.tauntedBy, markedTarget = actor.brain.markedTarget;
        int maxTargStr = GameManager.Instance.RandTuning.value_behaviour_targeting;
        TargConStrength strength = (int)condition.strength > maxTargStr ? (TargConStrength)maxTargStr : condition.strength;

        if (selection == TargetSelection.Self)
        {
            return new KeyValuePair<bool, int>[] { actor.teamIndex };
        }
        else if (selection == TargetSelection.AlliedAll)
        {
            KeyValuePair<bool, int>[] output;
            if (playerAlly)
            {
                output = new KeyValuePair<bool, int>[CombatManager.playerTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.playerTeam[i].teamIndex;
                }
            }
            else
            {
                output = new KeyValuePair<bool, int>[CombatManager.enemyTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.enemyTeam[i].teamIndex;
                }
            }
            return output;
        }
        else if (selection == TargetSelection.OpposedAll)
        {
            KeyValuePair<bool, int>[] output;
            if (playerAlly)
            {
                output = new KeyValuePair<bool, int>[CombatManager.enemyTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.enemyTeam[i].teamIndex;
                }
            }
            else
            {
                output = new KeyValuePair<bool, int>[CombatManager.playerTeam.Count];
                for (i = 0; i < output.GetLength(0); i++)
                {
                    output[i] = CombatManager.playerTeam[i].teamIndex;
                }
            }
            return output;
        }
        else if (selection == TargetSelection.AnyAll)
        {
            int a = CombatManager.playerTeam.Count, b = a + CombatManager.enemyTeam.Count;
            KeyValuePair<bool, int>[] output = new KeyValuePair<bool, int>[b];
            for (i = 0; i < a; i++)
            {
                output[i] = new KeyValuePair<bool, int>(true, i);
            }
            for (i = a; i < b; i++)
            {
                output[i] = new KeyValuePair<bool, int>(false, i - a);
            }
            return output;
        }
        else
        {
            if (tauntable && tauntedBy >= 0)
            {
                return new KeyValuePair<bool, int>[] { new KeyValuePair<bool, int>(!playerAlly, tauntedBy) };
            }
            else if (!ignoreMarked && markedTarget >= 0)
            {
                return new KeyValuePair<bool, int>[] { new KeyValuePair<bool, int>(!playerAlly, markedTarget) };
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
                        sortedTargs = targs.SortedByStatusStacks(condition.statusEffect, condition.invert, condition.threshold);
                        break;

                    case TargConType.StatusLifetime:
                        sortedTargs = targs.SortedByStatusLifetime(condition.statusEffect, condition.invert, condition.threshold);
                        break;
                }
                if (strength == TargConStrength.None)
                {
                    int r = Random.Range(0, sortedTargs.Count);
                    return new KeyValuePair<bool, int>[] { sortedTargs[r].teamIndex };
                }
                else if (strength == TargConStrength.Full)
                {
                    if (condition.type == TargConType.None)
                    {
                        int r = Random.Range(0, sortedTargs.Count);
                        return new KeyValuePair<bool, int>[] { sortedTargs[r].teamIndex };
                    }
                    else
                    {
                        return new KeyValuePair<bool, int>[] { sortedTargs[0].teamIndex };
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
                                return new KeyValuePair<bool, int>[] { sortedTargs[i].teamIndex };
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
                                return new KeyValuePair<bool, int>[] { sortedTargs[i].teamIndex };
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
                                return new KeyValuePair<bool, int>[] { sortedTargs[i].teamIndex };
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
                                return new KeyValuePair<bool, int>[] { sortedTargs[i].teamIndex };
                        }
                    }
                }
            }

            return new KeyValuePair<bool, int>[0];
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

    public TargetCondition(TargConType type = TargConType.None)
    {
        this.type = type;
        strength = TargConStrength.None;
        statusEffect = new StatusEffectReturnData();
        threshold = int.MinValue;
        invert = false;
    }

    public TargetCondition(TargConType type, TargConStrength strength = TargConStrength.Full, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData();
        this.threshold = int.MinValue;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, int threshold, TargConStrength strength = TargConStrength.Full, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData();
        this.threshold = threshold;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, string effectInternalName, int threshold, TargConStrength strength = TargConStrength.Full, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData(effectInternalName, false);
        this.threshold = threshold;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, string effectInternalName, bool effectSpecial, int threshold, TargConStrength strength = TargConStrength.Full, bool invert = false)
    {
        this.type = type;
        this.strength = strength;
        statusEffect = new StatusEffectReturnData(effectInternalName, effectSpecial);
        this.threshold = threshold;
        this.invert = invert;
    }
    
    public TargetCondition(TargConType type, StatusEffectReturnData statusEffect, int threshold, TargConStrength strength = TargConStrength.Full, bool invert = false)
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
