using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using NeoCambion.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using NeoCambion.Unity.Interpolation;
using NeoCambion.Maths;
using UnityEditor;
using UnityEngine.UIElements;

public class WorldEnemy : WorldEntityCore
{
    #region [ OBJECTS / COMPONENTS ]

    public WorldEnemySet Set { get; private set; }

    #endregion

    #region [ PROPERTIES ]

    private bool setupComplete = false;

    public PositionInArea originPoint { get; private set; }

    [Header("World Options")]
    [SerializeField] bool roam = true;
    
    [Header("Combat Options")]
    [Range(1, 30)]
    public int level = 1;
    public EnemyData enemyData;

    public EnemyClass Class { get { return enemyData.Class; } }

    //public WanderHandler wanderHandler;
    private List<Vector3> wanderPoints = new List<Vector3>();

    #endregion

    #region [ COROUTINES ]

    Coroutine c_doWander = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        GameManager.Instance.enemyListW.Add(this);

    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

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
        }

        LevelManager.AddEnemy(this);
        if (!disabled)
        {
            setupComplete = true;
            DoWander();
        }
    }

    public void SetData(EnemyData data)
    {
        if (data != null)
        {
            enemyData = data;
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
        float facing, dist, t, tMax, delta;
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
                transform.position = Vector3.Lerp(last, target, delta);
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

    public WorldEnemy[] enemies;
    public EnemyData[] enemyData => GetData();
    public int Size { get { return enemies.Length; } }

    public WorldEnemy this[int index]
    {
        get { return enemies.InBounds(index) ? enemies[index] : null; }
        set { if (enemies.InBounds(index)) { enemies[index] = value; } }
    }

    public WorldEnemySet(int setSize, LevelArea area)
    {
        enemies = new WorldEnemy[setSize];
        this.area = area;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public EnemyData[] GetData()
    {
        EnemyData[] data = new EnemyData[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            data[i] = enemies[i].enemyData;
        }
        return data;
    }
    public void TriggerCombat() => GameManager.Instance.OnCombatStart(this);
}

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
