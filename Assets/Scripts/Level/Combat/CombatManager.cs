using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;

public class CombatManager : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] Transform enemyParent;
    private List<CombatEnemy> enemies = new List<CombatEnemy>();

    #endregion

    #region [ PROPERTIES ]



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

    public void StartCombat(EnemyData[] enemyList)
    {
        if (enemies.Count > 0)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Destroy(enemies[i].gameObject, 0.001f);
                enemies.RemoveAt(i);
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
            enemies.Add(enemy);
        }
        Destroy(temp);
    }

    public void StartCombatDelayed(EnemyData[] enemyList, float delay)
    {
        if (c_StartCombatDelayed != null)
            StopCoroutine(c_StartCombatDelayed);
        c_StartCombatDelayed = StartCoroutine(IStartCombatDelayed(enemyList, delay));
    }

    private IEnumerator IStartCombatDelayed(EnemyData[] enemyList, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCombat(enemyList);
    }
}
