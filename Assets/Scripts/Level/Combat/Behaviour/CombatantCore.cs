using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.Interpolation;
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
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;

// BASIC THREAT AND TAUNT SYSTEMS NEEDED
// MEMORY (ATTACKED BY + ETC) NEEDED

public enum CombatantAttribute { Health, Attack, Defence, Speed, InflictChance, InflictResist }
public class CombatantCore : Core
{
    public CombatManager CombatManager { get { return LevelManager.Combat; } }

    #region [ OBJECTS / COMPONENTS ]

    [System.NonSerialized]
    public CombatantData baseData;
    public EntityModel modelObj;

    public Transform pivot;

    public HealthBar healthBar;
    public TargetingArrow targetingArrow;
    public Transform cameraViewAnchor;

    public Transform damageTextAnchor;

    #endregion

    #region [ PROPERTIES ]

    public Vector3 position { get { return gameObject.transform.position; } set { gameObject.transform.position = value; } }
    public Vector3 rotation { get { return pivot.eulerAngles; } set { pivot.eulerAngles = value; } }
    public int Size { get { return modelObj == null ? 0 : modelObj.size; } }

    public bool gotData;
    public int index = -1;

    public string displayName;
    public int level;

    public CombatValue health = null;
    public CombatValue attack = null;
    public CombatValue defence = null;
    public CombatSpeed speed = null;

    public int activeShieldCount;
    public int totalShieldValue;

    public float inflictChance = 40.0f;
    public float inflictResist = 0.0f;

    public DamageType.Type attackType;
    public bool[] weakAgainst;

    public List<DamageDealtModifier> damageOutMods = new List<DamageDealtModifier>();
    public List<DamageTakenModifier> damageInMods = new List<DamageTakenModifier>();

    public List<HealingModifier> healingOutMods = new List<HealingModifier>();
    public List<HealingModifier> healingInMods = new List<HealingModifier>();
    
    public List<ShieldModifier> shieldOutMods = new List<ShieldModifier>();
    public List<ShieldModifier> shieldInMods = new List<ShieldModifier>();

    public List<StatusModifier> statusInMods = new List<StatusModifier>();

    public CombatantBrain brain;
    public CombatantTeamIndex teamIndex { get { return new CombatantTeamIndex(brain.friendly, index); } }

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

    public ActiveEffects statusEffects;

