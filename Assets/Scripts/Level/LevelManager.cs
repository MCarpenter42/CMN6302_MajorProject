using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Unity;
using Unity.VisualScripting;
using static Generation;

[RequireComponent(typeof(Generation))]
public class LevelManager : Core
{
    private static GameDataStorage GameData => GameManager.Instance.GameData;
    private static bool gameplayScene => GameManager.Instance.SceneAttributes.GameplayScene;

    #region [ LEVEL GENERATION ]

    public Generation Generator { get; private set; }
    public Generation.Grid TileGrid { get; private set; }

    public List<LevelRoom> Rooms { get; private set; }
    public List<LevelCorridor> Corridors { get; private set; }
    public List<LevelArea> AllAreas { get; private set; }
    public LevelArea GetArea(LevelArea.AreaID ID)
    {
        if (ID.type == LevelArea.AreaType.Room)
        {
            foreach (LevelRoom room in Rooms)
            {
                if (ID == room.ID)
                    return room;
            }
        }
        else if (ID.type == LevelArea.AreaType.Corridor)
        {
            foreach (LevelCorridor corr in Corridors)
            {
                if (ID == corr.ID)
                    return corr;
            }
        }
        return null;
    }

    #endregion

    #region [ OBJECTS / COMPONENTS ]

    [HideInInspector] public Transform tileParent;

    [HideInInspector] public Dictionary<LevelArea.AreaID, WorldEnemySet> worldEnemies = new Dictionary<LevelArea.AreaID, WorldEnemySet>();
    public List<WorldEnemy> AllEnemies
    {
        get
        {
            List<WorldEnemy> all = new List<WorldEnemy>();
            foreach (WorldEnemySet set in worldEnemies.Values)
                all.AddRange(set.enemies);
            return all;
        }
    }
    [HideInInspector] public Dictionary<LevelArea.AreaID, List<WorldItem>> worldItems = new Dictionary<LevelArea.AreaID, List<WorldItem>>();
    public List<WorldItem> AllItems
    {
        get
        {
            List<WorldItem> all = new List<WorldItem>();
            foreach (List<WorldItem> set in worldItems.Values)
                all.AddRange(set);
            return all;
        }
    }
    public void RemoveItem(WorldItem item)
    {
        LevelArea.AreaID areaID = item.areaID;
        if (worldItems.ContainsKey(areaID))
            worldItems[areaID].Remove(item);
        Player.ClearInteractions();
        Destroy(item.gameObject);
        Player.FindInteractions();
    }

    [HideInInspector] public CombatManager Combat = null;

    public WorldPlayer Player;
    public Camera camWorld;
    public PivotArmCamera camCombat;

    #endregion

    #region [ PROPERTIES ]

    [HideInInspector] public StageStart spawnPoint;
    [HideInInspector] public StageEnd endPoint;

    [HideInInspector] public int currentStage = 0;
    [HideInInspector] public bool inRestStage = false;
    public LevelArea.AreaID currentCombatRoom = LevelArea.AreaID.Null;

