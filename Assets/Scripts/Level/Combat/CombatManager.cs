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
using UnityEngine.Playables;

public class CombatManager : Core
{
    public GameDataStorage GameData { get { return GameManager.Instance.GameData; } }
    public HUDManager HUDManager { get { return UIManager.HUD; } }

    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] PivotArmCamera combatCamera;

    [Header("Parent Transforms")]
    [SerializeField] Transform allyParent;
    [SerializeField] Transform allyViewAnchor;
    [SerializeField] Transform enemyParent;
    [SerializeField] Transform enemyViewAnchor;

    [Header("Template Objects")]
    [SerializeField] GameObject allyPrefab;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject turnIndicator;
    [SerializeField] DamageNumber damageText;

    public CombatantsContainer combatants = new CombatantsContainer();
    public List<CombatPlayer> allyTeam { get { return combatants.allyTeam; } }
    public List<CombatEnemy> enemyTeam { get { return combatants.enemyTeam; } }

    #endregion

    #region [ PROPERTIES ]

    public bool disableStart = false;

    public static float TimeFactor = 5000;
    public float combatTime { get; private set; }
    public int turnCounter { get; private set; }

    public static float delayBetweenTurns = 1.5f;

    [HideInInspector] public int turnOfInd = -1;
    public CombatantCore currentlyActing { get { return combatants[turnOfInd]; } }

    [HideInInspector] public int playerSkillPower = 3;
    public static int maxSkillPower = 5;
    public bool canUseSkill => playerSkillPower > 0;

    [HideInInspector] public int playerUltPower = 0;
    public static int maxUltPower = 25;
    public static int requiredUltPower = 20;
    public bool canUseUlt => playerUltPower >= requiredUltPower;

    [HideInInspector] public bool selectionActive = false;
    [HideInInspector] public TargetSelection currentTargetingType = TargetSelection.None;
    [HideInInspector] public bool targetingAllies { get { return currentTargetingType == TargetSelection.Self || currentTargetingType == TargetSelection.Allied || currentTargetingType == TargetSelection.Self; } }
    [HideInInspector] public int currentAllyTarget = -1;
    [HideInInspector] public int currentEnemyTarget = -1;
    [HideInInspector] public int currentBlastWidth = 0;

    [HideInInspector] public CameraAngle defaultCameraAngle;
    [HideInInspector] public CameraAngle defaultCameraAngleFlipped;

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_EventDelay = null;
    private Coroutine c_RotateOverview = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected override void Initialise()
    {
        defaultCameraAngle = new CameraAngle(combatCamera.position, combatCamera.eulerAngles, combatCamera.camOffset);
        defaultCameraAngleFlipped = new CameraAngle(defaultCameraAngle, new Vector3(0f, 180f, 0f));
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public struct TurnOrderRef
    {
        public bool allyTeam;
        public int index;
        public float nextActionTime;

        public TurnOrderRef(bool allyTeam, int index, float nextActionTime)
        {
            this.allyTeam = allyTeam;
            this.index = index;
            this.nextActionTime = nextActionTime;
        }

        public TurnOrderRef(TurnOrderRef template, float timeMod)
        {
            allyTeam = template.allyTeam;
            index = template.index;
            nextActionTime = template.nextActionTime + timeMod;
        }

        public static TurnOrderRef Empty
        {
            get
            {
                return new TurnOrderRef(true, -1, float.MaxValue);
            }
        }

        public bool IsEmpty()
        {
            return index == -1 && nextActionTime == float.MaxValue;
        }
    }
    public TurnOrderRef[] GetActionOrder(ushort forecastMax)
    {
        TurnOrderRef[] order = new TurnOrderRef[forecastMax];
        List<TurnOrderRef> reflist = new List<TurnOrderRef>();
        float nxTime;
        bool valueAdded;
        int i, j, k, checkMax = Mathf.RoundToInt((float)forecastMax / combatants.Count) + 1;
        reflist.Add(new TurnOrderRef(true, -1, float.MaxValue));
        for (i = 0; i < checkMax; i++)
        {
            valueAdded = false;
            for (j = 0; j < allyTeam.Count; j++)
            {
                nxTime = allyTeam[j].nextActionTime + allyTeam[j].actionInterval * i;
                for (k = 0; k < forecastMax && k < reflist.Count; k++)
                {
                    if (nxTime < reflist[k].nextActionTime)
                    {
                        reflist.Insert(k, new TurnOrderRef(true, j, nxTime));
                        valueAdded = true;
                        break;
                    }
                }
            }
            for (j = 0; j < enemyTeam.Count; j++)
            {
                nxTime = enemyTeam[j].nextActionTime + enemyTeam[j].actionInterval * i;
                for (k = 0; k < forecastMax && k < reflist.Count; k++)
                {
                    if (nxTime < reflist[k].nextActionTime)
                    {
                        reflist.Insert(k, new TurnOrderRef(false, j, nxTime));
                        valueAdded = true;
                        break;
                    }
                }
            }
            if (!valueAdded)
                break;
        }
        for (i = 0; i < order.Length; i++)
        {
            order[i] = reflist[i];
        }
        return order;
    }

    public float TimeToAction(int combatantInd)
    {
        CombatantCore combantant = combatants[combatantInd];
        if (combantant.alive)
            return combantant.nextActionTime - combatTime;
        else
            return float.MaxValue;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    FloatRange spacing = new FloatRange(1f, 2f);
    public void StartCombat(CombatantData[] allyList, CombatantData[] enemyList)
    {
        Debug.Log("- - - - -");
        Debug.Log("Combat started with " + allyList.Length + " allies and " + enemyList.Length + " enemies");
        if (!disableStart)
        {
            combatants.allyTransform = allyParent;
            combatants.enemyTransform = enemyParent;

            if (combatants.Count > 0)
                combatants.Clear();

            combatants.Initialise(allyList, allyPrefab, enemyList, enemyPrefab);

            combatants.ReassignIndices();

            combatants.UpdateTeamPositions(true, spacing);
            combatants.UpdateTeamPositions(false, spacing);

            ResetCombatValues();
            playerUltPower = GameData.runData.p_ultCharge;
            HUDManager.UpdateSkillPowerDisplay(playerSkillPower);
            HUDManager.UpdateUltPowerDisplay(playerUltPower);

            for (int i = 0; i < allyTeam.Count; i++)
            {
                allyTeam[i].healthBar = HUDManager.playerHealthBars_Combat[i];
            }
            foreach (CombatEnemy combatant in enemyTeam)
            {
                if (combatant.healthBar != null)
                    ((HealthBar3D)combatant.healthBar).rotateTarget = combatCamera.cam.transform;
            }

            foreach (CombatEnemy enemy in enemyTeam)
            {
                enemy.rotation = 180.0f * Vector3.up;
            }

            OpeningEvents();
        }
    }

    public void StartCombatDelayed(CombatantData[] allyList, CombatantData[] enemyList, float delay)
    {
        if (c_EventDelay != null)
            StopCoroutine(c_EventDelay);
        c_EventDelay = StartCoroutine(IStartCombatDelayed(allyList, enemyList, delay));
    }

    private IEnumerator IStartCombatDelayed(CombatantData[] allyList, CombatantData[] enemyList, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCombat(allyList, enemyList);
    }

    private void ResetCombatValues()
    {
        combatTime = 0f;
        turnCounter = 0;

        currentTargetingType = TargetSelection.None;
        currentAllyTarget = GetMiddle(true);
        currentEnemyTarget = GetMiddle(false);

        HUDManager.SetAbilityButtonsEnabled(false);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void OpeningEvents()
    {
        if (c_EventDelay != null)
            StopCoroutine(c_EventDelay);
        c_EventDelay = StartCoroutine(IOpeningEvents());
    }

    private IEnumerator IOpeningEvents()
    {
        //yield return new WaitForSeconds(RotateOverview());
        yield return null;
        yield return new WaitForSeconds(0.1f);
        NextTurn();
    }

    private float RotateOverview()
    {
        float duration = 5.0f;
        if (c_RotateOverview != null)
            StopCoroutine(c_RotateOverview);
        c_RotateOverview = StartCoroutine(IRotateOverview(duration));
        return duration;
    }

    private IEnumerator IRotateOverview(float duration)
    {
        Transform pivot = combatCamera.pivot;
        Vector3 rotStart = pivot.eulerAngles, rotOffset = Vector3.zero;
        float t = 0f, tMax = duration, delta;
        while(t < duration)
        {
            yield return null;
            t += Time.deltaTime;
            delta = t / tMax;
            rotOffset.y = Mathf.Lerp(0f, 360f, InterpDelta.CosCurve(delta));
            pivot.eulerAngles = rotStart + rotOffset;
        }
        pivot.eulerAngles = rotStart;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void OnTurnStarted(CombatantCore combatant, float time)
    {
        Debug.Log("Turn started: " + combatant.displayName + " (" + (combatant.brain.friendly ? "Player" : "Enemy") + ")\nCombat time: " + time);
    }
    
    public void OnTurnEnded(CombatantCore combatant, float time)
    {
        Debug.Log("Turn ended: " + combatant.displayName + " (" + (combatant.brain.friendly ? "Player" : "Enemy") + ")\nCombat time: " + time);
    }

    public void OnCombatantDamaged(CombatantCore combatant, int value)
    {
        if (value > 0)
            Debug.Log(combatant.displayName + " was damaged for " + value + ", leaving them on " + (combatant.health.Current - value) + "/" + combatant.health.Scaled + " health (" + combatant.health.CurrentPercentString + ")");
        else
            Debug.Log(combatant.displayName + " was attacked but took no damage, leaving them on " + combatant.health.Current + "/" + combatant.health.Scaled + " health (" + combatant.health.CurrentPercentString + ")");
        Vector3 dmgNumPos = combatant.damageTextAnchor == null ? combatant.transform.position + Vector3.up : combatant.damageTextAnchor.position;
        SpawnDamageNumber("-" + value.ToString(), dmgNumPos, Color.red, 0.15f, 0.45f, 0.4f);
    }

    public void OnCombatantShieldDamaged(CombatantCore combatant, int value)
    {
        Debug.Log(combatant.displayName + "'s shields were damaged for " + value);
        Vector3 dmgNumPos = combatant.damageTextAnchor == null ? combatant.transform.position + Vector3.up : combatant.damageTextAnchor.position;
        SpawnDamageNumber("-" + value.ToString(), dmgNumPos, Color.cyan, 0.15f, 0.45f, 0.4f);
    }

    public void OnCombatantHealed(CombatantCore combatant, int value)
    {
        Debug.Log(combatant.displayName + " was healed for " + value + ", leaving them on " + (combatant.health.Current + value) + " health (" + combatant.health.CurrentPercentString + ")");
        Vector3 dmgNumPos = combatant.damageTextAnchor == null ? combatant.transform.position + Vector3.up : combatant.damageTextAnchor.position;
        SpawnDamageNumber("+" + value.ToString(), dmgNumPos, Color.green, 0.15f, 0.45f, 0.4f);
    }

    public void OnCombatantShielded(CombatantCore combatant, int value)
    {
        Debug.Log(combatant.displayName + " was killed!");
        Vector3 dmgNumPos = combatant.damageTextAnchor == null ? combatant.transform.position + Vector3.up : combatant.damageTextAnchor.position;
        SpawnDamageNumber("+" + value.ToString(), dmgNumPos, Color.cyan, 0.15f, 0.45f, 0.4f);
    }

    public void OnCombatantDied(CombatantCore combatant)
    {
        Debug.Log(combatant.displayName + " was killed!");
    }

    public void OnCombatantRevived(CombatantCore combatant, int healedBy)
    {
        Debug.Log(combatant.displayName + " was revived, and healed for " + healedBy + "!");
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private int NextTurn()
    {
        if (turnOfInd > -1)
            OnTurnEnded(currentlyActing, combatTime);

        turnCounter++;
        HUDManager.DrawActionOrder(GetActionOrder(10));

        float lowest = float.MaxValue;
        int cInd = -1;
        for (int i = 0; i < combatants.Count; i++)
        {
            if (combatants[i].nextActionTime < lowest)
            {
                cInd = i;
                lowest = combatants[i].nextActionTime;
            }
        }
        turnOfInd = cInd;
        OnTurnStarted(currentlyActing, combatTime);

        combatTime = combatants[cInd].nextActionTime;
        combatants[cInd].lastActionTime = combatants[cInd].nextActionTime;
        turnIndicator.transform.position = combatants[cInd].position;
        if (combatants[cInd].brain.autonomous)
        {
            float t = combatants[cInd].NextAction();
            AdvanceTurnOrder(t);
        }
        else
        {
            StartPlayerTurn();
        }
        return cInd;
    }

    public void AdvanceTurnOrder(float delay)
    {
        StartCoroutine(IAdvanceTurnOrder(delay));
    }

    private IEnumerator IAdvanceTurnOrder(float delay)
    {
        yield return new WaitForSeconds(delay + delayBetweenTurns / 2f);
        RemoveDead();
        if (!EndCondition())
        {
            yield return new WaitForSeconds(delayBetweenTurns / 2f);
            NextTurn();
        }
    }

    public void RemoveDead()
    {
        for (int i = enemyTeam.Count - 1; i >= 0; i--)
        {
            if (!enemyTeam[i].alive)
            {
                enemyTeam[i].gameObject.DestroyThis();
                enemyTeam.RemoveAt(i);
            }
        }
        combatants.ReassignIndices();
        combatants.UpdateTeamPositions(false, spacing);
    }

    public bool EndCondition()
    {
        if (enemyTeam.Count > 0)
        {
            if (combatants.anyAllyAlive)
                return false;
            else
            {
                GameManager.Instance.OnCombatEnd(false, 2.5f);
                ResetCombat();
                GameData.SaveRunData();
                return true;
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                GameData.runData.p_healthValues[i] = new int[] { allyTeam[i].health.Current, allyTeam[i].health.Scaled };
                GameData.playerData[i].currentHealthPercent = GameData.runData.p_healthPercentages[i];
            }
            GameManager.Instance.OnCombatEnd(true, 2.5f);
            ResetCombat();
            GameData.SaveRunData();
            return true;
        }
    }

    public void ResetCombat()
    {
        combatants.Clear();
        turnOfInd = -1;
        playerSkillPower = 3;
        selectionActive = false;
        currentTargetingType = TargetSelection.None;
        currentAllyTarget = -1;
        currentEnemyTarget = -1;
        currentBlastWidth = 0;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void StartPlayerTurn()
    {
        HUDManager.SetAbilityButtonsEnabled(true);
        StartPlayerTargetSelection(ActionPoolCategory.Standard);
    }
    
    public void EndPlayerTurn(float delay)
    {
        HUDManager.SetAbilityButtonsEnabled(false);
        AdvanceTurnOrder(delay);
    }

    private int GetMiddle(bool playerTeam)
    {
        if (playerTeam)
            return (allyTeam.Count - 1) / 2;
        else
            return (enemyTeam.Count - 1) / 2;
    }

    private void SetVisibleTargetArrows(bool playerTeam, int index, bool targetAll, int blastWidth = 0)
    {
        if (playerTeam)
        {
            if (index < 0)
                index = 0;
            else if (index >= allyTeam.Count)
                index = allyTeam.Count - 1;
            if (targetAll)
            {
                foreach (CombatantCore ally in allyTeam)
                {
                    ally.SetTargetedState(TargetArrowState.AOE);
                }
            }
            else
            {
                allyTeam[index].SetTargetedState(TargetArrowState.Direct);
                for (int i = index - 1; i >= 0; i--)
                {
                    if (i >= index - blastWidth)
                        allyTeam[i].SetTargetedState(TargetArrowState.Blast);
                    else
                        allyTeam[i].SetTargetedState(TargetArrowState.None);
                }
                for (int i = index + 1; i < allyTeam.Count; i++)
                {
                    if (i <= index + blastWidth)
                        allyTeam[i].SetTargetedState(TargetArrowState.Blast);
                    else
                        allyTeam[i].SetTargetedState(TargetArrowState.None);
                }
            }
            foreach (CombatantCore enemy in enemyTeam)
            {
                enemy.SetTargetedState(TargetArrowState.None);
            }
        }
        else
        {
            if (index < 0)
                index = 0;
            else if (index >= enemyTeam.Count)
                index = enemyTeam.Count - 1;
            if (targetAll)
            {
                foreach (CombatantCore enemy in enemyTeam)
                {
                    enemy.SetTargetedState(TargetArrowState.AOE);
                }
            }
            else
            {
                enemyTeam[index].SetTargetedState(TargetArrowState.Direct);
                for (int i = index - 1; i >= 0; i--)
                {
                    if (i >= index - blastWidth)
                        enemyTeam[i].SetTargetedState(TargetArrowState.Blast);
                    else
                        enemyTeam[i].SetTargetedState(TargetArrowState.None);
                }
                for (int i = index + 1; i < enemyTeam.Count; i++)
                {
                    if (i <= index + blastWidth)
                        enemyTeam[i].SetTargetedState(TargetArrowState.Blast);
                    else
                        enemyTeam[i].SetTargetedState(TargetArrowState.None);
                }
            }
            foreach (CombatantCore ally in allyTeam)
            {
                ally.SetTargetedState(TargetArrowState.None);
            }
        }
    }
    private void ClearVisibleTargetArrows()
    {
        foreach (CombatantCore combatant in allyTeam)
        {
            combatant.SetTargetedState(TargetArrowState.None);
        }
        foreach (CombatantCore combatant in enemyTeam)
        {
            combatant.SetTargetedState(TargetArrowState.None);
        }
    }

    public void StartPlayerTargetSelection(ActionPoolCategory ability)
    {
        selectionActive = true;
        CombatantCore combatant = currentlyActing;
        switch (ability)
        {
            default:
                currentTargetingType = TargetSelection.None;
                currentBlastWidth = 0;
                break;

            case ActionPoolCategory.Standard:
                currentTargetingType = combatant.brain.actions.standard[0].targeting.selection;
                currentBlastWidth = combatant.brain.actions.standard[0].multiTarget.count;
                break;

            case ActionPoolCategory.Advanced:
                currentTargetingType = combatant.brain.actions.advanced[0].targeting.selection;
                currentBlastWidth = combatant.brain.actions.advanced[0].multiTarget.count;
                break;

            case ActionPoolCategory.Special:
                currentTargetingType = combatant.brain.actions.special[0].targeting.selection;
                currentBlastWidth = combatant.brain.actions.special[0].multiTarget.count;
                break;
        }
        //Debug.Log("Beginning target selection for " + combatant.displayName + ": " + ability.ToString() + "\nTargeting type: " + currentTargetingType.ToString() + " | Blast width: " + currentBlastWidth);

        if (targetingAllies)
        {
            if (currentAllyTarget >= allyTeam.Count)
                currentAllyTarget = GetMiddle(true);
            combatCamera.SetViewAngle(defaultCameraAngleFlipped);
            SetVisibleTargetArrows(true, currentAllyTarget, currentTargetingType == TargetSelection.AlliedAll, currentBlastWidth);
        }
        else
        {
            if (currentEnemyTarget >= enemyTeam.Count)
                currentEnemyTarget = GetMiddle(false);
            combatCamera.SetViewAngle(defaultCameraAngle);
            SetVisibleTargetArrows(false, currentEnemyTarget, currentTargetingType == TargetSelection.OpposedAll, currentBlastWidth);
        }
    }

    public void MoveSelect(bool right)
    {
        if (selectionActive)
        {
            if (targetingAllies)
            {
                currentAllyTarget = (currentAllyTarget + (right ? 1 : -1)).Clamp(0, allyTeam.Count);
                combatCamera.SetViewAngle(defaultCameraAngleFlipped);
                SetVisibleTargetArrows(true, currentAllyTarget, currentTargetingType == TargetSelection.AlliedAll, currentBlastWidth);
            }
            else
            {
                currentEnemyTarget = (currentEnemyTarget + (right ? 1 : -1)).Clamp(0, enemyTeam.Count);
                combatCamera.SetViewAngle(defaultCameraAngle);
                SetVisibleTargetArrows(false, currentEnemyTarget, currentTargetingType == TargetSelection.OpposedAll, currentBlastWidth);
            }
        }
    }
    public void MoveSelectTo(int index)
    {
        if (selectionActive)
        {
            if (targetingAllies)
            {
                currentAllyTarget = index.Clamp(0, allyTeam.Count);
                combatCamera.SetViewAngle(defaultCameraAngleFlipped);
                SetVisibleTargetArrows(true, currentAllyTarget, currentTargetingType == TargetSelection.AlliedAll, currentBlastWidth);
            }
            else
            {
                currentEnemyTarget = index.Clamp(0, enemyTeam.Count);
                combatCamera.SetViewAngle(defaultCameraAngle);
                SetVisibleTargetArrows(false, currentEnemyTarget, currentTargetingType == TargetSelection.OpposedAll, currentBlastWidth);
            }
        }
    }

    private CombatantTeamIndex[] GetCurrentTargets()
    {
        CombatantTeamIndex[] targets;
        switch (currentTargetingType)
        {
            default:
            case TargetSelection.None:
                targets = new CombatantTeamIndex[0];
                break;

            case TargetSelection.Allied:
                targets = new CombatantTeamIndex[1] { allyTeam[currentAllyTarget].teamIndex };
                break;
                
            case TargetSelection.AlliedAll:
                targets = new CombatantTeamIndex[allyTeam.Count];
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i] = new CombatantTeamIndex(true, i);
                }
                break;

            case TargetSelection.Opposed:
                targets = new CombatantTeamIndex[1] { enemyTeam[currentEnemyTarget].teamIndex };
                break;

            case TargetSelection.OpposedAll:
                targets = new CombatantTeamIndex[enemyTeam.Count];
                for (int i = 0; i < targets.Length; i++)
                {
                    targets[i] = new CombatantTeamIndex(false, i);
                }
                break;
        }
        return targets;
    }

    public void TriggerPlayerAbility()
    {
        ActionPoolCategory type = HUDManager.selectedAbilityType;
        TriggerPlayerAbility(type);
    }
    public void TriggerPlayerAbility(ActionPoolCategory type)
    {
        Debug.Log("Used player ability: " + type);
        if (currentlyActing.brain.friendly)
        {
            CombatantTeamIndex[] targets = GetCurrentTargets();
            selectionActive = false;
            ClearVisibleTargetArrows();
            if (type == ActionPoolCategory.Standard)
            {
                AdjustSkillPower(+1);
                AdjustUltPower(+1);
                if (targets.Length > 0)
                    EndPlayerTurn(currentlyActing.brain.ExecuteAction(type, targets, 0, false));
                else
                    EndPlayerTurn(currentlyActing.brain.ExecuteAction(type, 0, false));
            }
            else if (type == ActionPoolCategory.Advanced)
            {
                if (playerSkillPower > 0)
                {
                    AdjustSkillPower(-1);
                    AdjustUltPower(+2);
                    if (targets.Length > 0)
                        EndPlayerTurn(currentlyActing.brain.ExecuteAction(type, targets, 0, false));
                    else
                        EndPlayerTurn(currentlyActing.brain.ExecuteAction(type, 0, false));
                }
            }
            else if (type == ActionPoolCategory.Special)
            {
                if (playerUltPower >= requiredUltPower)
                {
                    AdjustUltPower(-requiredUltPower);
                    if (targets.Length > 0)
                        EndPlayerTurn(currentlyActing.brain.ExecuteAction(type, targets, 0, false));
                    else
                        EndPlayerTurn(currentlyActing.brain.ExecuteAction(type, 0, false));
                }
            }
        }
    }

    public void AdjustSkillPower(int adjustBy)
    {
        playerSkillPower += adjustBy;
        if (playerSkillPower < 0)
            playerSkillPower = 0;
        else if (playerSkillPower > maxSkillPower)
            playerSkillPower = maxSkillPower;

        HUDManager.UpdateSkillPowerDisplay(playerSkillPower);
    }

    public void AdjustUltPower(int adjustBy)
    {
        playerUltPower += adjustBy;
        if (playerUltPower < 0)
            playerUltPower = 0;
        else if (playerUltPower > maxUltPower)
            playerUltPower = maxUltPower;
        GameData.runData.p_ultCharge = playerUltPower;
        HUDManager.UpdateUltPowerDisplay(playerUltPower);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void SpawnDamageNumber(string value, Vector3 spawnAt, Color color, float tFadeIn, float tPause, float tFadeOut)
    {
        DamageNumber dmgNum = Instantiate(damageText, spawnAt, Quaternion.identity);
        dmgNum.Text = value;
        dmgNum.lookTarg = combatCamera.cameraTransform;
        dmgNum.Animate(color, tFadeIn, tPause, tFadeOut);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void ActionAnim(string actionName, CombatantTeamIndex origin, CombatantTeamIndex target, float delayA, float delayB)
    {
        HUDManager.ActionNameDisplay(actionName, delayA + delayB, 0.8f);
        CameraAngle actorViewAngle, targetViewAngle;

        if (origin.playerTeam)
            actorViewAngle = new CameraAngle(allyTeam[origin.teamIndex].cameraViewAnchor.position, new Vector3(5f, 190f, 0f), 3f);
        else
            actorViewAngle = new CameraAngle(enemyTeam[origin.teamIndex].cameraViewAnchor.position, new Vector3(5f, 010f, 0f), 3f);

        if (target.playerTeam)
            targetViewAngle = new CameraAngle(allyTeam[target.teamIndex].cameraViewAnchor.position, new Vector3(5f, 190f, 0f), 3f);
        else
            targetViewAngle = new CameraAngle(enemyTeam[target.teamIndex].cameraViewAnchor.position, new Vector3(5f, 010f, 0f), 3f);

        StartCoroutine(IActionAnim(actorViewAngle, targetViewAngle, delayA, delayB));
    }
    public void ActionAnim(string actionName, CombatantTeamIndex origin, bool targetPlayerTeam, float delayA, float delayB)
    {
        HUDManager.ActionNameDisplay(actionName, delayA + delayB, 0.8f);
        CameraAngle actorViewAngle, targetViewAngle;

        if (origin.playerTeam)
            actorViewAngle = new CameraAngle(allyTeam[origin.teamIndex].cameraViewAnchor.position, new Vector3(5f, 190f, 0f), 3f);
        else
            actorViewAngle = new CameraAngle(enemyTeam[origin.teamIndex].cameraViewAnchor.position, new Vector3(5f, 010f, 0f), 3f);

        if (targetPlayerTeam)
            targetViewAngle = new CameraAngle(allyViewAnchor.position, new Vector3(15f, 180f, 0f), 9f);
        else
            targetViewAngle = new CameraAngle(enemyViewAnchor.position, new Vector3(15f, 000f, 0f), 9f);

        StartCoroutine(IActionAnim(actorViewAngle, targetViewAngle, delayA, delayB));
    }
    private IEnumerator IActionAnim(CameraAngle actorViewAngle, CameraAngle targetViewAngle, float actorViewDur, float targetViewDur)
    {
        combatCamera.SetViewAngle(actorViewAngle);
        yield return new WaitForSeconds(actorViewDur);
        combatCamera.SetViewAngle(targetViewAngle);
        yield return new WaitForSeconds(targetViewDur);
        combatCamera.SetViewAngle(defaultCameraAngle);
    }
}

public class CombatantsContainer
{
    public GameDataStorage GameData => GameManager.Instance.GameData;

    public Transform allyTransform;
    public Vector3 allyAnchor => allyTransform == null ? -6f * Vector3.forward : allyTransform.position;
    public List<CombatPlayer> allyTeam;
    public bool anyAllyAlive
    {
        get
        {
            foreach (CombatPlayer ally in allyTeam)
                if (ally.alive)
                    return true;
            return false;
        }
    }

    public Transform enemyTransform;
    public Vector3 enemyAnchor => enemyTransform == null ? 6f * Vector3.forward : enemyTransform.position;
    public List<CombatEnemy> enemyTeam;

    public int Count => (allyTeam == null ? 0 : allyTeam.Count) + (enemyTeam == null ? 0 : enemyTeam.Count);

    public CombatantCore this[bool ally, int index] => ally ? allyTeam[index]: enemyTeam[index];
    public CombatantCore this[int indexOverall] => indexOverall >= allyTeam.Count ? enemyTeam[indexOverall - allyTeam.Count] : allyTeam[indexOverall];

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Clear()
    {
        if (allyTeam.Count > 0)
            allyTeam.ClearAndDestroy();
        if (enemyTeam.Count > 0)
            enemyTeam.ClearAndDestroy();
    }

    public void Initialise(CombatantData[] allyData, GameObject allyTemplate, CombatantData[] enemyData, GameObject enemyTemplate)
    {
        allyTeam = new List<CombatPlayer>();
        float perc;
        for (int i = 0; i < 4; i++)
        {
            CombatPlayer ally = allyTeam.AddClone(allyTemplate, allyTransform);
            ally.healthBar = Core.UIManager.HUD.playerHealthBars_Combat[i];
            ally.GetData(allyData[i], GameData.runData.p_level);
            if (ally.gotData)
            {
                ally.gameObject.name = allyData[i].displayName;
                ally.health.Current = Mathf.RoundToInt(GameData.playerData[i].currentHealthPercent * ally.health.ScaledAsFloat);
                perc = ally.health.Current / ally.health.ScaledAsFloat;
                (ally.healthBar as HealthBarCanvas).SetValue(ally.health.Current, ally.health.Scaled, 0f);
                //Debug.Log(i + ": " + GameData.playerData[i].currentHealthPercent + " / " + perc);
            }
            else
                allyTeam.RemoveLastAndDestroy();
        }

        enemyTeam = new List<CombatEnemy>();
        foreach (EnemyData data in enemyData)
        {
            CombatEnemy enemy = enemyTeam.AddClone(enemyTemplate, enemyTransform);
            enemy.GetData(data);
            if (enemy.gotData)
                enemy.gameObject.name = data.displayName;
            else
                enemyTeam.RemoveLastAndDestroy();
        }
    }

    public void ReassignIndices()
    {
        int i;
        for (i = 0; i < allyTeam.Count; i++)
        {
            allyTeam[i].index = i;
        }
        for (i = 0; i < enemyTeam.Count; i++)
        {
            enemyTeam[i].index = i;
        }
    }

    public void UpdateTeamPositions(bool allies, FloatRange spacingInfo)
    {
        float spacing, w;
        int indL, indR;
        Vector3 posL, posR;
        if (allies && allyTeam.Count > 1)
        {
            posL = allyAnchor;
            posR = allyAnchor;
            w = allyTeam[0].Size;
            posL.x += ((allyTeam.Count - 1) / 2f) * (spacingInfo.Upper + w);
            allyTeam[0].position = posL;
            for (int i = 1; i < allyTeam.Count; i++)
            {
                posL.x -= spacingInfo.Upper + w;
                allyTeam[i].position = posL;
            }
        }
        else if (!allies && enemyTeam.Count > 1)
        {
            posL = enemyAnchor;
            posR = enemyAnchor;
            if (enemyTeam.Count <= 4)
                spacing = spacingInfo.Upper;
            else
            {
                switch (enemyTeam.Count)
                {
                    case 5: spacing = spacingInfo.Upper - 0.25f * spacingInfo.Range; break;
                    case 6: spacing = spacingInfo.Upper - 0.50f * spacingInfo.Range; break;
                    case 7: spacing = spacingInfo.Upper - 0.75f * spacingInfo.Range; break;
                    default: spacing = spacingInfo.Lower; break;
                }
            }

            if (enemyTeam.Count % 2 == 1)
            {
                int centreInd = (enemyTeam.Count - 1) / 2;
                enemyTeam[centreInd].position = enemyAnchor;
                indR = centreInd + 1;
                posR.x += enemyTeam[centreInd].Size / 2f;
                indL = centreInd - 1;
                posL.x -= enemyTeam[centreInd].Size / 2f;
            }
            else
            {
                indR = enemyTeam.Count / 2;
                indL = indR - 1;

                posL.x -= (enemyTeam[indL].Size + spacing) / 2f;
                enemyTeam[indL].position = posL;
                posL.x -= enemyTeam[indL].Size / 2f;
                indL--;

                posR.x += (enemyTeam[indR].Size + spacing) / 2f;
                enemyTeam[indR].position = posR;
                posR.x += enemyTeam[indR].Size / 2f;
                indR++;
            }

            for (; indL >= 0 && indR < enemyTeam.Count; indL--, indR++)
            {
                posL.x -= enemyTeam[indL].Size / 2f + spacing;
                enemyTeam[indL].position = posL;
                posL.x -= enemyTeam[indL].Size / 2f;

                posR.x += enemyTeam[indR].Size / 2f + spacing;
                enemyTeam[indR].position = posR;
                posR.x += enemyTeam[indR].Size / 2f;
            }
        }
    }
}
