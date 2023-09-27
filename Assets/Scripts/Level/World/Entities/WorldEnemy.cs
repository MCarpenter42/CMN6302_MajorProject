using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Unity;

public class WorldEnemy : WorldEntityCore
{
    private static List<EnemyData> EnemyData => GameManager.Instance.GameData.EnemyData;

    #region [ OBJECTS / COMPONENTS ]

    public WorldEnemySet Set { get; set; }

    #endregion

    #region [ PROPERTIES ]

    private bool setupComplete = false;

    public PositionInArea originPoint { get; private set; }

    [Header("World Options")]
    [SerializeField] bool roam = true;
    
    [Header("Combat Options")]
    [Range(1, 30)]
    public int level = 1;
    public int dataIndex = -1;
    private EnemyData _enemyData = null;
    public EnemyData enemyData
    {
        get
        {
            if (_enemyData == null && EnemyData.InBounds(dataIndex))
                _enemyData = EnemyData[dataIndex];
            return _enemyData;
        }
    }

    public EnemyClass Class { get { if (enemyData == null) Debug.Log("Data index: " + dataIndex); return enemyData.Class; } }

    //public WanderHandler wanderHandler;
    private List<Vector3> wanderPoints = new List<Vector3>();

    #endregion

    #region [ COROUTINES ]