    private MapGenSaveData currentMapData;
    private LevelPopSaveData currentPopData;

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_CombatTransition;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected override void Initialise()
    {
        if (gameplayScene)
        {
            Generator = GetComponent<Generation>();
            Combat = FindObjectOfType<CombatManager>();
            if (Combat == null)
                Debug.LogError("Combat manager not found!");
            if (GameDataStorage.MetaData.ActiveRun)
            {
                Debug.Log("Loading existing run data");
                SaveDataWrapper saveData = GameDataStorage.LoadPlayData();
                LoadStage(saveData);
            }
            else
            {
                Debug.Log("Creating new run data");
                GameData.NewRunData();
                NewStage(false);
                Debug.Log(GameData.runData.p_healthValues[0][0] + "/" + GameData.runData.p_healthValues[0][1]);
            }
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void NewStage(bool restStage = false)
    {
        stageReady = false;
        inRestStage = restStage;
        if (!restStage)
            currentStage++;
        NewGeneration(restStage);
        StartCoroutine(IStageReadyWait(true));
    }

    public void LoadStage(SaveDataWrapper loadFrom)
    {
        stageReady = false;
        MapGenSaveData mapData = loadFrom.mapGenData;
        LevelPopSaveData popData = loadFrom.lvlPopData;
        int stage = loadFrom.runData == null ? 1 : loadFrom.runData.s_stagesCleared + 1;
        LoadGeneration(stage, mapData, popData);
        StartCoroutine(IStageReadyWait(false));
    }

    public void RestStage()
    {

    }

    private bool stageReady;
    private IEnumerator IStageReadyWait(bool isNewStage)
    {
        yield return new WaitUntil(() => stageReady);
        yield return null;
        OnStageReady(isNewStage);
    }

    public void OnStageReady(bool isNewStage)
    {
        if (isNewStage)
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
        }
        Player.FindInteractions();
        spawnPoint.Trigger(Player, 2.5f);
        UIManager.HUD.UpdateWorldHealthBars();
        GameManager.Instance.LoadingScreen.AlphaFade(1f, 0f, 1.5f, true);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void OnStageComplete()
    {
        Debug.Log("Completed stage " + currentStage + "!");
        StartCoroutine(IOnStageComplete(1.5f));
        GameManager.Instance.LoadingScreen.AlphaFade(0f, 1f, 1.5f, true);
    }
    private IEnumerator IOnStageComplete(float delay)
    {
        yield return new WaitForSeconds(delay);
        TileGrid.Clear(true);
        bool restStage = currentStage % 3 == 0 && !inRestStage;
        if (!restStage)
            GameData.runData.StageCompletionChanges();
        GameData.SaveRunData();
        NewStage(restStage);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void OnRunPaused()
    {
        GameDataStorage.SavePlayData(new MapGenSaveData(currentStage, TileGrid, Rooms, Corridors));
        GameDataStorage.SavePlayData(new LevelPopSaveData(spawnPoint.transform.position, endPoint.transform.position, worldEnemies.Values.ToArray(), AllItems));
        GameData.SaveRunData();
    }

    public void OnRunEnded(RunStatus runStatus)
    {
        GameData.runData.s_runStatus = runStatus;
        GameData.ArchiveRun();
        GameManager.LastRunCode = RandTuning.SettingsString;
        UIManager.ToScene_MainMenu();
    }
    public void OnRunWon() => OnRunEnded(RunStatus.Successful);
    public void OnRunLost() => OnRunEnded(RunStatus.Failed);
    public void OnRunAbandoned() => OnRunEnded(RunStatus.Abandoned);

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public Vector3 RandPos(int index)
    {
        if (AllAreas.InBounds(index))
            return AllAreas[index].RandInternalPosition();
        return Vector3.zero;
    }
    public Vector3 RandPos(LevelArea.AreaID ID)
    {
        if (ID.type == LevelArea.AreaType.Room)
        {
            if (Rooms.InBounds(ID.value))
                return Rooms[ID.value].RandInternalPosition();
        }
        else if (ID.type == LevelArea.AreaType.Corridor)
        {
            if (Corridors.InBounds(ID.value))
                return Corridors[ID.value].RandInternalPosition();
        }
        return Vector3.zero;
    }
    public Vector3 RandPos(LevelArea.AreaType type, int index)
    {
        if (type == LevelArea.AreaType.Room)
        {
            if (Rooms.InBounds(index))
                return Rooms[index].RandInternalPosition();
        }
        else if (type == LevelArea.AreaType.Corridor)
        {
            if (Corridors.InBounds(index))
                return Corridors[index].RandInternalPosition();
        }
        return Vector3.zero;
    }

    public void ClearMap()
    {
        AllAreas.Clear();
        Rooms.ClearAndDestroy();
        Corridors.ClearAndDestroy();
        TileGrid.Clear();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private static int iterations => RandTuning.GenSettings.iterations;
    private static RoomStructureVariance roomVariance => RandTuning.GenSettings.roomStructure;
    private static CorridorStructureVariance corrVariance => RandTuning.GenSettings.corridorStructure;
    private static bool connectToExisting => RandTuning.GenSettings.connectToExisting;
    private static FloatRange enemyDensityRange => RandTuning.GenSettings.enemyDensity;
    private static FloatRange itemDensityRange => RandTuning.GenSettings.itemDensity;

    public void NewGeneration(bool restStage) => StartCoroutine(INewGeneration(restStage));
    private IEnumerator INewGeneration(bool restStage)
    {
        if (worldEnemies != null)
        {
            foreach (WorldEnemySet set in worldEnemies.Values)
            {
                set.Reset();
            }
            worldEnemies.Clear();
        }
        if (worldItems != null)
        {
            foreach (List<WorldItem> set in worldItems.Values)
            {
                set.ClearAndDestroy();
            }
            worldItems.Clear();
        }
        Player.ClearInteractions();
        Player.transform.position = -5000f * Vector3.forward;
        if (TileGrid == null)
            TileGrid = new Generation.Grid(Generator);
        else
            TileGrid.Clear();
        yield return null;

        Generator.mapTransform.DestroyAllChildren();
        worldEnemies = null;
        worldItems = null;
        yield return null;

        if (AllAreas == null)
            AllAreas = new List<LevelArea>();
        else
            AllAreas.Clear();
        if (restStage)
            (Rooms, Corridors) = Generator.GenerateRestMap();
        else
            (Rooms, Corridors) = Generator.GenerateMap(iterations, roomVariance, corrVariance, connectToExisting);
        AllAreas.AddRange(Rooms);
        AllAreas.AddRange(Corridors);
        AllAreas.ReParentTiles();
        yield return null;

        Generation.LevelPopulationData popData;
        if (restStage)
            popData = Generator.PopulateRestLevel();
        else
            popData = Generator.PopulateLevel(enemyDensityRange, itemDensityRange);
        worldEnemies = popData.enemySets;
        worldItems = popData.items;
        yield return null;

        List<WorldEnemy> allEnemies = AllEnemies;
        int[] enemyInds = EnemyData.ClassDataInds(EnemyClass.Standard, allEnemies.Count);
        for (int i = 0; i < allEnemies.Count; i++)
        {
            allEnemies[i].dataIndex = enemyInds[i];
        }
        yield return null;

        if (spawnPoint == null)
            spawnPoint = Instantiate(Generator.spawnPoint, Generator.objectTransform);
        spawnPoint.gameObject.name = "Player Spawn";
        spawnPoint.transform.position = popData.spawnPosition;

        if (endPoint == null)
            endPoint = Instantiate(Generator.endTarget, Generator.objectTransform);
        endPoint.gameObject.name = "Stage End";
        endPoint.transform.position = popData.endPosition;
        yield return null;

        AllWallMeshes();
        yield return null;

        if (Rooms.Count > 0)
            FloorMeshes(Rooms);
        if (Corridors.Count > 0)
            FloorMeshes(Corridors);
        yield return null;

        CreateMiniMap();
        yield return null;

        if (!inRestStage)
        {
            int cStandard = 0, cElite = 0, cBoss = 0;
            foreach (WorldEnemy enemy in AllEnemies)
            {
                if (enemy.Class == EnemyClass.Standard)
                    cStandard++;
                else if (enemy.Class == EnemyClass.Elite)
                    cElite++;
                else if (enemy.Class == EnemyClass.Boss)
                    cBoss++;
            }
            GameData.runData.e_spawnedInStage.Set(cStandard, cElite, cBoss);

            currentMapData = new MapGenSaveData(currentStage, TileGrid, Rooms, Corridors);
            currentPopData = new LevelPopSaveData(spawnPoint.transform.position, endPoint.transform.position, worldEnemies.Values.GetSaveData(), AllItems.GetSaveData());

            GameDataStorage.SavePlayData(new MapGenSaveData(currentStage, TileGrid, Rooms, Corridors));
            GameDataStorage.SavePlayData(new LevelPopSaveData(spawnPoint.transform.position, endPoint.transform.position, worldEnemies.Values.ToArray(), AllItems));
            GameData.SaveRunData();

            GameDataStorage.MetaData.RandState = Random.state;
        }
        stageReady = true;
    }

    public void LoadGeneration(int stageInd, MapGenSaveData mapGenData, LevelPopSaveData lvlPopData) => StartCoroutine(ILoadGeneration(stageInd, mapGenData, lvlPopData));
    private IEnumerator ILoadGeneration(int stageInd, MapGenSaveData mapGenData, LevelPopSaveData lvlPopData)
    {
        if (worldEnemies != null)
        {
            foreach (WorldEnemySet set in worldEnemies.Values)
            {
                set.Reset();
            }
            worldEnemies.Clear();
        }
        if (worldItems != null)
        {
            foreach (List<WorldItem> set in worldItems.Values)
            {
                set.ClearAndDestroy();
            }
            worldItems.Clear();
        }
        Player.ClearInteractions();
        Player.transform.position = -5000f * Vector3.forward;
        yield return null;

        if (TileGrid == null)
            TileGrid = new Generation.Grid(Generator);
        else
            TileGrid.Clear();
        if (AllAreas == null)
            AllAreas = new List<LevelArea>();
        else
            AllAreas.Clear();
        yield return null;

        (Rooms, Corridors) = Generator.ReGenerateMap(mapGenData);
        AllAreas.AddRange(Rooms);
        AllAreas.AddRange(Corridors);
        yield return null;

        Generation.LevelPopulationData repopData = Generator.RePopulateLevel(lvlPopData);
        worldEnemies = repopData.enemySets;
        worldItems = repopData.items;
        yield return null;

        if (spawnPoint == null)
        {
            spawnPoint = Instantiate(Generator.spawnPoint, Generator.objectTransform);
            spawnPoint.gameObject.name = "Player Spawn";
        }
        spawnPoint.transform.position = repopData.spawnPosition;

        if (endPoint == null)
        {
            endPoint = Instantiate(Generator.endTarget, Generator.objectTransform);
            endPoint.gameObject.name = "Stage End";
        }
        endPoint.transform.position = repopData.endPosition;
        yield return null;

        WallMeshes(Rooms);
        WallMeshes(Corridors);
        FloorMeshes(Rooms);
        FloorMeshes(Corridors);
        yield return null;

        AllAreas.ReParentTiles();
        yield return null;

        CreateMiniMap();
        yield return null;

        currentStage = stageInd;
        currentMapData = mapGenData;
        currentPopData = lvlPopData;
        stageReady = true;
    }

    public void RestGeneration() => StartCoroutine(IRestGeneration());
    private IEnumerator IRestGeneration()
    {
        if (TileGrid == null)
            TileGrid = new Generation.Grid(Generator);
        else
            TileGrid.Clear();
        worldEnemies = null;
        worldItems = null;
        yield return null;

        if (AllAreas == null)
            AllAreas = new List<LevelArea>();
        else
            AllAreas.Clear();
        (Rooms, Corridors) = Generator.GenerateMap(iterations, roomVariance, corrVariance, connectToExisting);
        AllAreas.AddRange(Rooms);
        AllAreas.AddRange(Corridors);
        AllAreas.ReParentTiles();
        yield return null;

        Generation.LevelPopulationData popData = Generator.PopulateLevel(enemyDensityRange, itemDensityRange);
        worldEnemies = popData.enemySets;
        worldItems = popData.items;
        yield return null;

        spawnPoint = Instantiate(Generator.spawnPoint, Generator.objectTransform);
        spawnPoint.gameObject.name = "Player Spawn";
        spawnPoint.transform.position = popData.spawnPosition;

        endPoint = Instantiate(Generator.endTarget, Generator.objectTransform);
        endPoint.gameObject.name = "Stage End";
        endPoint.transform.position = popData.endPosition;
        yield return null;

        AllWallMeshes();
        yield return null;

        FloorMeshes(Rooms);
        FloorMeshes(Corridors);
        yield return null;

        CreateMiniMap();
        yield return null;

        int cStandard = 0, cElite = 0, cBoss = 0;
        foreach (WorldEnemy enemy in AllEnemies)
        {
            if (enemy.Class == EnemyClass.Standard)
                cStandard++;
            else if (enemy.Class == EnemyClass.Elite)
                cElite++;
            else if (enemy.Class == EnemyClass.Boss)
                cBoss++;
        }
        GameData.runData.e_spawnedInStage.Set(cStandard, cElite, cBoss);

        stageReady = true;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void AllWallMeshes()
    {
        foreach (LevelTile tile in TileGrid.GetTiles())
        {
            if (tile.type == LevelTile.TileType.Room)
            {
                (tile.mesh, tile.colliderMesh) = Generator.RoomTileMesh(tile.connections);
                tile.material = Generator.MatWallRoom;
            }
            else if (tile.type == LevelTile.TileType.Corridor)
            {
                (tile.mesh, tile.colliderMesh) = Generator.CorridorTileMesh(tile.connections);
                tile.material = Generator.MatWallCorridor;
            }
        }
    }

    public void WallMeshes<T>(ICollection<T> areas) where T : LevelArea
    {
        Mesh mesh, cMesh;
        Material mat;

        if (typeof(T) == typeof(LevelRoom))
        {
            mat = Generator.MatWallRoom;
            foreach (LevelArea room in areas)
            {
                foreach (LevelTile tile in room.tiles)
                {
                    (mesh, cMesh) = Generator.RoomTileMesh(tile.connections);
                    if (mesh != null)
                        tile.SetVisuals(mesh, mat);
                    if (cMesh != null)
                        tile.SetCollider(cMesh);
                }
            }
        }
        if (typeof(T) == typeof(LevelCorridor))
        {
            //Debug.Log("Creating corridor meshes (" + areas.Count + " corridors)");
            mat = Generator.MatWallCorridor;
            foreach (LevelArea corr in areas)
            {
                //Debug.Log(corr.ID.value + ": " + corr.tilePositions.Count + " / " + corr.tiles.Count);
                foreach (LevelTile tile in corr.tiles)
                {
                    (mesh, cMesh) = Generator.CorridorTileMesh(tile.connections);
                    if (mesh != null)
                        tile.SetVisuals(mesh, mat);
                    if (cMesh != null)
                        tile.SetCollider(cMesh);
                }
            }
        }
    }

    public void FloorMeshes<T>(ICollection<T> areas) where T : LevelArea
    {
        foreach (LevelArea area in areas)
        {
            area.gameObject.GetOrAddComponent<MeshFilter>().sharedMesh = Generator.FloorMesh(area);
            area.gameObject.GetOrAddComponent<MeshRenderer>().sharedMaterial = Generator.MatFloor;
        }
    }

    public void CreateMiniMap()
    {
        Generator.miniMapTransform.DestroyAllChildren();
        TileGrid.GenerateMiniTiles(Generator.miniMapTransform, Generator.MatMiniMap);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void SetAIPause(bool pause) { foreach (WorldEnemy enemy in AllEnemies) enemy.SetPaused(pause); }

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
        inTransition = true;

        GameManager.SetControlState(ControlState.None);
        Cursor.lockState = CursorLockMode.Locked;

        GameManager.Instance.UI.CombatTransitionOverlay(duration);

        yield return new WaitForSeconds(duration * 0.5f);
        UIManager.HUD.UpdateWorldHealthBars();
        UIManager.HUD.ClearActionOrder();

        camWorld.enabled = true;
        camCombat.cam.enabled = true;
        if (entering)
            camWorld.enabled = false;
        else
            camCombat.cam.enabled = false;
        GameManager.Instance.UI.HUD.ShowHUD(entering ? ControlState.Combat : ControlState.World);

        yield return new WaitForSeconds(duration * 0.5f);

        GameManager.SetControlState(entering ? ControlState.Combat : ControlState.World);

        inTransition = false;
    }

    public void INDEV_ExitCombat()
    {
        GameManager.Instance.OnCombatEnd();
    }

    public void ClearCurrentCombatRoom(bool enemiesDefeated = true)
    {
        if (currentCombatRoom != null && worldEnemies.ContainsKey(currentCombatRoom))
        {
            if (enemiesDefeated)
                worldEnemies[currentCombatRoom].OnDefeated();
            else
                worldEnemies[currentCombatRoom].Reset();
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SaveData()
    {
        if (!inRestStage)
        {
            currentMapData = new MapGenSaveData(currentStage, TileGrid, Rooms, Corridors);
            currentPopData = new LevelPopSaveData(spawnPoint.transform.position, endPoint.transform.position, worldEnemies.Values.GetSaveData(), AllItems.GetSaveData());

            GameDataStorage.SavePlayData(new MapGenSaveData(currentStage, TileGrid, Rooms, Corridors));
            GameDataStorage.SavePlayData(new LevelPopSaveData(spawnPoint.transform.position, endPoint.transform.position, worldEnemies.Values.ToArray(), AllItems));

            GameDataStorage.MetaData.RandState = Random.state;
        }
    }

    /*public void Respawn()
    {
        player.transform.position = GetArea(spawnArea).ClosestTilePosition(player.transform.position);
    }*/
}
