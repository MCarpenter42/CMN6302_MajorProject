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
    [HideInInspector] public List<CombatantCore> combatants = new List<CombatantCore>();
    public int[] allies
    {
        get
        {
            List<int> l = new List<int>();
            for (int i = 0; i < combatants.Count; i++)
            {
                if (combatants[i] != null && combatants[i].gotData && combatants[i].isFriendly)
                    l.Add(i);
            }
            return l.ToArray();
        }
    }
    public int[] enemies
    {
        get
        {
            List<int> l = new List<int>();
            for (int i = 0; i < combatants.Count; i++)
            {
                if (combatants[i] != null && combatants[i].gotData && !combatants[i].isFriendly)
                    l.Add(i);
            }
            return l.ToArray();
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
        Vector3 anchor;
        float spacing = 2.0f, totalWidth = 0.0f;
        int[] inds;
        int i;
        if (enemyPos)
        {
            anchor = enemyAnchor;
            inds = enemies;
        }
        else
        {
            anchor = allyAnchor;
            inds = allies;
        }
        float[] offsets = new float[inds.Length];
        for (i = 1; i < inds.Length; i++)
        {
            totalWidth += (combatants[inds[i - 1]].size + combatants[inds[i]].size) / 2.0f + spacing;
            offsets[i] = totalWidth;
        }
        Vector3[] positions = new Vector3[inds.Length];
        if (inds.Length > 0)
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

    public int[] GetActionOrder(ushort forecastMax)
    {
        List<Data_IntTag<float>> order = new List<Data_IntTag<float>>();
        Data_IntTag<float> nxAct;
        int x = Mathf.RoundToInt(forecastMax / combatants.Count) + 1, x2, x3;
        int i, j;
        for (i = 0; i < combatants.Count; i++)
        {
            x3 = x;
            if (combatants[i] != null && combatants[i].alive)
            {
                if (order.Count > 0)
                {
                    x2 = order.Count;
                    nxAct = new Data_IntTag<float>(i, combatants[i].nextActionTime);
                    for (j = 0; j < x2 && x3 > 0; j++)
                    {
                        if (nxAct.value < order[j].value)
                        {
                            x2++;
                            x3--;
                            order.Insert(j, nxAct);
                            nxAct = new Data_IntTag<float>(nxAct.tag, nxAct.value + combatants[i].actionInterval);
                        }
                    }
                    for (j = 0; j < x3; j++)
                    {
                        nxAct = new Data_IntTag<float>(nxAct.tag, nxAct.value + combatants[i].actionInterval);
                        order.Add(nxAct);
                    }
                }
                else
                {
                    for (j = 0; j < x; j++)
                    {
                        nxAct = new Data_IntTag<float>(i, combatants[i].nextActionTime + j * combatants[i].actionInterval);
                        order.Add(nxAct);
                    }
                }
            }
        }
        int[] ordOut = new int[forecastMax];
        for (i = 0; i < ordOut.Length; i++)
        {
            if (i < order.Count)
                ordOut[i] = order[i].tag;
            else
                ordOut[i] = -1;
        }
        return ordOut;
    }

    public float TimeToAction(int combatantInd)
    {
        if (combatants.InBounds(combatantInd) && combatants[combatantInd].alive)
            return combatants[combatantInd].nextActionTime - combatTime;
        else
            return float.MaxValue;
    }

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
        Vector3[] allyPos = CombatantPositions(false);
        Vector3[] enemyPos = CombatantPositions(true);
        for (int i = 0; i < allies.Length; i++)
        {
            combatants[allies[i]].pos = allyPos[i];
        }
        for (int i = 0; i < enemies.Length; i++)
        {
            combatants[enemies[i]].pos = enemyPos[i];
            combatants[enemies[i]].rot = Vector3.up * 180.0f;
        }

        int[] initActOrd = GetActionOrder(10);
        for (int i = 0; i < 10; i++)
        {
            if (initActOrd[i] >= 0)
            {
                TurnOrderItem lItem = Instantiate(GameManager.Instance.UI.HUD.turnOrderItem, GameManager.Instance.UI.HUD.turnOrderAnchor.transform);
                lItem.rTransform.anchoredPosition = Vector3.zero + Vector3.up * -80 * i;
                lItem.SetName(combatants[initActOrd[i]].displayName);
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
    }

    public int NextTurn()
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
        if (combatants[cInd].playerControlled)
        {
            // ENABLE PLAYER CONTROL
        }
        else
        {
            // PASS THROUGH TO AI
        }
        return cInd;
    }
}
