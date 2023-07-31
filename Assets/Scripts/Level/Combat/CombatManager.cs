using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;

public class CombatManager : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] Transform allyParent;
    [SerializeField] Transform enemyParent;
    public List<CombatantCore> combatants = new List<CombatantCore>();
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

    #endregion

    #region [ PROPERTIES ]

    public static float TimeFactor = 5000;
    public float combatTime { get; private set; }

    public int turnOfInd = -1;
    public CombatantCore turnOf { get { return combatants.InBounds(turnOfInd) ? combatants[turnOfInd] : null; } }

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_StartCombatDelayed = null;

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
            anchor = enemyParent.position;
            inds = enemies;
        }
        else
        {
            anchor = allyParent.position;
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
        for (int i = 0; i < enemyList.Length && i < 9; i++)
        {
            GameObject enemyObj = Instantiate(temp, enemyParent);
            float xOff = 4.0f - (4.0f * (i % 3));
            float zOff = 4.0f - (4.0f * ((i - i % 3) / 3));
            enemyObj.transform.localPosition = new Vector3(xOff, 0.0f, zOff);
            enemyObj.transform.localEulerAngles = Vector3.zero;
            CombatEnemy enemy = enemyObj.AddComponent<CombatEnemy>();
            enemy.GetData(enemyList[i]);
            if (enemy.gotData)
                combatants.Add(enemy);
        }
        Destroy(temp);
    }

    public void StartCombatDelayed(CombatantData[] allyList, CombatantData[] enemyList, float delay)
    {
        if (c_StartCombatDelayed != null)
            StopCoroutine(c_StartCombatDelayed);
        c_StartCombatDelayed = StartCoroutine(IStartCombatDelayed(allyList, enemyList, delay));
    }

    private IEnumerator IStartCombatDelayed(CombatantData[] allyList, CombatantData[] enemyList, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCombat(allyList, enemyList);
    }

    public int[] GetActionOrder(ushort forecastMax)
    {
        List<Data_IntTag<float>> order = new List<Data_IntTag<float>>();
        float nxActTime;
        int i, j;
        for (i = 0; i < combatants.Count && order.Count < forecastMax; i++)
        {
            if (combatants[i] != null && combatants[i].alive)
            {
                nxActTime = combatants[i].nextActionTime;
                if (order.Count > 0)
                {
                    for (j = 0; j < order.Count; j++)
                    {
                        if (nxActTime < order[j].value)
                        {
                            order.Insert(j, new Data_IntTag<float>(i, nxActTime));
                        }
                    }
                    order.Add(new Data_IntTag<float>(i, nxActTime));
                }
                else
                {
                    order.Add(new Data_IntTag<float>(i, nxActTime));
                }
            }
        }
        int[] ordOut = new int[order.Count];
        for (i = 0; i < order.Count; i++)
        {
            ordOut[i] = order[i].tag;
        }
        return ordOut;
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
        return cInd;
    }

    public float TimeToAction(int combatantInd)
    {
        if (combatants.InBounds(combatantInd) && combatants[combatantInd].alive)
            return combatants[combatantInd].nextActionTime - combatTime;
        else
            return float.MaxValue;
    }
}