    Coroutine c_doWander = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Initialise()
    {
        base.Initialise();
        GameManager.Instance.enemyListW.Add(this);
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void Setup(PositionInArea originPoint, WorldEnemySet set)
    {
        this.originPoint = originPoint;
        transform.position = originPoint.worldPosition;
        Set = set;

        //wanderHandler = new WanderHandler(transform.position, (LevelRoom)(set.area));
        wanderPoints.Add(transform.position);
        LevelRoom room = set.area as LevelRoom;
        int closestTile = room.ClosestTile(transform.position);
        for (int i = 0; i < 5; i++)
        {
            wanderPoints.Add(room.RandInternalPosition(closestTile));
            if (wanderPoints.Last().x == float.NaN)
                Debug.Log("Position adding failed!");
        }

        if (!disabled)
        {
            setupComplete = true;
            DoWander();
        }
    }

    public void SetData(int index)
    {
        if (EnemyData.InBounds(index))
        {
            dataIndex = index;
            _enemyData = EnemyData[index];
            string modelPath = EntityModel.GetModelPathFromUID(enemyData.modelHexUID);
            if (modelPath != null)
            {
                GameObject modelTemplate = Resources.Load<GameObject>(modelPath);
                if (modelTemplate != null)
                {
                    model = modelTemplate.GetComponent<EntityModel>();
                    UpdateModelObject(true);
                }
            }
        }
    }
    public void SetData(EnemyData data)
    {
        if (data != null)
        {
            dataIndex = EnemyData.IndexOf(data);
            _enemyData = data;
            string modelPath = EntityModel.GetModelPathFromUID(data.modelHexUID);
            if (modelPath != null)
            {
                GameObject modelTemplate = Resources.Load<GameObject>(modelPath);
                if (modelTemplate != null)
                {
                    model = modelTemplate.GetComponent<EntityModel>();
                    UpdateModelObject(true);
                }
            }
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void DoWander(bool resume = false)
    {
        if (c_doWander != null)
            StopCoroutine(c_doWander);
        StartCoroutine(IDoWander(resume));
    }

    public bool halt = false;
    private Vector3 last, target;

    private IEnumerator IDoWander(bool resume = false)
    {
        if (!resume)
        {
            last = transform.position;
            target = wanderPoints[Random.Range(0, wanderPoints.Count)];
        }
        if (last.x == float.NaN)
            Debug.Log("Origin position invalid!");
        if (target.x == float.NaN)
            Debug.Log("Target position invalid!");
        float facing, dist, t, tMax, delta;
        Vector3 deltaPos;
        while (!halt)
        {
            facing = (target - transform.position).Angle2D(DualAxis.XZ);
            RotateTo(facing, 0.4f);
            dist = (target - transform.position).magnitude;
            t = 0;
            tMax = dist / maxSpeed;
            while (t <= tMax && !halt)
            {
                yield return null;
                t += Time.deltaTime;
                delta = t / tMax;
                deltaPos = Vector3.Lerp(last, target, delta);
                if (deltaPos.x == float.NaN)
                    Debug.Log("Lerped position invalid!");
                if (transform.position.x == float.NaN)
                    Debug.Log("Current position invalid!");
                transform.position = deltaPos;
            }
            if (!halt)
            {
                transform.position = target;
                yield return new WaitForSeconds(Random.Range(2.0f, 8.0f));
                last = transform.position;
                target = wanderPoints[Random.Range(0, wanderPoints.Count)];
            }
        }
        halt = false;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void TriggerCombat()
    {
        if (!disabled && setupComplete)
            Set.TriggerCombat();
    }

    public void SetPaused(bool pause)
    {
        if (roam)
        {
            if (pause)
                halt = true;
            else
                DoWander(true);
        }
    }
}

public class WorldEnemySet
{
    public LevelArea area;

    public List<WorldEnemy> enemies;
    public EnemyData[] enemyData => GetData();
    public int[] dataIndices => GetDataIndices();
    public int Size { get { return enemies.Count; } }

    public bool defeated = false;

    public WorldEnemySet(LevelArea area = null)
    {
        this.area = area;
        enemies = new List<WorldEnemy>();
    }
    public WorldEnemySet(IList<WorldEnemy> toAdd, LevelArea area = null)
    {
        this.area = area;
        enemies = new List<WorldEnemy>();
        enemies.AddRange(toAdd);
    }

    public WorldEnemy this[int index]
    {
        get { return enemies.InBounds(index) ? enemies[index] : null; }
        set { if (enemies.InBounds(index)) { enemies[index] = value; } }
    }
    public EnemyData GetData(int index)
    {
        if (enemies.InBounds(index))
            return enemies[index].enemyData;
        else
            return null;
    }
    public int GetDataIndex(int index)
    {
        if (enemies.InBounds(index))
            return enemies[index].dataIndex;
        else
            return -1;
    }
    public EnemyData[] GetData()
    {
        EnemyData[] data = new EnemyData[enemies.Count];
        for (int i = 0; i < enemies.Count; i++)
        {
            data[i] = enemies[i].enemyData;
        }
        return data;
    }
    public int[] GetDataIndices()
    {
        int[] indices = new int[enemies.Count];
        for (int i = 0; i < enemies.Count; i++)
        {
            indices[i] = enemies[i].dataIndex;
        }
        return indices;
    }
    public void Add(WorldEnemy newEnemy, Vector3 position)
    {
        enemies.Add(newEnemy);
        newEnemy.Setup(new PositionInArea(area.ID, position), this);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void TriggerCombat() => GameManager.Instance.OnCombatStart(this);

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Reset()
    {
        if (enemies == null)
            enemies = new List<WorldEnemy>();
        else
            enemies.ClearAndDestroy();
    }

    public void OnDefeated()
    {
        defeated = true;
        foreach (WorldEnemy enemy in enemies)
        {
            if (enemy.enemyData.Class == EnemyClass.Standard)
                GameDataStorage.Data.runData.e_killedInStage.standard++;
            else if (enemy.enemyData.Class == EnemyClass.Elite)
                GameDataStorage.Data.runData.e_killedInStage.standard++;
            else if (enemy.enemyData.Class == EnemyClass.Boss)
                GameDataStorage.Data.runData.e_killedInStage.standard++;
        }
        Reset();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldEnemy))]
public class WorldEnemyEditor : Editor
{
    WorldEnemy targ { get { return target as WorldEnemy; } }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        string text = (targ.enemyData != null).ToString();
        EditorGUILayout.LabelField("Has data: ", text);
        if (targ.enemyData != null)
        {
            text = targ.enemyData.displayName;
            EditorGUILayout.LabelField("Display name: ", text);
        }
    }
}
#endif
