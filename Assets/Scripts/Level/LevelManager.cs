using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections.Unity;
using JetBrains.Annotations;

[RequireComponent(typeof(Generation))]
public class LevelManager : Core
{
    #region [ LEVEL GENERATION ]

    public Generation Generator { get { return GetComponent<Generation>(); } }
    public Generation.Grid TileGrid { get; private set; }

    public List<LevelRoom> Rooms { get; private set; }
    public List<LevelCorridor> Corridors { get; private set; }
    public List<LevelArea> AllAreas { get; private set; }

    public List<WorldEnemy> AllEnemies { get; private set; }
    public List<WorldItem> AllItems { get; private set; }

    #endregion

    #region [ OBJECTS / COMPONENTS ]

    [HideInInspector] public Transform tileParent;

    [HideInInspector] public PlayerSpawn spawnPoint;

    [HideInInspector] public Dictionary<(bool, int), WorldEnemySet> worldEnemies = new Dictionary<(bool, int), WorldEnemySet>();
    [HideInInspector] public Dictionary<(bool, int), WorldItem[]> worldItems = new Dictionary<(bool, int), WorldItem[]>();

    [HideInInspector] public CombatManager Combat = null;
    public Camera camWorld;
    public Camera camCombat;

    #endregion

    #region [ PROPERTIES ]



    #endregion

    #region [ COROUTINES ]

    private Coroutine c_CombatTransition;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        SetRef_LevelManager(this);
        Combat = FindObjectOfType<CombatManager>();
        if (Combat == null)
            Debug.LogError("Combat manager not found!");
    }

    void Start()
    {
        DoGeneration();
        OnGenerationComplete();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    private void DoGeneration()
    {
        TileGrid = new Generation.Grid(Generator);
        Rooms = new List<LevelRoom>();
        Corridors = new List<LevelCorridor>();
        AllAreas = new List<LevelArea>();

        Generator.Generate(RandTuning.GenSettings);
        CreateMiniMap();
    }

    public void AddRoom(LevelRoom newRoom) => Rooms.Add(newRoom);

    public void AddCorridor(LevelCorridor newCorridor) => Corridors.Add(newCorridor);

    public void AddEnemy(WorldEnemy enemy)
    {
        if (AllEnemies == null)
            AllEnemies = new List<WorldEnemy>();
        AllEnemies.Add(enemy);
    }

    public void AddItem(WorldItem item)
    {
        if (AllItems == null)
            AllItems = new List<WorldItem>();
        AllItems.Add(item);
    }

    public void GetAllAreas()
    {
        AllAreas.AddRange(Rooms);
        AllAreas.AddRange(Corridors);
    }

    public void CreateMiniMap()
    {
        TileGrid.GenerateMiniTiles(Generator.miniMapTransform, Generator.MatMiniMap);
    }

    public void OnGenerationComplete()
    {
        List<EnemyData>[] categorised = GameDataStorage.Data.EnemyData.Categorise();
        foreach (LevelRoom room in Rooms)
        {
            if (room.containsEnemy && room.enemies != null)
            {
                foreach (WorldEnemy enemy in room.enemies.enemies)
                {
                    enemy.SetData(categorised[0].RandFrom());
                }
            }
        }

        spawnPoint.Trigger(GameManager.Instance.playerW);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void SetAIPause(bool pause)
    { foreach (WorldEnemy enemy in AllEnemies) enemy.SetPaused(pause); }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private bool inTransition = false;
    public void CombatTransition(bool entering, float duration = 1.0f)
    {
        if (!inTransition)
        {
            if (c_CombatTransition != null)
                StopCoroutine(c_CombatTransition);
            c_CombatTransition = StartCoroutine(ICombatTransition(entering, duration));
        }
    }

    private IEnumerator ICombatTransition(bool entering, float duration)
    {
        GameManager.SetControlState(ControlState.None);
        Cursor.lockState = CursorLockMode.Locked;

        GameManager.Instance.UI.CombatTransitionOverlay(duration);

        yield return new WaitForSeconds(duration * 0.5f);

        camWorld.enabled = true;
        camCombat.enabled = true;
        if (entering)
            camWorld.enabled = false;
        else
            camCombat.enabled = false;
        GameManager.Instance.UI.HUD.ShowHUD(entering ? ControlState.Combat : ControlState.World);

        yield return new WaitForSeconds(duration * 0.5f);

        GameManager.SetControlState(entering ? ControlState.Combat : ControlState.World);
    }

    public void INDEV_ExitCombat()
    {
        GameManager.Instance.OnCombatEnd();
    }
}