    public static float animTimeOffset = 0.3f;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected override void Initialise()
    {
        statusEffects = new ActiveEffects(this);
        if (targetingArrow != null)
            targetingArrow.ClearState();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private int BaseStatVariance(int value, CombatantAttribute attribute)
    {
        float v;
        switch (attribute)
        {
            default:
                v = 0.0f;
                break;

            case CombatantAttribute.Health:
                v = RandTuning.valSts_health;
                break;

            case CombatantAttribute.Defence:
                v = RandTuning.valSts_defence;
                break;
        }

        return Mathf.RoundToInt(value * (1.0f + Random.Range(-v, v)));
    }

    public virtual void GetData(CombatantData data, int level = 1)
    {
        gotData = data != null;
        if (gotData)
        {
            baseData = data;
            this.level = level;

            string modelPath = EntityModel.GetModelPathFromUID(data.modelHexUID);
            if (modelPath != null)
            {
                GameObject modelTemplate = Resources.Load<GameObject>(modelPath);
                modelObj = modelTemplate == null ? null : Instantiate(modelTemplate, pivot ?? transform).GetComponent<EntityModel>();
            }

            displayName = data.displayName;

            health = new CombatValue(this, BaseStatVariance(baseData.baseHealth, CombatantAttribute.Health), baseData.healthScaling);
            health.Current = health.Scaled;
            attack = new CombatValue(this, baseData.baseAttack, baseData.attackScaling);
            defence = new CombatValue(this, BaseStatVariance(baseData.baseDefence, CombatantAttribute.Defence), baseData.defenceScaling);
            speed = new CombatSpeed(this, baseData.speeds, RandTuning.valSts_speed ? 10.0f : -1.0f);

            /*if (RandTuning.valSts_types)
            {
                attackType = (DamageType.Type)Random.Range(1, DamageType.TypeCount);
                weakAgainst = new bool[data.weakAgainst.Length];
                List<int> inds = new List<int>();
                int r;
                for (int i = 0; i < weakAgainst.Length; i++)
                {
                    if (i != (int)attackType)
                        inds.Add(i);
                }
                for (int i = 0; i < data.weakAgainst.CountIf(true); i++)
                {
                    r = Random.Range(0, inds.Count);
                    weakAgainst[inds[r]] = true;
                    inds.RemoveAt(r);
                }
            }
            else
            {*/
                attackType = data.attackType;
                weakAgainst = data.weakAgainst;
            //}

            brain = new CombatantBrain(this, !data.playerControlled, data.friendly);
            brain.actions = ActionSet.GetSet(data.actionSet);
            brain.actions.GetActions(attackType);
        }
        else
            Debug.Log("Empty data object!");
    }

    public void ModifyHealth(int value)
    {
        if (alive)
        {
            if (value < 0)
            {
                if (-value > health.Current)
                    value = -health.Current;
            }
            else
            {
                if (value > health.Scaled)
                    value = health.Scaled - health.Current;
            }

            health.Current += value;

            if (health.Current == 0)
            {
                OnDied();
            }
        }

        if (healthBar != null)
        {
            if (healthBar.GetType() == typeof(HealthBar3D))
            {
                float healthPercent = (float)health.Current / health.Scaled;
                if (value > 0)
                    healthBar.SetValueWithFlash(healthPercent, false, 0.6f);
                else if (value < 0)
                    healthBar.SetValueWithFlash(healthPercent, true, 0.6f);
            }
            else
            {
                if (value > 0)
                    (healthBar as HealthBarCanvas).SetValueWithFlash(health.Current, health.Scaled, false, 0.6f);
                else if (value < 0)
                    (healthBar as HealthBarCanvas).SetValueWithFlash(health.Current, health.Scaled, true, 0.6f);
            }
            health.Current = Mathf.Clamp(health.Current, 0, health.Scaled);
        }
    }

    public void OnDied()
    {
        if (alive)
        {
            alive = false;
            CombatManager.OnCombatantDied(this);
        }
    }

    public void OnRevived(int healBy = 1)
    {
        if (!alive)
        {
            alive = true;
            ModifyHealth(healBy);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public float NextAction()
    {
        return brain.ExecuteNextAction();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public float DamageOut(float actionMultiplier, int typeID)
    {
        return DamageOut(CombatantAttribute.Attack, actionMultiplier, typeID);
    }
    public float DamageOut(CombatantAttribute baseAttribute, float actionMultiplier, int typeID)
    {
        float dmgMult = 1.0f;
        foreach (DamageDealtModifier modifier in damageOutMods)
        {
            if (modifier.typeID == typeID)
                dmgMult += modifier.mod;
        }
        float dmgOut;
        switch (baseAttribute)
        {
            default:
            case CombatantAttribute.Attack:
                dmgOut = attack.ScaledAsFloat * actionMultiplier * dmgMult;
                break;

            case CombatantAttribute.Health:
                dmgOut = health.ScaledAsFloat * actionMultiplier * dmgMult;
                break;

            case CombatantAttribute.Defence:
                dmgOut = defence.ScaledAsFloat * actionMultiplier * dmgMult;
                break;

            case CombatantAttribute.Speed:
                dmgOut = (float)speed.Current * actionMultiplier * dmgMult;
                break;
        }
        //Debug.Log("Damage out for " + displayName + ": " + dmgOut);
        return dmgOut;
    }
    
    public float HealingOut(float actionMultiplier)
    {
        return HealingOut(CombatantAttribute.Health, actionMultiplier);
    }
    public float HealingOut(CombatantAttribute baseAttribute, float actionMultiplier)
    {
        float healMult = 1.0f;
        foreach (HealingModifier modifier in healingOutMods)
        {
            healMult += modifier.mod;
        }
        switch (baseAttribute)
        {
            default:
            case CombatantAttribute.Attack:
                return attack.ScaledAsFloat * actionMultiplier * healMult;

            case CombatantAttribute.Health:
                return health.ScaledAsFloat * actionMultiplier * healMult;

            case CombatantAttribute.Defence:
                return defence.ScaledAsFloat * actionMultiplier * healMult;

            case CombatantAttribute.Speed:
                return (float)speed.Current * actionMultiplier * healMult;
        }
    }

    public float ShieldOut(float actionMultiplier)
    {
        return ShieldOut(CombatantAttribute.Health, actionMultiplier);
    }
    public float ShieldOut(CombatantAttribute baseAttribute, float actionMultiplier)
    {
        float healMult = 1.0f;
        foreach (HealingModifier modifier in healingOutMods)
        {
            healMult += modifier.mod;
        }
        switch (baseAttribute)
        {
            default:
            case CombatantAttribute.Attack:
                return attack.ScaledAsFloat * actionMultiplier * healMult;

            case CombatantAttribute.Health:
                return health.ScaledAsFloat * actionMultiplier * healMult;

            case CombatantAttribute.Defence:
                return defence.ScaledAsFloat * actionMultiplier * healMult;

            case CombatantAttribute.Speed:
                return (float)speed.Current * actionMultiplier * healMult;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void DamageTaken(float delayA, float delayB, CombatantTeamIndex origin, float baseDamage, int typeID, bool crit = false)
    {
        StartCoroutine(IDamageTaken(delayA, delayB, origin, baseDamage, typeID, crit));
    }
    private IEnumerator IDamageTaken(float delayA, float delayB, CombatantTeamIndex origin, float baseDamage, int typeID, bool crit = false)
    {
        yield return new WaitForSeconds(delayA + animTimeOffset);
        DamageTaken(origin, baseDamage, typeID, crit);
        yield return new WaitForSeconds(delayB - animTimeOffset);
    }
    public int DamageTaken(CombatantTeamIndex origin, float baseDamage, int typeID, bool crit = false)
    {
        float dmgVar = RandTuning.valDmg_base;
        if (dmgVar > 0.0f)
            baseDamage *= 1.0f + Random.Range(-dmgVar, dmgVar);
        if (crit)
            baseDamage *= RandTuning.valDmg_critScale;

        if (activeShieldCount > 0)
            baseDamage = SubtractShielding(baseDamage);
            
        if (baseDamage > 0f)
        {
            float defValue = defence.ScaledAsFloat, defMult = 1 - (defValue / (100 + defValue)), rcvMult = 1.0f;

            // DAMAGE TYPE CALCULATION DISABLED
            /*if (weakAgainst.InBounds(typeID) && weakAgainst[typeID])
                rcvMult *= 2.0f;
            else if (typeID == (int)attackType)
                rcvMult *= 0.5f;*/

            foreach (DamageTakenModifier modifier in damageInMods)
            {
                if (modifier.typeID == typeID)
                    rcvMult *= modifier.mod;
            }

            float f_preDef = baseDamage * rcvMult;
            float f_postDef = f_preDef * defMult;
            int preDef = Mathf.RoundToInt(f_preDef), postDef = Mathf.RoundToInt(f_postDef), dmgOut;

            if (origin.playerTeam != brain.friendly)
                brain.lastAttackedBy = origin.teamIndex;

            if (preDef - postDef < 1)
                dmgOut = preDef - 1;
            else
                dmgOut = postDef;
            CombatManager.OnCombatantDamaged(this, dmgOut);
            ModifyHealth(-dmgOut);
            return dmgOut;
        }
        else
        {
            CombatManager.OnCombatantDamaged(this, 0);
            return 0;
        }
    }

    public float SubtractShielding(float damage)
    {
        int blocked, totalBlocked = 0;
        foreach (StatusEffect effect in statusEffects.Special)
        {
            if (effect.shielding > 0)
            {
                blocked = effect.DamageShield(damage);
                damage -= blocked;
                totalBlocked += blocked;
            }
            if (damage <= 0)
                return 0f;
        }
        foreach (StatusEffect effect in statusEffects.Normal)
        {
            if (effect.shielding > 0)
            {
                blocked = effect.DamageShield(damage);
                damage -= blocked;
                totalBlocked += blocked;
            }
            if (damage <= 0)
                return 0f;
        }

        return damage;
    }

    public void Healed(float delayA, float delayB, CombatantTeamIndex origin, float baseHealing)
    {
        StartCoroutine(IHealed(delayA, delayB, origin, baseHealing));
    }
    private IEnumerator IHealed(float delayA, float delayB, CombatantTeamIndex origin, float baseHealing)
    {
        yield return new WaitForSeconds(delayA + animTimeOffset);
        Healed(origin, baseHealing);
        yield return new WaitForSeconds(delayB - animTimeOffset);
    }
    public int Healed(CombatantTeamIndex origin, float baseHealing)
    {
        float healedFloat = baseHealing;
        foreach (HealingModifier modifier in healingInMods)
        {
            healedFloat *= modifier.mod;
        }
        int healedBy = Mathf.RoundToInt(healedFloat);
        CombatManager.OnCombatantHealed(this, healedBy);
        ModifyHealth(healedBy);
        return healedBy;
    }

    public void Shielded(float delayA, float delayB, CombatantTeamIndex origin, float baseShielding, int duration = 2)
    {
        StartCoroutine(IShielded(delayA, delayB, origin, baseShielding, duration));
    }
    private IEnumerator IShielded(float delayA, float delayB, CombatantTeamIndex origin, float baseShielding, int duration = 2)
    {
        yield return new WaitForSeconds(delayA + animTimeOffset);
        Shielded(origin, baseShielding, duration);
        yield return new WaitForSeconds(delayB - animTimeOffset);
    }
    public int Shielded(CombatantTeamIndex origin, float baseShielding, int duration = 2)
    {
        float shieldFloat = baseShielding;
        foreach (ShieldModifier modifier in shieldInMods)
        {
            shieldFloat *= modifier.mod;
        }
        int shieldInt = Mathf.RoundToInt(shieldFloat);
        statusEffects.Add(StatusEffect.Shield(shieldInt, duration));
        return shieldInt;
    }

    /*public float StatusApplyChance(float actionModifier)
    {

    }*/

    public void StatusApplication(float delayA, float delayB, StatusEffect effect, float baseChance, bool guaranteed = false)
    {
        StartCoroutine(IStatusApplication(delayA, delayB, effect, baseChance, guaranteed));
    }
    private IEnumerator IStatusApplication(float delayA, float delayB, StatusEffect effect, float baseChance, bool guaranteed = false)
    {
        yield return new WaitForSeconds(delayA);
        StatusApplication(effect, baseChance, guaranteed);
        yield return new WaitForSeconds(delayB);
    }
    public void StatusApplication(StatusEffect effect, float baseChance, bool guaranteed = false)
    {

    }

    public bool Interrupt(StatusEffect effect, float baseChance, bool guaranteed = false)
    {

        return false;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SetTargetedState(TargetArrowState state) => targetingArrow.SetState(state);
}

public struct CombatantTeamIndex
{
    public bool playerTeam;
    public int teamIndex;

    public readonly bool isNull => teamIndex == int.MinValue;

    public CombatantTeamIndex(bool playerTeam, int teamIndex)
    {
        this.playerTeam = playerTeam;
        this.teamIndex = teamIndex;
    }

    public static CombatantTeamIndex Null => new CombatantTeamIndex(false, int.MinValue);
}

public struct CombatantReturnData
{
    public bool isNull;
    public bool isFriendly;
    public int index;
    public int intValue;
    public float floatValue;
    public CombatantTeamIndex teamIndex { get { return new CombatantTeamIndex(isFriendly, index); } }

    public CombatantReturnData(CombatantTeamIndex teamAndIndex, int intValue = int.MinValue)
    {
        isNull = intValue == int.MinValue;
        this.isFriendly = teamAndIndex.playerTeam;
        this.index = teamAndIndex.teamIndex;
        this.intValue = intValue;
        this.floatValue = float.MinValue;
    }
    
    public CombatantReturnData(bool isFriendly, int index, int intValue = int.MinValue)
    {
        isNull = intValue == int.MinValue;
        this.isFriendly = isFriendly;
        this.index = index;
        this.intValue = intValue;
        this.floatValue = float.MinValue;
    }

    public CombatantReturnData(CombatantTeamIndex teamAndIndex, float floatValue = float.MinValue)
    {
        isNull = floatValue == float.MinValue;
        this.isFriendly = teamAndIndex.playerTeam;
        this.index = teamAndIndex.teamIndex;
        this.intValue = int.MinValue;
        this.floatValue = floatValue;
    }

    public CombatantReturnData(bool isFriendly, int index, float floatValue = float.MinValue)
    {
        isNull = floatValue == float.MinValue;
        this.isFriendly = isFriendly;
        this.index = index;
        this.intValue = int.MinValue;
        this.floatValue = floatValue;
    }
}

public static class CombatantUtility
{
    public static int TotalIntValue(this List<CombatantReturnData> list)
    {
        int result = 0;
        foreach (CombatantReturnData data in list)
        {
            result += data.intValue;
        }
        return result;
    }
    
    public static float TotalFloatValue(this List<CombatantReturnData> list)
    {
        float result = 0;
        foreach (CombatantReturnData data in list)
        {
            result += data.floatValue;
        }
        return result;
    }

    public static List<CombatantReturnData> SortedByThreat(this List<CombatantCore> combatants, bool ascending = false, int includeThreshold = int.MinValue)
    {
        List<CombatantReturnData> lOut = new List<CombatantReturnData>();
        int i, j, n, threat;
        lOut.Add(new CombatantReturnData(combatants[0].brain.friendly, combatants[0].index, combatants[0] == null ? 0 : combatants[0].threat));
        if (!ascending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                threat = combatants[i] == null ? 0 : combatants[i].threat;
                if (threat >= includeThreshold)
                {
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, threat));
                        else if (threat >= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, threat));
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
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, combatants[i].threat));
                        else if (threat <= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, combatants[i].threat));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }
    
    public static List<CombatantReturnData> SortedByCurrentHealth(this List<CombatantCore> combatants, bool ascending = false, int includeThreshold = int.MinValue)
    {
        List<CombatantReturnData> lOut = new List<CombatantReturnData>();
        int i, j, n, health;
        lOut.Add(new CombatantReturnData(combatants[0].brain.friendly, combatants[0].index, combatants[0] == null ? 0 : combatants[0].health.Current));
        if (!ascending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[i] == null ? 0 : combatants[i].health.Current;
                if (health >= includeThreshold)
                {
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                        else if (health >= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
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
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                        else if (health <= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }
    
    public static List<CombatantReturnData> SortedByPercentHealth(this List<CombatantCore> combatants, bool ascending = false, float includeThreshold = float.MinValue)
    {
        List<CombatantReturnData> lOut = new List<CombatantReturnData>();
        int i, j, n;
        float health;
        if (includeThreshold == float.MinValue && !ascending)
            includeThreshold = float.MaxValue;
        includeThreshold = Mathf.Clamp(includeThreshold, 0.0f, 100.0f);
        lOut.Add(new CombatantReturnData(combatants[0].brain.friendly, combatants[0].index, combatants[0] == null ? 0 : (float)combatants[0].health.Current / combatants[0].health.ScaledAsFloat));
        if (!ascending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[i] == null ? 0 : (float)combatants[i].health.Current / combatants[i].health.ScaledAsFloat;
                if (health >= includeThreshold)
                {
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                        else if (health >= lOut[j].floatValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
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
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                        else if (health <= lOut[j].floatValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }

    public static List<CombatantReturnData> SortedByMaxHealth(this List<CombatantCore> combatants, bool ascending = false, int includeThreshold = int.MinValue)
    {
        List<CombatantReturnData> lOut = new List<CombatantReturnData>();
        int i, j, n, health;
        lOut.Add(new CombatantReturnData(combatants[0].brain.friendly, combatants[0].index, combatants[0] == null ? 0 : combatants[0].health.Scaled));
        if (!ascending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                health = combatants[i] == null ? 0 : combatants[i].health.Scaled;
                if (health >= includeThreshold)
                {
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                        else if (health >= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
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
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                        else if (health <= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, health));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }

    public static List<CombatantReturnData> SortedByStatusStacks(this List<CombatantCore> combatants, string effectName, bool ascending = false, int includeThreshold = int.MinValue)
    {
        return combatants.SortedByStatusStacks(effectName, false, ascending, includeThreshold);
    }

    public static List<CombatantReturnData> SortedByStatusStacks(this List<CombatantCore> combatants, string effectName, bool special, bool ascending = false, int includeThreshold = int.MinValue)
    {
        List<CombatantReturnData> lOut = new List<CombatantReturnData>();
        int[] stacks = new int[combatants.Count];
        int i, j, n;
        for (i = 0; i < stacks.Length; i++)
        {
            stacks[i] = combatants[i] == null ? 0 : combatants[i].statusEffects.Stacks(effectName, special, !ascending);
        }
        lOut.Add(new CombatantReturnData(combatants[0].brain.friendly, combatants[0].index, combatants[0] == null ? 0 : stacks[0]));
        if (!ascending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                if (stacks[i] >= includeThreshold && stacks[i] >= 0)
                {
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, stacks[i]));
                        else if (stacks[i] >= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, stacks[i]));
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
                if (stacks[i] <= includeThreshold && stacks[i] >= 0)
                {
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, stacks[i]));
                        else if (stacks[i] <= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, stacks[i]));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }
    
    public static List<CombatantReturnData> SortedByStatusStacks(this List<CombatantCore> combatants, StatusEffectReturnData effect, bool ascending = false, int includeThreshold = int.MinValue)
    {
        return combatants.SortedByStatusStacks(effect.internalName, effect.special, ascending, includeThreshold);
    }

    public static List<CombatantReturnData> SortedByStatusLifetime(this List<CombatantCore> combatants, string effectName, bool ascending = false, int includeThreshold = int.MinValue)
    {
        return combatants.SortedByStatusLifetime(effectName, false, ascending, includeThreshold);
    }

    public static List<CombatantReturnData> SortedByStatusLifetime(this List<CombatantCore> combatants, string effectName, bool special, bool ascending = false, int includeThreshold = int.MinValue)
    {
        List<CombatantReturnData> lOut = new List<CombatantReturnData>();
        int[] lifetime = new int[combatants.Count];
        int i, j, n;
        for (i = 0; i < lifetime.Length; i++)
        {
            lifetime[i] = combatants[i] == null ? 0 : combatants[i].statusEffects.Lifetime(effectName, special, !ascending);
        }
        lOut.Add(new CombatantReturnData(combatants[0].brain.friendly, combatants[0].index, combatants[0] == null ? 0 : lifetime[0]));
        if (!ascending)
        {
            for (i = 1; i < combatants.Count; i++)
            {
                if (lifetime[i] >= includeThreshold && lifetime[i] >= 0)
                {
                    n = lOut.Count;
                    for (j = 0; j <= n; j++)
                    {
                        if (j == n)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, lifetime[i]));
                        else if (lifetime[i] >= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, lifetime[i]));
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
                if (lifetime[i] <= includeThreshold && lifetime[i] >= 0)
                {
                    for (j = 0; j <= lOut.Count; j++)
                    {
                        if (j == lOut.Count)
                            lOut.Add(new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, lifetime[i]));
                        else if (lifetime[i] <= lOut[j].intValue)
                        {
                            lOut.Insert(j, new CombatantReturnData(combatants[i].brain.friendly, combatants[i].index, lifetime[i]));
                            break;
                        }
                    }
                }
            }
        }
        return lOut;
    }

    public static List<CombatantReturnData> SortedByStatusLifetime(this List<CombatantCore> combatants, StatusEffectReturnData effect, bool ascending = false, int includeThreshold = int.MinValue)
    {
        return combatants.SortedByStatusLifetime(effect.internalName, effect.special, ascending, includeThreshold);
    }
}

public class CombatValue
{
    public static float ScaledFloat(int baseValue, int level, float scalingPercent)
    {
        float scaling = Mathf.Clamp(scalingPercent, 0.0f, 1.0f) * 0.06f;
        level -= (ushort)(level > 0 ? 1 : 0);
        return (float)baseValue * Mathf.Exp(scaling * (float)level);
    }

    public static int ScaledInt(int baseValue, int level, float scalingPercent)
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

    public string CurrentPercentString { get { return ((float)Current / Scaled * 100f).ToString() + "%"; } }

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

    public static SpeedAtLevel Default { get { return new SpeedAtLevel(0, 80); } }
}

public class CombatSpeed
{
    public CombatantCore combatant = null;
    private int _level;
    public int level
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
    
    public CombatSpeed(CombatantCore combatant, SpeedAtLevel[] speeds, float variance = -1.0f)
    {
        this.combatant = combatant;
        level = 1;
        Overwrite(speeds);
        if (variance > 0.0f)
            ApplyVariance(variance);
    }
    
    public CombatSpeed(CombatantCore combatant, List<SpeedAtLevel> speeds, float variance = -1.0f)
    {
        this.combatant = combatant;
        level = 1;
        Overwrite(speeds);
        if (variance > 0.0f)
            ApplyVariance(variance);
    }
    
    public CombatSpeed(ushort level)
    {
        combatant = null;
        this.level = level;
        speeds.Add(SpeedAtLevel.Default);
    }
    
    public CombatSpeed(ushort level, SpeedAtLevel[] speeds, float variance = -1.0f)
    {
        combatant = null;
        this.level = level;
        Overwrite(speeds);
        if (variance > 0.0f)
            ApplyVariance(variance);
    }
    
    public CombatSpeed(ushort level, List<SpeedAtLevel> speeds, float variance = -1.0f)
    {
        combatant = null;
        this.level = level;
        Overwrite(speeds);
        if (variance > 0.0f)
            ApplyVariance(variance);
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

    public void ApplyVariance(float variance)
    {
        float v = Random.Range(-variance, variance);
        for (int i = 1; i < speeds.Count; i++)
        {
            speeds[i].value = Mathf.RoundToInt(speeds[i].value * (1.0f * v));
            if (speeds[i].value < 1)
                speeds[i].value = 1;
        }
    }

    public int GetAtLevel(int level)
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

    private CombatantCore combatant;

    public bool autonomous;
    public bool friendly;
    public int tauntedBy = -1;
    private int _lastAttackedBy = -1;
    public int lastAttackedBy
    {
        get
        {
            if (friendly)
            {
                if (CombatManager.enemyTeam.InBounds(_lastAttackedBy) && CombatManager.enemyTeam[_lastAttackedBy].alive)
                    return _lastAttackedBy;
                else
                    return -1;
            }
            else
            {
                if (CombatManager.allyTeam.InBounds(_lastAttackedBy) && CombatManager.allyTeam[_lastAttackedBy].alive)
                    return _lastAttackedBy;
                else
                    return -1;
            }
        }
        set
        {
            _lastAttackedBy = value;
        }
    }
    public int markedTarget = -1;

    public ActionSet actions;

    public int nextAction = 0;

    public SummonPool[] summonPools = null;
    public List<CombatantCore> summons = new List<CombatantCore>();
    public CombatantTeamIndex summonedBy = new CombatantTeamIndex(true, -1);

    public CombatantBrain(CombatantCore combatant, bool autonomous, bool friendly)
    {
        this.combatant = combatant;
        this.autonomous = autonomous;
        this.friendly = friendly;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public float ExecuteAction(ActionPoolCategory cat, int index, bool allowVariance = true) => actions.ExecuteAction(combatant, cat, index, allowVariance);
    public float ExecuteAction(ActionPoolCategory cat, CombatantTeamIndex[] targets, int index, bool allowVariance = true) => actions.ExecuteAction(combatant, targets, cat, index, allowVariance);
    public float ExecuteAction(int sequenceIndex, bool allowVariance = true) => actions.ExecuteAction(combatant, sequenceIndex, allowVariance);

    public float ExecuteNextAction()
    {
        string debugStr = nextAction.ToString();
        float t = actions.ExecuteAction(combatant, nextAction);
        nextAction++;
        if (nextAction >= actions.Sequence.Length)
            nextAction = 0;
        debugStr += nextAction.ToString();
        Debug.Log(debugStr);
        return t;
    }
}

[System.Serializable]
public class SummonPool
{
    public SummonItem[] summons;
}

[System.Serializable]
public class SummonItem
{
    public string hexUID;
    public float priority;
}

public class CombatantTrait
{

}
