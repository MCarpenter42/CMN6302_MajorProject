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
using UnityEngine.UIElements;

public class CombatManager : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] Transform allyParent;
    private Vector3 allyAnchor
    {
        get
        {
            if (allyParent == null)
                return Vector3.forward * 6.0f;
            else
                return allyParent.position;
        }
    }
    [SerializeField] Transform enemyParent;
    private Vector3 enemyAnchor
    {
        get
        {
            if (enemyParent == null)
                return Vector3.forward * -6.0f;
            else
                return enemyParent.position;
        }
    }
    [HideInInspector] public List<CombatantCore> playerTeam = new List<CombatantCore>();
    [HideInInspector] public List<CombatantCore> enemyTeam = new List<CombatantCore>();
    public List<CombatantCore> combatants
    {
        get
        {
            List<CombatantCore> combatants = new List<CombatantCore>();
            combatants.AddRange(playerTeam);
            combatants.AddRange(enemyTeam);
            return combatants;
        }
    }
    [SerializeField] GameObject turnIndicator;

    #endregion

    #region [ PROPERTIES ]

    public static float TimeFactor = 5000;
    public float combatTime { get; private set; }

    [HideInInspector] public int turnOfInd = -1;
    public CombatantCore turnOf { get { return combatants.InBounds(turnOfInd) ? combatants[turnOfInd] : null; } }

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_EventDelay = null;

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

    public Vector3[] CombatantPositions(bool enemyPos = true)
    {
        Vector3 anchor = enemyPos ? enemyAnchor : allyAnchor;
        float spacing = 2.0f, totalWidth = 0.0f;
        int i;
        float[] offsets;
        if (enemyPos)
        {
            offsets = new float[enemyTeam.Count];
            for (i = 1; i < offsets.Length; i++)
            {
                totalWidth += (enemyTeam[i - 1].size + enemyTeam[i].size) / 2.0f + spacing;
                offsets[i] = totalWidth;
            }
        }
        else
        {
            offsets = new float[playerTeam.Count];
            for (i = 1; i < offsets.Length; i++)
            {
                totalWidth += (playerTeam[i - 1].size + playerTeam[i].size) / 2.0f + spacing;
                offsets[i] = totalWidth;
            }
        }
        Vector3[] positions = new Vector3[offsets.Length];
        if (offsets.Length > 0)
        {
            positions[0] = anchor - Vector3.right * (totalWidth / 2.0f);
        }
        for (i = 1; i < positions.Length; i++)
        {
            positions[i] = positions[0] + Vector3.right * offsets[i];
        }
        return positions;
    }

    public bool SpawnCombatant(GameObject template, CombatantData data)
    {
        GameObject cmbObj = Instantiate(template);
        CombatantCore combatant = cmbObj.GetOrAddComponent<CombatantCore>();
        combatant.GetData(data);
        if (combatant.gotData)
        {
            cmbObj.name = data.displayName;
            combatants.Add(combatant);
            return true;
        }
        else
        {
            Destroy(cmbObj);
            return false;
        }
    }

    private void ReassignIndices()
    {
        int i;
        for (i = 0; i < playerTeam.Count; i++)
        {
            playerTeam[i].index = i;
        }
        for (i = 0; i < enemyTeam.Count; i++)
        {
            enemyTeam[i].index = i;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public struct TurnOrderRef
    {
        public bool playerTeam;
        public int index;
        public float nextActionTime;

        public TurnOrderRef(bool playerTeam, int index, float nextActionTime)
        {
            this.playerTeam = playerTeam;
            this.index = index;
            this.nextActionTime = nextActionTime;
        }

        public TurnOrderRef(TurnOrderRef template, float timeMod)
        {
            playerTeam = template.playerTeam;
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
        TurnOrderRef nxAct;
        int combatantCount = playerTeam.Count + enemyTeam.Count;
        int i, j;
        for (i = 0; i < order.Length; i++)
        {
            order[i] = new TurnOrderRef(true, 0, playerTeam[0].nextActionTime + playerTeam[0].actionInterval * i);
        }
        for (i = 1; i < playerTeam.Count; i++)
        {
            nxAct = new TurnOrderRef(true, i, playerTeam[i].nextActionTime);
            for (j = 0; j < order.Length; j++)
            {
                if (nxAct.nextActionTime < order[j].nextActionTime)
                {
                    order.Insert(i, nxAct);
                    nxAct = new TurnOrderRef(nxAct, playerTeam[i].actionInterval);
                }
            }
        }
        for (i = 0; i < enemyTeam.Count; i++)
        {
            nxAct = new TurnOrderRef(true, i, enemyTeam[i].nextActionTime);
            for (j = 0; j < order.Length; j++)
            {
                if (nxAct.nextActionTime < order[j].nextActionTime)
                {
                    order.Insert(i, nxAct);
                    nxAct = new TurnOrderRef(nxAct, enemyTeam[i].actionInterval);
                }
            }
        }
        return order;
    }

    public float TimeToAction(int combatantInd)
    {
        if (combatants.InBounds(combatantInd) && combatants[combatantInd].alive)
            return combatants[combatantInd].nextActionTime - combatTime;
        else
            return float.MaxValue;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void StartCombat(CombatantData[] allyList, CombatantData[] enemyList)
    {
        if (combatants.Count > 0)
        {
            for (int i = combatants.Count - 1; i >= 0; i--)
            {
                Destroy(combatants[i].gameObject, 0.001f);
                combatants.RemoveAt(i);
            }
        }

        GameObject temp = new GameObject();
        foreach (CombatantData ally in allyList)
        {
            SpawnCombatant(temp, ally);
        }
        foreach (CombatantData enemy in enemyList)
        {
            SpawnCombatant(temp, enemy);
        }
        Destroy(temp);
        ReassignIndices();
        Vector3[] allyPos = CombatantPositions(false);
        Vector3[] enemyPos = CombatantPositions(true);
        for (int i = 0; i < playerTeam.Count; i++)
        {
            playerTeam[i].pos = allyPos[i];
        }
        for (int i = 0; i < enemyTeam.Count; i++)
        {
            enemyTeam[i].pos = enemyPos[i];
            enemyTeam[i].rot = Vector3.up * 180.0f;
        }

        TurnOrderRef[] initActOrd = GetActionOrder(10);
        int index;
        for (int i = 0; i < 10; i++)
        {
            index = initActOrd[i].index;
            if (index >= 0)
            {
                TurnOrderItem lItem = Instantiate(GameManager.Instance.UI.HUD.turnOrderItem, GameManager.Instance.UI.HUD.turnOrderAnchor.transform);
                lItem.rTransform.anchoredPosition = Vector3.zero + Vector3.up * -80 * i;
                lItem.SetName(initActOrd[i].playerTeam ? playerTeam[index].displayName : enemyTeam[index].displayName);
            }
        }

        OpeningEvents();
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

    private void OpeningEvents()
    {
        if (c_EventDelay != null)
            StopCoroutine(c_EventDelay);
        c_EventDelay = StartCoroutine(IOpeningEvents());
    }

    private IEnumerator IOpeningEvents()
    {
        yield return null;
        NextTurn();
    }

    private int NextTurn()
    {
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
        combatTime = combatants[cInd].nextActionTime;
        turnIndicator.transform.position = combatants[cInd].pos;
        if (combatants[cInd].brain.autonomous)
        {
            float t = combatants[cInd].NextAction();
            AdvanceTurnOrder(t);
        }
        else
        {
            // ENABLE ACTION SELECTION HERE
        }
        return cInd;
    }

    public void AdvanceTurnOrder(float delay)
    {
        if (delay > 0.0f)
        {
            StartCoroutine(IAdvanceTurnOrder(delay));
        }
        else
        {
            NextTurn();
        }
    }

    private IEnumerator IAdvanceTurnOrder(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextTurn();
    }
}
