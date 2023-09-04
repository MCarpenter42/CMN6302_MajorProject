using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Maths;
using NeoCambion.Random.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using TMPro;

public interface LevelArea : IEnumerable
{
    public GameObject gameObject { get; }

    public List<Vec2Int> tilePositions { get; }
    public List<LevelTile> tiles { get; }
    public Vec2Int startTile { get { return tilePositions.HasContents() ? tilePositions[0] : Vec2Int.zero; } }

    public bool containsEnemy { get; set; }
    public int enemyCount { get; }
    public WorldEnemySet enemies { get; }
    public int itemCount { get; }
    public int itemCapacity { get; }
    public WorldItem[] items { get; }

    public bool Contains(Vec2Int position);

    public void UpdatePositions();

    public Vector3 RandInternalPosition();
    public bool IncrementItemCount();
    public (PositionsInArea, PositionsInArea) SpawnPositions(int indexOverride);
    public void OverwriteEnemies(int enemyCount);
    public void OverwriteItems(int itemCount);
}

public struct PositionInArea
{
    public bool inRoom;
    public int areaIndex;
    public Vector3 worldPosition;

    public PositionInArea(bool inRoom, int areaIndex, Vector3 worldPosition)
    {
        this.inRoom = inRoom;
        this.areaIndex = areaIndex;
        this.worldPosition = worldPosition;
    }

    public PositionInArea Room(int roomIndex, Vector3 worldPosition) => new PositionInArea(true, roomIndex, worldPosition);
    public PositionInArea Corridor(int corridorIndex, Vector3 worldPosition) => new PositionInArea(false, corridorIndex, worldPosition);

    public PositionInArea Offset(Vector3 offset) => new PositionInArea(inRoom, areaIndex, worldPosition + offset);
}

public struct PositionsInArea
{
    public bool inRoom;
    public int areaIndex;
    public Vector3[] worldPositions;

    public Vector3 this[int index]
    {
        get { return worldPositions[index]; }
        set { worldPositions[index] = value; }
    }
    public int Length => worldPositions.Length;

    public PositionsInArea(bool inRoom, int areaIndex, int posCount)
    {
        this.inRoom = inRoom;
        this.areaIndex = areaIndex;
        this.worldPositions = new Vector3[posCount];
    }
    public PositionsInArea(bool inRoom, int areaIndex, Vector3[] worldPositions)
    {
        this.inRoom = inRoom;
        this.areaIndex = areaIndex;
        this.worldPositions = worldPositions;
    }

    public PositionsInArea Room(int roomIndex, Vector3[] worldPositions) => new PositionsInArea(true, roomIndex, worldPositions);
    public PositionsInArea Corridor(int corridorIndex, Vector3[] worldPositions) => new PositionsInArea(false, corridorIndex, worldPositions);

    public PositionInArea Single(int index) => new PositionInArea(inRoom, areaIndex, worldPositions[index]);
}

public struct AdjacentRef
{
    public HexGridDirection direction;
    public Vec2Int position;
    public LevelTile.TileType tileType;
    public bool isNull => direction == HexGridDirection.INVALID && position == Vec2Int.zero && tileType == LevelTile.TileType.None;

    public AdjacentRef(HexGridDirection direction, Vec2Int position, LevelTile.TileType tileType)
    {
        this.direction = direction;
        this.position = position;
        this.tileType = tileType;
    }

    public static AdjacentRef Null => new AdjacentRef(HexGridDirection.INVALID, Vec2Int.zero, LevelTile.TileType.None);
}

public interface AreaSourceRef
{
    public LevelArea source { get; }
    public int indexInSource { get; set; }
    public bool isNull { get; }
}

public class Generation : Core
{
    public class Grid
    {
        public class GridRow
        {
            public const int Min = -1073741824;
            public const int Max = 1073741823;

            public Grid grid { get; private set; }
            public Generation generator
            {
                get
                {
                    if (grid == null)
                        return null;
                    return grid.generator;
                }
            }
            private Dictionary<int, LevelTile> tiles;
            public int y;

            public GridRow(Grid grid, int y)
            {
                this.grid = grid;
                tiles = new Dictionary<int, LevelTile>();
                this.y = y;
            }

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            public bool InBounds(int x) => x >= Min && x <= Max;

            public bool Contains(int x) => Get(x) != null;

            public LevelTile Get(int x) => InBounds(x) ? tiles.GetOrRemove(x) : null;

            public LevelTile Create(int x, LevelTile.TileType type)
            {
                LevelTile tile = null;
                if (InBounds(x))
                {
                    Vec2Int position = new Vec2Int(x, y);
                    if (tiles.ContainsKey(x))
                    {
                        if (tiles[x] != null)
                            tiles[x].gameObject.DestroyThis();
                        tiles.Remove(x);
                    }
                    tile = tiles.AddCloneValue(x, generator.tileTemplate, null).Value;
                    tile.SetAttributes(position, generator.TilePosition(position), type);
                }
                return tile;
            }

            public LevelTile GetOrCreate(int x, LevelTile.TileType type)
            {
                LevelTile tile = null;
                if (InBounds(x))
                {
                    tile = Get(x);
                    if (tile == null)
                    {
                        Vec2Int position = new Vec2Int(x, y);
                        tile = tiles.AddCloneValue(x, generator.tileTemplate, grid.generator.transform).Value;
                        tile.SetAttributes(position, generator.TilePosition(position), type);
                    }
                }
                return tile;
            }

            public bool Add(int x, LevelTile tile)
            {
                if (InBounds(x) && !tiles.ContainsKey(x) && tile != null)
                    tiles.Add(x, tile);
                return false;
            }

            public bool Remove(int x)
            {
                if (InBounds(x))
                    return tiles.Remove(x);
                return false;
            }

            public LevelTile this[int x] { get { return Get(x); } set { Add(x, value); } }

            public void Clear(bool destroyTiles = false)
            {
                if (destroyTiles)
                {
                    foreach (int x in tiles.Keys)
                    {
                        if (tiles[x] != null)
                            tiles[x].gameObject.DestroyThis();
                        tiles.Remove(x);
                    }
                }
                else
                {
                    tiles.Clear();
                }
            }

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            public void GenerateMeshes(bool[] debug = null)
            {
                Vec2Int pos = new Vec2Int();
                LevelTile tile;
                LevelTile.HexConnections connections;
                LevelTile.TileType type;
                Mesh mesh, cMesh;
                Material mat;
                foreach (KeyValuePair<int, LevelTile> kvp in tiles)
                {
                    pos.x = kvp.Key;
                    pos.y = y;
                    tile = kvp.Value;
                    connections = tile.connections;
                    type = tile.type;

                    switch (kvp.Value.type)
                    {
                        default:
                            mesh = null;
                            cMesh = null;
                            mat = null;
                            break;

                        case LevelTile.TileType.Corridor:
                            (mesh, cMesh) = generator.CorridorTileMesh(connections, debug);
                            mat = generator.MatWallCorridor;
                            break;

                        case LevelTile.TileType.Room:
                            (mesh, cMesh) = generator.RoomTileMesh(connections, debug);
                            mat = generator.MatWallRoom;
                            break;
                    }

                    if (debug[0] && !(debug[3] && debug[5]))
                    {
                        if (mesh == null)
                            Debug.Log("Mesh for " + type + " tile at " + pos.ToString() + " is null");
                        else
                            Debug.Log("Mesh for " + type + " tile at " + pos.ToString() + " has " + mesh.vertexCount + " vertices");
                    }

                    if (mesh != null)
                        tile.SetVisuals(mesh, mat);

                    if (cMesh != null)
                        tile.SetCollider(cMesh);
                }
            }

            public void GenerateMiniTiles(Transform parentTransform, Material miniTileMat)
            {
                foreach (LevelTile tile in tiles.Values)
                {
                    tile.SpawnMiniTile(parentTransform, miniTileMat);
                }
            }
        }

        public const int Min = -1073741824;
        public const int Max = 1073741823;

        private Dictionary<int, GridRow> rows;
        public Generation generator;

        public Grid(Generation generator)
        {
            this.generator = generator;
            rows = new Dictionary<int, GridRow>();
        }

        public GridRow Row(int y)
        {
            GridRow rowParent = null;
            if (InBounds(y))
            {
                if (rows.ContainsKey(y))
                    rowParent = rows[y];
                else
                    rows.Add(y, null);

                if (rowParent == null)
                {
                    rowParent = new GridRow(this, y);
                    rows[y] = rowParent;
                }
            }
            return rowParent;
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public bool InBounds(int i) => i >= Min && i <= Max;
        public bool InBounds(int x, int y) => InBounds(x) && InBounds(y);
        public bool InBounds(Vec2Int position) => InBounds(position.x) && InBounds(position.y);

        public LevelTile Get(int x, int y)
        {
            if (InBounds(x, y))
                return Row(y).Get(x);
            return null;
        }
        public LevelTile Get(Vec2Int position)
        {
            if (InBounds(position))
                return Row(position.y).Get(position.x);
            return null;
        }

        public LevelTile Create(int x, int y, LevelTile.TileType type)
        {
            if (InBounds(x, y))
                return Row(y).Create(x, type);
            return null;
        }
        public LevelTile Create(Vec2Int position, LevelTile.TileType type)
        {
            if (InBounds(position.x, position.y))
                return Row(position.y).Create(position.x, type);
            return null;
        }

        public LevelTile GetOrCreate(int x, int y, LevelTile.TileType type)
        {
            if (InBounds(x, y))
                return Row(y).GetOrCreate(x, type);
            return null;
        }
        public LevelTile GetOrCreate(Vec2Int position, LevelTile.TileType type)
        {
            if (InBounds(position))
                return Row(position.y).GetOrCreate(position.x, type);
            return null;
        }

        public bool Add(int x, int y, LevelTile tile)
        {
            if (InBounds(x, y))
            {
                return Row(y).Add(x, tile);
            }
            return false;
        }
        public bool Add(Vec2Int position, LevelTile tile)
        {
            if (InBounds(position))
            {
                return Row(position.y).Add(position.x, tile);
            }
            return false;
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public void Clear(bool destroyTiles = true)
        {
            foreach (KeyValuePair<int, GridRow> kvp in rows)
            {
                if (kvp.Value != null)
                    kvp.Value.Clear(destroyTiles);
                rows.Remove(kvp.Key);
            }
        }

        public bool Contains(int x, int y)
        {
            if (rows.ContainsKey(y))
                return rows[y].Contains(x);
            return false;
        }
        public bool Contains(Vec2Int position)
        {
            if (rows.ContainsKey(position.y))
                return rows[position.y].Contains(position.x);
            return false;
        }

        public bool EmptyAt(Vec2Int position)
        {
            LevelTile tile = Get(position);
            return tile == null || tile.emptySpace;
        }

        public LevelTile this[int x, int y] { get { return Row(y).Get(x); } set { Row(y).Add(x, value); } }
        public LevelTile this[Vec2Int position] { get { return Row(position.y).Get(position.x); } set { Row(position.y).Add(position.x, value); } }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public void GenerateMeshes(bool[] debug = null)
        {
            foreach (GridRow row in rows.Values)
                row?.GenerateMeshes(debug);
        }

        public void GenerateMiniTiles(Transform parentTransform, Material miniTileMat)
        {
            foreach (GridRow row in rows.Values)
            {
                row.GenerateMiniTiles(parentTransform, miniTileMat);
            }
        }
    }

    #region [ OBJECTS / COMPONENTS ]

    public Grid tiles { get { return LevelManager.TileGrid; } }

    public List<LevelRoom> rooms { get { return LevelManager.Rooms; } }
    public List<LevelCorridor> corridors { get { return LevelManager.Corridors; } }
    public List<LevelArea> allAreas { get { return LevelManager.AllAreas; } }

    public int spawnRoomInd { get; private set; }
    public LevelRoom spawnRoom { get { return rooms.InBounds(spawnRoomInd) ? rooms[spawnRoomInd] : null; } }
    public int endRoomInd { get; private set; }
    public LevelRoom endRoom { get { return rooms.InBounds(endRoomInd) ? rooms[endRoomInd] : null; } }


    public Transform mapTransform;
    public Transform entityTransform;
    public Transform objectTransform;
    public Transform miniMapTransform;

    public LevelTile tileTemplate;
    public PlayerSpawn spawnPoint;
    public GameObject endTarget;
    public GameObject worldEnemy;
    public GameObject enemyPathAnchor;
    public GameObject worldItem;


    public GameObject corridorConn060;
    public GameObject corridorConn120;
    public GameObject corridorConn180;
    public GameObject corridorConn240;
    public GameObject corridorConn300;
    public GameObject corridorConn360;

    public GameObject roomConnectorL;
    public GameObject roomConnectorR;
    public GameObject roomDoorway;
    public GameObject roomWall;
    public GameObject roomWallCorner;

    public GameObject floorInner;
    public GameObject floorEdge;
    public GameObject floorLeftI2E;
    public GameObject floorRightI2E;
    public GameObject floorBothI2E;
    

    public GameObject coll_corridorConn060;
    public GameObject coll_corridorConn120;
    public GameObject coll_corridorConn180;
    public GameObject coll_corridorConn240;
    public GameObject coll_corridorConn300;
    public GameObject coll_corridorConn360;

    public GameObject coll_roomConnectorL;
    public GameObject coll_roomConnectorR;
    public GameObject coll_roomDoorway;
    public GameObject coll_roomWall;


    public Material matWallRoom;
    public Material MatWallRoom { get { return matWallRoom ?? UnityExt_Material.DefaultDiffuse; } }
    public Material matWallCorridor;
    public Material MatWallCorridor { get { return matWallCorridor ?? UnityExt_Material.DefaultDiffuse; } }
    public Material matDoor;
    public Material MatDoor { get { return matDoor ?? UnityExt_Material.DefaultDiffuse; } }
    public Material matFloor;
    public Material MatFloor { get { return matFloor ?? UnityExt_Material.DefaultDiffuse; } }
    public Material matMiniMap;
    public Material MatMiniMap { get { return matMiniMap ?? UnityExt_Material.DefaultDiffuse; } }

    #endregion

    #region [ PROPERTIES ]

    public static LevelTile.TileType tNone = LevelTile.TileType.None;
    public static LevelTile.TileType tEmpty = LevelTile.TileType.Empty;
    public static LevelTile.TileType tCorridor = LevelTile.TileType.Corridor;
    public static LevelTile.TileType tRoom = LevelTile.TileType.Room;

    [Header("Other Settings")]
    public float tileRadius = 6.0f;
    public float tileInnerRadius = 5.0f;
    public float miniTileRadius { get { return tileRadius * 0.1f; } }
    public bool switchRadiusAxis = false;
    public bool radiusToCorner = true;
    private float offset { get { return radiusToCorner ? Mathf.Cos(60.0f.ToRad()) : 2.0f * Mathf.Tan(30.0f.ToRad()); } }
    public float xOffset { get { return switchRadiusAxis ? offset * tileRadius : tileRadius; } }
    public float zOffset { get { return switchRadiusAxis ? tileRadius : offset * tileRadius; } }
    public float meshRotation = 180.0f;
    [Range(0.0f, 100.0f)]
    public float mergeRooms = 25.0f;
    public float wallHeight = 1.0f;

    public UVec2Int test;

    #endregion

    #region [ COROUTINES ]



    #endregion

    #region < Debug Toggles >

    [Header("Debugging - Rooms")]
    [Tooltip("")] /*0*/
    public bool _TileUpdateFail = false;
    [Tooltip("")] /*1*/
    public bool _NoAvPositions = false;
    [Tooltip("")] /*2*/
    public bool _InternalConns = false;
    [Tooltip("")] /*3*/
    public bool _ConnectCount = false;
    [Tooltip("")] /*4*/
    public bool _CorridorStarts = false;
    private bool[] roomDebug {
        get { return new bool[] {
            _TileUpdateFail, // Tile update failure
            _NoAvPositions,  // No available positions
            _InternalConns,  // Internal connections
            _ConnectCount,   // Connection count
            _CorridorStarts, // Corridor start positions
        };
    } }

    [Header("Debugging - Corridors")]
    [Tooltip("")] /*0*/
    public bool _SourceTileNull = false;
    [Tooltip("")] /*1*/
    public bool _TileConnectFail = false;
    [Tooltip("")] /*5*/
    public bool _TerminateCauses = false;
    [Tooltip("")] /*2*/
    public bool _WeightedDirs = false;
    [Tooltip("")] /*4*/
    public bool _Endpoints = false;
    [Tooltip("")] /*3*/
    public bool _ConnectTileInfo = false;
    private bool[] corrDebug {
        get { return new bool[] {
            _SourceTileNull,  // Source tile null
            _TileConnectFail, // Tile connection failure
            _WeightedDirs,    // Weighted directions
            _ConnectTileInfo, // Connection tile info
            _Endpoints,       // Endpoint positions
            _TerminateCauses, // Reasons for branch termination
        };
    } }

    [Header("Debugging - Meshes")]
    [Tooltip("Vertex count in output meshes (or null)")] /*0*/
    public bool _MeshResult = false;
    [Tooltip("Number of components generated")] /*3*/
    public bool _ComponentCount = false;
    [Tooltip("Vertex count per component mesh")] /*5*/
    public bool _ComponentVerts = false;
    [Tooltip("Input info for room mesh generation")] /*1*/
    public bool _RoomInitialInfo = false;
    [Tooltip(null)] /*2*/
    public bool _RoomDirOffsets = false;
    [Tooltip("Index of selected corridor mesh option")] /*4*/
    public bool _CrdrOptionSelect = false;
    private bool[] meshDebug {
        get { return new bool[] {
            _MeshResult,
            _RoomInitialInfo,
            _RoomDirOffsets,
            _ComponentCount,
            _CrdrOptionSelect,
            _ComponentVerts,
        };
    } }

    [Header("-")]
    public bool endingDebug = false;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Generate(GenerationSettings settings)
    {
        GenerateMap(settings.iterations, settings.roomStructure, settings.corridorStructure, settings.connectToExisting);
        PopulateLevel(settings.enemyDensity, settings.itemDensity);
    }

    public void GenerateMap(int baseIterations, RoomStructureVariance roomVariance, CorridorStructureVariance corrVariance, bool connectToExisting)
    {
        tiles.Clear();
        int maxIterations = baseIterations;
        int i, j, n;
        int nRooms = 1, nCorridors = 0, iterCount = 0;

        List<RoomSourceRef> newRooms = new List<RoomSourceRef>();
        List<CorridorSourceRef> newCorridors = new List<CorridorSourceRef>();

        LevelRoom newRoom = NewRoom(roomVariance, true, 0, roomDebug);
        LevelManager.AddRoom(newRoom);
        int nConnect = newRoom.ConnectionCount(roomDebug[3]);
        newCorridors.AddRange(newRoom.GetCorridorStarts(nConnect, connectToExisting, roomDebug[4]));
        LevelCorridor newCorridor;

        int branchRestrict = 0;

        string logStr = "Generation stats by iteration:";
        for (i = 1; i < maxIterations; i++)
        {
            iterCount++;
            n = newCorridors.Count;
            logStr += "\n[" + iterCount + "] Generating " + n + " new corridors and ";
            for (j = 0; j < n; j++)
            {
                newCorridor = NewCorridor(corrVariance, newCorridors[j], nCorridors, corrDebug);
                if (newCorridor != null)
                {
                    LevelManager.AddCorridor(newCorridor);
                    newRooms.AddRange(newCorridor.GetRoomStarts());
                    foreach (Vec2Int pos in newCorridor.tilePositions)
                    {
                        if (tiles[pos] != null)
                            tiles[pos].connections.Replace();
                    }
                    nCorridors++;
                }
            }
            newCorridors.Clear();

            n = newRooms.Count;
            logStr += n + " new rooms.";
            for (j = 0; j < n; j++)
            {
                newRoom = NewRoom(roomVariance, newRooms[j], false, nRooms, roomDebug);
                if (newRoom != null)
                {
                    LevelManager.AddRoom(newRoom);
                    if (i < maxIterations - 1)
                        nConnect = newRoom.ConnectionCount(roomDebug[3]) - (i > 2 ? branchRestrict++ : 0);
                    newCorridors.AddRange(newRoom.GetCorridorStarts(nConnect, roomDebug[4]));
                    foreach (Vec2Int pos in newRoom.tilePositions)
                    {
                        if (tiles[pos] != null)
                            tiles[pos].connections.Replace();
                    }
                    nRooms++;
                }
            }
            newRooms.Clear();

            if (newCorridors.Count == 0/* || rooms.Count >= roomCutoff*/)
                break;
        }

        if (endingDebug)
            Debug.Log("Iterations: " + iterCount + " | Total rooms: " + nRooms + " | Total corridors: " + nCorridors);

        LevelManager.GetAllAreas();
        tiles.GenerateMeshes(meshDebug);
        GenerateFloorMeshes();
    }

    public void GenerateFloorMeshes()
    {
        foreach (LevelArea area in allAreas)
        {
            area.gameObject.GetOrAddComponent<MeshFilter>().sharedMesh = FloorMesh(area);
            area.gameObject.GetOrAddComponent<MeshRenderer>().sharedMaterial = matFloor;
        }
    }

    public void PopulateLevel(FloatRange enemyDensityRange, FloatRange itemDensityRange)
    {
        int i, j, iR;
        float enemyDensity = Random.Range(enemyDensityRange.lower, enemyDensityRange.upper);
        float itemDensity = Random.Range(itemDensityRange.lower, itemDensityRange.upper);

        List<int> enemyAreaInds = new List<int>(), itemAreaInds = new List<int>();
        HashSet<int> areasToPopulate = new HashSet<int>();
        enemyAreaInds.IncrementalPopulate(0, 1, rooms.Count);
        itemAreaInds.IncrementalPopulate(0, 1, rooms.Count);
        for (i = 0; i < corridors.Count; i++)
        {
            if (corridors[i].ValidItemPlacements().Length > 0)
                itemAreaInds.Add(i + rooms.Count);
        }

        iR = Random.Range(1, enemyAreaInds.Count);
        spawnRoomInd = enemyAreaInds[iR];
        rooms[spawnRoomInd].isStartRoom = true;
        PlayerSpawn spawn = Instantiate(spawnPoint, rooms[spawnRoomInd].transform);
        spawn.transform.position = rooms[spawnRoomInd].RandInternalPosition();
        Debug.Log("Player Spawn Info\nRoom Index: " + spawnRoomInd + "\nRoom grid position: " + rooms[spawnRoomInd] + "\nWorld position of spawn: " + spawn.transform.position);
        LevelManager.spawnPoint = spawn;
        enemyAreaInds.RemoveAt(iR);
        itemAreaInds.RemoveAt(iR);

        iR = Random.Range(0, enemyAreaInds.Count);
        endRoomInd = enemyAreaInds[iR];

        int enemyCount = Mathf.RoundToInt(rooms.Count * enemyDensity);
        if (enemyCount < 2)
            enemyCount = 2;
        int itemCount = Mathf.RoundToInt(rooms.Count * itemDensity);
        if (itemCount < 1)
            itemCount = 1;
        
        for (i = 0; i < enemyCount; i++)
        {
            iR = Random.Range(0, enemyAreaInds.Count);
            LevelArea area = GetArea(enemyAreaInds[iR]);
            if (!area.containsEnemy)
            {
                area.containsEnemy = true;
                areasToPopulate.Add(enemyAreaInds[iR]);
                enemyAreaInds.RemoveAt(iR);
            }
        }
        for (i = 0; i < itemCount; i++)
        {
            iR = Random.Range(0, itemAreaInds.Count);
            LevelArea area = GetArea(itemAreaInds[iR]);
            if (area.itemCount == 0)
                areasToPopulate.Add(itemAreaInds[iR]);
            if (!area.IncrementItemCount())
                itemAreaInds.RemoveAt(iR);
        }

        int indAdj;
        foreach (int index in areasToPopulate)
        {
            indAdj = index > rooms.Count ? index - rooms.Count : index;
            LevelArea area = GetArea(index);
            if (area != null)
            {
                (PositionsInArea enemyPositions, PositionsInArea itemPositions) = area.SpawnPositions(indAdj);
                if (enemyPositions.Length > 0)
                {
                    area.OverwriteEnemies(enemyPositions.Length);
                    for (i = 0; i < enemyPositions.Length; i++)
                    {
                        WorldEnemy newEnemy = Instantiate(worldEnemy, entityTransform).GetComponent<WorldEnemy>();
                        area.enemies[i] = newEnemy;
                        newEnemy.Setup(enemyPositions.Single(i), area.enemies);
                    }
                }
                if (itemPositions.Length > 0)
                {
                    area.OverwriteItems(itemPositions.Length);
                    for (i = 0; i < itemPositions.Length; i++)
                    {
                        WorldItem newItem = Instantiate(worldItem, objectTransform).GetComponent<WorldItem>();
                        (area.items[i] = newItem).transform.position = itemPositions[i];
                    }
                }
            }
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public LevelTile UpdateTile(Vec2Int position, LevelTile.TileType type, bool overwrite = false)
    {
        LevelTile tile = tiles.GetOrCreate(position, type);
        bool typeSupercedes = type != tNone && tile.type == tNone;
        if (typeSupercedes || overwrite)
            tile.SetAttributes(position, TilePosition(position), type);
        return tile;
    }
    
    public LevelTile UpdateTile(Vec2Int position, LevelTile.TileType type, LevelArea parent, bool overwrite = false)
    {
        LevelTile tile = tiles.GetOrCreate(position, type);
        bool typeSupercedes = type != tNone && tile.type == tNone;
        if (typeSupercedes || overwrite)
            tile.SetAttributes(position, TilePosition(position), type);
        if (parent.GetType() == typeof(LevelRoom))
            tile.SetTransformParent(parent as LevelRoom);
        else if (parent.GetType() == typeof(LevelCorridor))
            tile.SetTransformParent(parent as LevelCorridor);
        return tile;
    }
    
    public bool SetConnectionStates(Vec2Int position, HexGridDirection direction, LevelTile.ConnectionState state)
    {
        Vec2Int pos2 = HexGrid2D.Adjacent(position, direction);
        LevelTile tileA = tiles[position], tileB = tiles[pos2];
        if (tileA != null && tileB != null)
        {
            tileA.connections[direction] = state;
            tileB.connections[direction.Invert()] = state;
            return true;
        }
        return false;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public List<AdjacentRef> Adjacent(Vec2Int position, LevelTile.TileType tileType)
    {
        List<AdjacentRef> selAdj = new List<AdjacentRef>();
        HexGridDirection dir;
        Vec2Int pos;
        LevelTile tile;
        for (int i = 0; i < 6; i++)
        {
            dir = (HexGridDirection)i;
            pos = HexGrid2D.Adjacent(position, dir);
            tile = tiles[pos];
            if (tile == null)
            {
                if (tileType == tNone)
                    selAdj.Add(new AdjacentRef(dir, pos, tileType));
            }
            else
            {
                if (tileType == tile.type)
                    selAdj.Add(new AdjacentRef(dir, pos, tileType));
            }
        }
        return selAdj;
    }
    
    public (List<AdjacentRef>, List<AdjacentRef>, List<AdjacentRef>, List<AdjacentRef>) CategorisedAdjacent(Vec2Int position)
    {
        List<AdjacentRef> none = new List<AdjacentRef>();
        List<AdjacentRef> empty = new List<AdjacentRef>();
        List<AdjacentRef> corridor = new List<AdjacentRef>();
        List<AdjacentRef> room = new List<AdjacentRef>();
        HexGridDirection dir;
        Vec2Int pos;
        LevelTile tile;

        for (int i = 0; i < 6; i++)
        {
            dir = (HexGridDirection)i;
            pos = HexGrid2D.Adjacent(position, dir);
            tile = tiles[pos];
            LevelTile.TileType type = tile == null ? tNone : tile.type;
            switch (type)
            {
                default:
                case LevelTile.TileType.None:
                    none.Add(new AdjacentRef(dir, pos, type));
                    break;

                case LevelTile.TileType.Empty:
                    empty.Add(new AdjacentRef(dir, pos, type));
                    break;

                case LevelTile.TileType.Corridor:
                    corridor.Add(new AdjacentRef(dir, pos, type));
                    break;

                case LevelTile.TileType.Room:
                    room.Add(new AdjacentRef(dir, pos, type));
                    break;
            }
        }

        return (none, empty, corridor, room);
    }

    public Vector3 TilePosition(Vec2Int gridPosition)
    {
        Vector3 vect = Vector3.zero;
        vect.x = (float)gridPosition.x * 1.5f * xOffset;
        vect.z = ((float)gridPosition.y * 2.0f + (gridPosition.x % 2 == 0 ? 0.0f : 1.0f)) * zOffset;
        return vect;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private LevelArea GetArea(int index)
    {
        if (index < 0)
            return null;

        if (index < rooms.Count)
            return rooms[index];

        int indexAdj = index - rooms.Count;
        if (indexAdj < corridors.Count)
            return corridors[indexAdj];

        Debug.Log("Original index: " + index + " --> " + rooms.Count + " rooms | Adjusted index: " + indexAdj + " --> " + corridors.Count + " corridors");
        return null;
    }
    private LevelArea GetArea(bool room, int index) => room ? rooms[index] : corridors[index];

    private LevelRoom NewRoom(RoomStructureVariance roomVar, bool largeRoom, int ID = -1, bool[] debug = null)
    {
        return NewRoom(roomVar, RoomSourceRef.Null, largeRoom, ID, debug);
    }
    private LevelRoom NewRoom(RoomStructureVariance roomVar, RoomSourceRef source, bool largeRoom, int ID = -1, bool[] debug = null)
    {
        Vec2Int startTile;
        if (source.isNull)
            startTile = Vec2Int.zero;
        else
            startTile = source.startTile;

        LevelRoom room = null;
        bool cont = !source.isNull || tiles[startTile] == null || tiles[startTile].emptySpace;

        if (cont)
        {
            room = NewObject("Room " + ID, mapTransform, typeof(LevelRoom)).GetComponent<LevelRoom>();
            room.Initialise(this, ID);

            int sizeMin = roomVar.Min(largeRoom), sizeMax = roomVar.Max(largeRoom) + 1;
            WeightingCurve weighting = largeRoom ? WeightingCurve.None : WeightingCurve.Power;
            int tileCount = Ext_Random.RangeWeighted(sizeMin, sizeMax, weighting, false, 2.0f);

            LevelTile newTile = UpdateTile(startTile, tRoom, room, true);
            newTile.AddAdditionalInfo("Room start tile");
            room.Add(newTile);

            List<AdjacentRef> localFree, allFree = new List<AdjacentRef>();
            Vec2Int targ;
            int i, j, cTile;
            for (i = 1; i < tileCount; i++)
            {
                cTile = room.CentreTileIndex();
                for (j = 0; j < room.tilePositions.Count; j++)
                {
                    localFree = Adjacent(room.tilePositions[j], tNone);
                    if (localFree != null && localFree.Count > 0)
                    {
                        if (j == cTile)
                            allFree.MultiAddRange(localFree, largeRoom ? 5 : 2);
                        else
                            allFree.AddRange(localFree);
                    }
                }

                if (allFree.Count > 0)
                {
                    targ = allFree[Random.Range(0, allFree.Count)].position;
                    newTile = tiles.GetOrCreate(targ, tRoom);
                    if (newTile == null)
                    {
                        if (debug[0])
                            Debug.Log("Failed to update tile at " + targ.ToString() + " on iteration " + i);
                    }
                    else
                    {
                        room.Add(newTile);
                    }
                    //newTile.SetTransformParent(room);
                }
                else
                {
                    if (debug[1])
                        Debug.Log("No new tile positions available on iteration " + i + "!");
                    break;
                }
                allFree.Clear();
            }

            /*room.transform.position = room.CentrepointWorld();
            foreach (LevelTile tile in room.tiles)
            {
                tile.position = TilePosition(tile.gridPosition);
            }*/
        }

        if (room != null)
        {
            if (debug[2])
                room.DebugInternalConnections();
            else
                room.InternalConnections();
        }
        return room;
    }

    private LevelCorridor NewCorridor(CorridorStructureVariance corrVar, CorridorSourceRef source, int ID = -1, bool[] debug = null)
    {
        Vec2Int startTile = source.sourceRoom.tilePositions[source.tileInd];
        string debugInfo = "Generating new corridor from " + startTile.ToString() + ", with bearing " + source.startDirection.ToString();
        int db_totalCorridorBranches = 1;
        string db_endPositions = "End Positions[ ";

        int i, j;
        int lMin = (int)corrVar.lengthMin, lMax = (int)corrVar.lengthMax + 1;
        int maxLength = Ext_Random.RangeWeighted(lMin, lMax, WeightingCurve.Power, false, 2.0f);

        HexGridDirection initDir = source.startDirection;
        Vec2Int initPos = startTile, firstTile;
        LevelCorridor corr = NewObject("Corridor " + ID, mapTransform, typeof(LevelCorridor)).GetComponent<LevelCorridor>();
        corr.Initialise(this, source.sourceRoom, ID, startTile, initDir);

        if (debug[0] && tiles[initPos] == null)
            Debug.LogError("Corridor source tile " + initPos.ToString() + " is null!");
        firstTile = corr.mainBranch.tiles.Last();
        LevelTile newTile = UpdateTile(firstTile, tCorridor);
        corr.AddTile(newTile);
        //newTile.SetTransformParent(corr);

        if (!SetConnectionStates(initPos, initDir, LevelTile.ConnectionState.Connect) && debug[1])
        {
            string logString = "Can't connect a null tile!" + (tiles[initPos] == null ? " | From tile " + initPos.ToString() + " is null" : null) + (tiles[firstTile] == null ? " | To tile " + firstTile.ToString() + " is null" : null);
            Debug.LogError(logString);
        }

        float chance;
        HexGridDirection lastDir;
        Vec2Int lastPos;
        AdjacentRef next = AdjacentRef.Null;
        List<AdjacentRef> localNone, localCorridor, localRoom;
        bool noFreeSpace, ending, splitting, connecting;
        List<int> newBranches = new List<int>();

        for (i = 1; i < maxLength; i++)
        {
            foreach (CorridorBranch branch in corr)
            {
                if (branch == null || branch.fullTerminated)
                    break;

                lastPos = branch.tiles.Count > 0 ? branch.tiles.Last() : branch.startTile;
                lastDir = branch.steps.Count > 0 ? branch.steps.Last() : branch.startDir;
                if (branch.child != null && (branch.tiles.Count - 1) == branch.childStartsAt)
                    lastDir = lastDir.Rotate(-2);

                (localNone, _, localCorridor, localRoom) = CategorisedAdjacent(lastPos);
                ending = false;
                connecting = false;
                chance = Random.Range(0.0f, 1.0f);
                noFreeSpace = localNone.Count == 0;
                splitting = (branch.child == null) && (ending ? false : (i > 1 ? chance >= (1.0f - corr.branchChance) : false));

                if (noFreeSpace)
                {
                    ending = true;
                    if (debug[5])
                        Debug.Log("Branch terminating due to lack of available positions\nOrigin tile: " + corr.startTile + "\nBranch index: " + branch.index);
                    if (localRoom.Count > 0)
                        next = branch.GetConnectionTile(lastPos, localRoom, lastDir, true, debug[2]);
                    else if (localCorridor.Count > 0)
                        next = branch.GetConnectionTile(lastPos, localCorridor, lastDir, true, debug[2]);
                    if (!next.isNull)
                        connecting = true;
                    db_endPositions += next.position.ToString() + ", ";
                }
                else if (i == (maxLength - 1))
                {
                    ending = true;
                    if (debug[5])
                        Debug.Log("Branch terminating due to corridor reaching maximum length\nCorridor length: " + (i + 1) + "\n Branch length: " + (branch.tiles.Count + 1) + "\nOrigin tile: " + corr.startTile + "\nBranch index: " + branch.index);
                    if (localRoom.Count > 0 && chance >= 0.65f)
                        next = branch.GetConnectionTile(lastPos, localRoom, lastDir, true, debug[2]);
                    else if (localCorridor.Count > 0 && chance <= 0.35f)
                        next = branch.GetConnectionTile(lastPos, localCorridor, lastDir, true, debug[2]);

                    if (next.isNull)
                        next = branch.GetConnectionTile(lastPos, localNone, lastDir, true, debug[2]);
                    else
                        connecting = true;
                    db_endPositions += next.position.ToString() + ", ";
                }
                else
                {
                    if (localRoom.Count > 0 && chance >= 0.90f)
                    {
                        if (debug[5])
                            Debug.Log("Branch terminating due to random chance\nOrigin tile: " + corr.startTile + "\nBranch index: " + branch.index);
                        next = branch.GetConnectionTile(lastPos, localRoom, lastDir, true, debug[2]);
                        ending = true;
                        connecting = true;
                    }
                    else if (localCorridor.Count > 0 && chance <= 0.10f)
                    {
                        if (debug[5])
                            Debug.Log("Branch terminating due to random chance\nOrigin tile: " + corr.startTile + "\nBranch index: " + branch.index);
                        next = branch.GetConnectionTile(lastPos, localCorridor, lastDir, true, debug[2]);
                        ending = true;
                        connecting = true;
                    }
                    else
                        next = branch.GetConnectionTile(lastPos, localNone, lastDir, true, debug[2]);
                }

                localNone.Clear();
                localCorridor.Clear();
                localRoom.Clear();

                Vec2Int nextPos = next.position;
                HexGridDirection nextDir = next.direction;
                if (!next.isNull && branch.AddStep(nextDir))
                {
                    if (ending)
                    {
                        if (connecting)
                            branch.terminateType = CorridorBranch.Termination.ExistingTile;
                        else
                        {
                            UpdateTile(nextPos, tRoom);
                            branch.terminateType = CorridorBranch.Termination.NewRoom;
                        }
                        branch.terminated = true;
                        string debugStr = "Corridor branch terminated at " + nextPos + "\nCorridor origin: " + corr.startTile + "\nEnded with ";
                        if (debug[4])
                        {
                            switch (branch.terminateType)
                            {
                                default: Debug.Log(debugStr + "??"); break;
                                case CorridorBranch.Termination.NewRoom: Debug.Log(debugStr + "new room"); break;
                                case CorridorBranch.Termination.ExistingTile: Debug.Log(debugStr + "existing tile"); break;
                                case CorridorBranch.Termination.EndCap: Debug.Log(debugStr + "end cap"); break;
                            }
                        }
                    }
                    else
                    {
                        if (splitting)
                        {
                            newBranches.Add(branch.index);
                        }
                        newTile = UpdateTile(nextPos, tCorridor);
                        corr.AddTile(newTile);
                    }
                    SetConnectionStates(lastPos, nextDir, LevelTile.ConnectionState.Connect);
                }
                next = AdjacentRef.Null;
            }

            for (j = 0; j < newBranches.Count; j++)
            {
                corr.SplitBranch(newBranches[j]);
            }

        }

        db_endPositions = db_endPositions.Substring(0, db_endPositions.Length - 2);
        debugInfo += "\nMax length: " + maxLength + " | Total branches: " + db_totalCorridorBranches + "\n" + db_endPositions + " ]";

        if (debug[3])
            Debug.Log(debugInfo);

        return corr;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public (Mesh, Mesh) RoomTileMesh(LevelTile.HexConnections connections, bool[] debug = null)
    {
        if (debug[1])
            Debug.Log("Generating room tile mesh with the following connections:\n" + connections.ToString());

        List<GameObject> components = new List<GameObject>();
        List<GameObject> coll_components = new List<GameObject>();
        Vector3 rot;
        HexGridDirection dir;
        LevelTile.ConnectionState conn, connL, connR;
        int i, iters = 0;
        for (i = 0; i < connections.Length; ++i)
        {
            rot = new Vector3(0.0f, (float)i * 60.0f + meshRotation, 0.0f);
            dir = (HexGridDirection)i;
            conn = connections[dir];
            connL = connections[dir.Rotate(-1)];
            connR = connections[dir.Rotate(1)];
            if (debug[2])
                Debug.Log(dir.ToString() + " --> [L] " + dir.Rotate(-1).ToString() + " / [R] " + dir.Rotate(1).ToString());
            if (conn == LevelTile.ConnectionState.Merge)
            {
                if (connL != LevelTile.ConnectionState.Merge)
                {
                    components.AddClone(roomConnectorL, null).transform.eulerAngles = rot;
                    coll_components.AddClone(coll_roomConnectorL, null).transform.eulerAngles = rot;
                }
                if (connR != LevelTile.ConnectionState.Merge)
                {
                    components.AddClone(roomConnectorR, null).transform.eulerAngles = rot;
                    coll_components.AddClone(coll_roomConnectorR, null).transform.eulerAngles = rot;
                }
            }
            else
            {
                if (conn == LevelTile.ConnectionState.Connect)
                {
                    components.AddClone(roomDoorway, null).transform.eulerAngles = rot;
                    coll_components.AddClone(coll_roomDoorway, null).transform.eulerAngles = rot;
                }
                else
                {
                    components.AddClone(roomWall, null).transform.eulerAngles = rot;
                    coll_components.AddClone(coll_roomWall, null).transform.eulerAngles = rot;
                }

                if (connR != LevelTile.ConnectionState.Merge)
                    components.AddClone(roomWallCorner, null).transform.eulerAngles = rot;
            }
            iters++;
        }

        int nComponents = components.Count;
        string compVertCounts = nComponents > 0 ? "Vertex count per component:" : "Vertex count per component:\n < No components >";
        if (debug[5])
        {
            Mesh _mesh;
            for (i = 0; i < components.Count; i++)
            {
                _mesh = components[i].GetMesh();
                if (_mesh != null)
                    compVertCounts += "\n - Component " + i + ": " + _mesh.vertexCount;
            }
        }

        components.Rescale(wallHeight, Axis.Y);
        coll_components.Rescale(wallHeight, Axis.Y);
        Mesh meshOut = UnityExt_Mesh.MergeFrom(components, true);
        Mesh coll_meshOut = UnityExt_Mesh.MergeFrom(coll_components, true);

        if (debug[3])
        {
            if (debug[5])
                Debug.Log("[Room] " + nComponents + " components generated for mesh" + (debug[0] ? " with " + meshOut.vertexCount + " vertices" : null) + "\n" + compVertCounts);
            else
                Debug.Log("[Room] " + nComponents + " components generated for mesh");
        }
        else if (debug[5])
        {
            Debug.Log(compVertCounts);
        }

        return (meshOut, coll_meshOut);
    }
    public (Mesh, Mesh) RoomTileMesh(Vec2Int tilePos) => CorridorTileMesh(tiles[tilePos].connections);
    
    private static float[] r = new float[]
    {
        0.0f,
        60.0f,
        120.0f,
        180.0f,
        -120.0f,
        -60.0f
    };
    private KeyValuePair<int, float>[][] corrMeshOptions{
    get {return new KeyValuePair<int, float>[][]{
        new KeyValuePair<int, float>[]/*00*/
        { },
        new KeyValuePair<int, float>[]/*01*/
        {
            new KeyValuePair<int, float>(0, r[0]),
        },
        new KeyValuePair<int, float>[]/*02*/
        {
            new KeyValuePair<int, float>(0, r[1]),
        },
        new KeyValuePair<int, float>[]/*03*/
        {
            new KeyValuePair<int, float>(1, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
        },
        new KeyValuePair<int, float>[]/*04*/
        {
            new KeyValuePair<int, float>(0, r[2]),
        },
        new KeyValuePair<int, float>[]/*05*/
        {
            new KeyValuePair<int, float>(2, r[0]),
            new KeyValuePair<int, float>(4, r[2]),
        },
        new KeyValuePair<int, float>[]/*06*/
        {
            new KeyValuePair<int, float>(1, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
        },
        new KeyValuePair<int, float>[]/*07*/
        {
            new KeyValuePair<int, float>(2, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
        },
        new KeyValuePair<int, float>[]/*08*/
        {
            new KeyValuePair<int, float>(0, r[3]),
        },
        new KeyValuePair<int, float>[]/*09*/
        {
            new KeyValuePair<int, float>(3, r[0]),
            new KeyValuePair<int, float>(3, r[3]),
        },
        new KeyValuePair<int, float>[]/*10*/
        {
            new KeyValuePair<int, float>(2, r[1]),
            new KeyValuePair<int, float>(4, r[3]),
        },
        new KeyValuePair<int, float>[]/*11*/
        {
            new KeyValuePair<int, float>(3, r[0]),
            new KeyValuePair<int, float>(4, r[3]),
            new KeyValuePair<int, float>(5, r[1]),
        },
        new KeyValuePair<int, float>[]/*12*/
        {
            new KeyValuePair<int, float>(1, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*13*/
        {
            new KeyValuePair<int, float>(3, r[0]),
            new KeyValuePair<int, float>(4, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*14*/
        {
            new KeyValuePair<int, float>(2, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*15*/
        {
            new KeyValuePair<int, float>(3, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*16*/
        {
            new KeyValuePair<int, float>(0, r[4]),
        },
        new KeyValuePair<int, float>[]/*17*/
        {
            new KeyValuePair<int, float>(2, r[4]),
            new KeyValuePair<int, float>(4, r[0]),
        },
        new KeyValuePair<int, float>[]/*18*/
        {
            new KeyValuePair<int, float>(3, r[1]),
            new KeyValuePair<int, float>(3, r[4]),
        },
        new KeyValuePair<int, float>[]/*19*/
        {
            new KeyValuePair<int, float>(3, r[4]),
            new KeyValuePair<int, float>(4, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
        },
        new KeyValuePair<int, float>[]/*20*/
        {
            new KeyValuePair<int, float>(2, r[2]),
            new KeyValuePair<int, float>(4, r[4]),
        },
        new KeyValuePair<int, float>[]/*21*/
        {
            new KeyValuePair<int, float>(4, r[0]),
            new KeyValuePair<int, float>(4, r[2]),
            new KeyValuePair<int, float>(4, r[4]),
        },
        new KeyValuePair<int, float>[]/*22*/
        {
            new KeyValuePair<int, float>(3, r[1]),
            new KeyValuePair<int, float>(4, r[4]),
            new KeyValuePair<int, float>(5, r[2]),
        },
        new KeyValuePair<int, float>[]/*23*/
        {
            new KeyValuePair<int, float>(4, r[0]),
            new KeyValuePair<int, float>(4, r[4]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
        },
        new KeyValuePair<int, float>[]/*24*/
        {
            new KeyValuePair<int, float>(1, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*25*/
        {
            new KeyValuePair<int, float>(3, r[3]),
            new KeyValuePair<int, float>(4, r[0]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*26*/
        {
            new KeyValuePair<int, float>(3, r[1]),
            new KeyValuePair<int, float>(4, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*27*/
        {
            new KeyValuePair<int, float>(4, r[0]),
            new KeyValuePair<int, float>(4, r[3]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*28*/
        {
            new KeyValuePair<int, float>(2, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*29*/
        {
            new KeyValuePair<int, float>(4, r[0]),
            new KeyValuePair<int, float>(4, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*30*/
        {
            new KeyValuePair<int, float>(3, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*31*/
        {
            new KeyValuePair<int, float>(4, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*32*/
        {
            new KeyValuePair<int, float>(0, r[5]),
        },
        new KeyValuePair<int, float>[]/*33*/
        {
            new KeyValuePair<int, float>(1, r[5]),
            new KeyValuePair<int, float>(5, r[0]),
        },
        new KeyValuePair<int, float>[]/*34*/
        {
            new KeyValuePair<int, float>(2, r[5]),
            new KeyValuePair<int, float>(4, r[1]),
        },
        new KeyValuePair<int, float>[]/*35*/
        {
            new KeyValuePair<int, float>(2, r[5]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
        },
        new KeyValuePair<int, float>[]/*36*/
        {
            new KeyValuePair<int, float>(3, r[2]),
            new KeyValuePair<int, float>(3, r[5]),
        },
        new KeyValuePair<int, float>[]/*37*/
        {
            new KeyValuePair<int, float>(3, r[5]),
            new KeyValuePair<int, float>(4, r[2]),
            new KeyValuePair<int, float>(5, r[0]),
        },
        new KeyValuePair<int, float>[]/*38*/
        {
            new KeyValuePair<int, float>(3, r[5]),
            new KeyValuePair<int, float>(4, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
        },
        new KeyValuePair<int, float>[]/*39*/
        {
            new KeyValuePair<int, float>(3, r[5]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
        },
        new KeyValuePair<int, float>[]/*40*/
        {
            new KeyValuePair<int, float>(2, r[3]),
            new KeyValuePair<int, float>(4, r[5]),
        },
        new KeyValuePair<int, float>[]/*41*/
        {
            new KeyValuePair<int, float>(3, r[3]),
            new KeyValuePair<int, float>(4, r[5]),
            new KeyValuePair<int, float>(5, r[0]),
        },
        new KeyValuePair<int, float>[]/*42*/
        {
            new KeyValuePair<int, float>(4, r[1]),
            new KeyValuePair<int, float>(4, r[3]),
            new KeyValuePair<int, float>(4, r[5]),
        },
        new KeyValuePair<int, float>[]/*43*/
        {
            new KeyValuePair<int, float>(4, r[3]),
            new KeyValuePair<int, float>(4, r[5]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
        },
        new KeyValuePair<int, float>[]/*44*/
        {
            new KeyValuePair<int, float>(3, r[2]),
            new KeyValuePair<int, float>(4, r[5]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*45*/
        {
            new KeyValuePair<int, float>(4, r[2]),
            new KeyValuePair<int, float>(4, r[5]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*46*/
        {
            new KeyValuePair<int, float>(4, r[1]),
            new KeyValuePair<int, float>(4, r[5]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*47*/
        {
            new KeyValuePair<int, float>(4, r[5]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
        },
        new KeyValuePair<int, float>[]/*48*/
        {
            new KeyValuePair<int, float>(1, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*49*/
        {
            new KeyValuePair<int, float>(2, r[4]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*50*/
        {
            new KeyValuePair<int, float>(3, r[4]),
            new KeyValuePair<int, float>(4, r[1]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*51*/
        {
            new KeyValuePair<int, float>(3, r[4]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*52*/
        {
            new KeyValuePair<int, float>(3, r[2]),
            new KeyValuePair<int, float>(4, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*53*/
        {
            new KeyValuePair<int, float>(4, r[2]),
            new KeyValuePair<int, float>(4, r[4]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*54*/
        {
            new KeyValuePair<int, float>(4, r[1]),
            new KeyValuePair<int, float>(4, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
            new KeyValuePair<int, float>(5, r[2]),
        },
        new KeyValuePair<int, float>[]/*55*/
        {
            new KeyValuePair<int, float>(4, r[4]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[4]),
        },
        new KeyValuePair<int, float>[]/*56*/
        {
            new KeyValuePair<int, float>(2, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*57*/
        {
            new KeyValuePair<int, float>(3, r[3]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*58*/
        {
            new KeyValuePair<int, float>(4, r[1]),
            new KeyValuePair<int, float>(4, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*59*/
        {
            new KeyValuePair<int, float>(4, r[3]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*60*/
        {
            new KeyValuePair<int, float>(3, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*61*/
        {
            new KeyValuePair<int, float>(4, r[2]),
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*62*/
        {
            new KeyValuePair<int, float>(4, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
        new KeyValuePair<int, float>[]/*63*/
        {
            new KeyValuePair<int, float>(5, r[0]),
            new KeyValuePair<int, float>(5, r[1]),
            new KeyValuePair<int, float>(5, r[2]),
            new KeyValuePair<int, float>(5, r[3]),
            new KeyValuePair<int, float>(5, r[4]),
            new KeyValuePair<int, float>(5, r[5]),
        },
    };}}
    public (List<GameObject>, List<GameObject>) CorridorTileMeshComponents(LevelTile.HexConnections connections, bool debug = false)
    {
        int i, i2, factor, sel = 0;
        for (i = 0, i2 = 5; i < 6; i++, i2--)
        {
            factor = 2.Pow((ushort)i);
            sel += (connections[i] == LevelTile.ConnectionState.Connect ? 1 : 0) * factor;
        }
        if (debug)
            Debug.Log("Selected mesh option index: " + sel);

        KeyValuePair<int, float>[] options = corrMeshOptions[sel];
        List<GameObject> components = new List<GameObject>();
        List<GameObject> coll_components = new List<GameObject>();
        foreach (KeyValuePair<int, float> kvp in options)
        {
            GameObject component = null, coll_component = null;
            switch (kvp.Key)
            {
                default: break;
                case 0: component = components.AddClone(corridorConn360, null); coll_component = coll_components.AddClone(coll_corridorConn360, null); break;
                case 1: component = components.AddClone(corridorConn300, null); coll_component = coll_components.AddClone(coll_corridorConn300, null); break;
                case 2: component = components.AddClone(corridorConn240, null); coll_component = coll_components.AddClone(coll_corridorConn240, null); break;
                case 3: component = components.AddClone(corridorConn180, null); coll_component = coll_components.AddClone(coll_corridorConn180, null); break;
                case 4: component = components.AddClone(corridorConn120, null); coll_component = coll_components.AddClone(coll_corridorConn120, null); break;
                case 5: component = components.AddClone(corridorConn060, null); coll_component = coll_components.AddClone(coll_corridorConn060, null); break;
            }
            component.transform.eulerAngles = Vector3.up * kvp.Value;
            coll_component.transform.eulerAngles = Vector3.up * kvp.Value;
        }

        return (components, coll_components);
    }

    public (Mesh, Mesh) CorridorTileMesh(LevelTile.HexConnections connections, bool[] debug = null)
    {
        if (debug[1])
            Debug.Log("Generating corridor tile mesh with the following connections:\n" + connections.ToString());

        List<GameObject> components, coll_components;
        (components, coll_components) = CorridorTileMeshComponents(connections, debug[4]);

        int i, nComponents = components.Count;
        string compVertCounts = nComponents > 0 ? "Vertex count per component:" : "Vertex count per component:\n < No components >";
        if (debug[5])
        {
            Mesh _mesh;
            for (i = 0; i < components.Count; i++)
            {
                _mesh = components[i].GetMesh();
                if (_mesh != null)
                    compVertCounts += "\n - Component " + i + ": " + _mesh.vertexCount;
            }
        }

        components.Rescale(wallHeight, Axis.Y);
        coll_components.Rescale(wallHeight, Axis.Y);
        Mesh meshOut = UnityExt_Mesh.MergeFrom(components, true);
        Mesh coll_meshOut = UnityExt_Mesh.MergeFrom(coll_components, true);

        if (debug[3])
        {
            if (debug[5])
                Debug.Log("[Corridor] " + nComponents + " components " + (debug[0] ? " --> " + meshOut.vertexCount + " vertices" : "generated for mesh") + "\n" + compVertCounts);
            else
                Debug.Log("[Corridor] " + nComponents + " components generated for mesh");
        }
        else if (debug[5])
        {
            Debug.Log(compVertCounts);
        }

        return (meshOut, coll_meshOut);
    }
    public (Mesh, Mesh) CorridorTileMesh(Vec2Int tilePos) => CorridorTileMesh(tiles[tilePos].connections);

    public Mesh FloorMesh(LevelArea area)
    {
        int i, iL, iR;
        List<GameObject> components = new List<GameObject>();
        Vec2Int gPos;
        Vec2Int[] adj;
        bool[] emptyAdj = new bool[6];
        foreach (LevelTile tile in area.tiles)
        {
            gPos = tile.gridPosition;
            adj = HexGrid2D.AllAdjacent(gPos);
            for (i = 0; i < 6; i++)
            {
                emptyAdj[i] = tiles.EmptyAt(adj[i]);
            }

            for (i = 0, iL = 5, iR = 1; i < 6; i++, iL++, iR++)
            {
                if (iL > 5)
                    iL = 0;
                if (iR > 5)
                    iR = 0;

                GameObject component;
                if (emptyAdj[i])
                {
                    component = components.AddClone(floorEdge, null);
                }
                else
                {
                    if (emptyAdj[iL])
                    {
                        if (emptyAdj[iR])
                            component = components.AddClone(floorBothI2E, null);
                        else
                            component = components.AddClone(floorLeftI2E, null);
                    }
                    else if (emptyAdj[iR])
                        component = components.AddClone(floorRightI2E, null);
                    else
                        component = components.AddClone(floorInner, null);
                }
                component.transform.eulerAngles = Vector3.up * 60.0f * i;
                component.transform.position = tile.transform.position;
            }
        }
        return UnityExt_Mesh.MergeFrom(components, true);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public EnemyClass[] PickEnemyClasses(int toPick = 3)
    {
        EnemyClass[] classes = new EnemyClass[toPick];
        return classes;
    }

    public ItemRarity[] PickItemRarities(int toPick = 1)
    {
        ItemRarity[] rarities = new ItemRarity[toPick];
        return rarities;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    /*public void CorridorTest()
    {
        LevelTile.HexConnections connections = new LevelTile.HexConnections();
        List<GameObject> components = new List<GameObject>();
        float spacing = 15;
        Vector3 pos = Vector3.zero, posZ = Vector3.zero;
        int x, z, a = 0, b, c, d;
        for (z = 0, pos.z = 3.5f * spacing ; z < 8; z++, pos.z -= spacing)
        {
            for (x = 0, pos.x = -3.5f * spacing; x < 8; x++, a++, pos.x += spacing)
            {
                b = a;
                for (c = 5; c >= 0; c--)
                {
                    d = 2.Pow((ushort)(c));
                    if (b >= d)
                    {
                        b -= d;
                        connections[c] = LevelTile.ConnectionState.Connect;
                    }
                    else
                    {
                        connections[c] = LevelTile.ConnectionState.Block;
                    }
                }
                components.AddRange(CorridorTileMeshComponents(connections));
                GameObject obj = NewObject((a + " | " + connections.ToString(true)), transform, pos, typeof(MeshFilter), typeof(MeshRenderer));
                obj.GetComponent<MeshFilter>().sharedMesh = UnityExt_Mesh.MergeFrom(components, false);
                obj.GetComponent<MeshRenderer>().sharedMaterial = UnityExt_Material.DefaultDiffuse;
                foreach (GameObject cObj in components)
                    cObj.transform.SetParent(obj.transform, false);
                components.Clear();
            }
        }
    }*/
}

[CustomEditor(typeof(Generation))]
[CanEditMultipleObjects]
public class GenerationEditor : Editor
{
    private Generation targ { get { return target as Generation; } }
    Rect rect, toggleRect;
    GUIContent label = new GUIContent() { tooltip = null };

    bool showObjects = false;
    bool showComponents = false;
    bool showCollComponents = false;
    bool showMaterials = false;
    bool showSettings = false;
    bool showDebug = false;

    private Object ObjField(string labelText, Object obj, System.Type type)
    {
        label.text = labelText;
        rect = EditorElements.PrefabLabel(label);
        EditorGUILayout.Space(1);
        return EditorGUI.ObjectField(rect, obj, type, false);
    }
    private Transform TrnField(string labelText, Transform trn)
    {
        label.text = labelText;
        rect = EditorElements.IconLabel(label, "Transform Icon", "Transform component of an existing object");
        EditorGUILayout.Space(1);
        return EditorGUI.ObjectField(rect, trn, typeof(Transform), true) as Transform;
    }
    private Material MatField(string labelText, Material mat)
    {
        label.text = labelText;
        rect = EditorElements.PrefixLabel(label);
        EditorGUILayout.Space(1);
        return EditorGUI.ObjectField(rect, label, mat, typeof(Material), false) as Material;
    }
    private float FloatField(string labelText, float f)
    {
        label.text = labelText;
        rect = EditorElements.PrefixLabel(label);
        EditorGUILayout.Space(1);
        return EditorGUI.FloatField(rect, f);
    }
    private bool Toggle(string labelText, bool toggle)
    {
        int w = 16;
        label.text = labelText;
        rect = EditorElements.ControlRect();
        toggleRect = new Rect(rect) { width = w, x = rect.x + 1 };
        rect.x += w + 2; rect.width -= w;
        EditorGUI.LabelField(rect, label);
        EditorGUILayout.Space(1);
        return EditorGUI.Toggle(toggleRect, toggle);
    }
    public override void OnInspectorGUI()
    {
        float slh = EditorGUIUtility.singleLineHeight;

        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding, GUIStyle.none);
        {
            EditorGUILayout.Space(2);

            if (showObjects = EditorGUILayout.Foldout(showObjects, new GUIContent("World Objects"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    EditorElements.SectionHeader("Transform Parents");
                    targ.mapTransform = TrnField("Map Transform", targ.mapTransform);
                    targ.entityTransform = TrnField("Entity Transform", targ.entityTransform);
                    targ.objectTransform = TrnField("Object Transform", targ.objectTransform);
                    targ.miniMapTransform = TrnField("Minimap Transform", targ.miniMapTransform);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Templates");
                    targ.tileTemplate = ObjField("Tile Template", targ.tileTemplate, typeof(LevelTile)) as LevelTile;
                    targ.spawnPoint = ObjField("Spawn Point", targ.spawnPoint, typeof(PlayerSpawn)) as PlayerSpawn;
                    targ.endTarget = ObjField("End Target", targ.endTarget, typeof(GameObject)) as GameObject;
                    targ.worldEnemy = ObjField("World Enemy", targ.worldEnemy, typeof(GameObject)) as GameObject;
                    targ.enemyPathAnchor = ObjField("Path Anchor", targ.enemyPathAnchor, typeof(GameObject)) as GameObject;
                    targ.worldItem = ObjField("World Item", targ.worldItem, typeof(GameObject)) as GameObject;

                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);

            if (showComponents = EditorGUILayout.Foldout(showComponents, new GUIContent("Component Mesh Objects"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    EditorElements.SectionHeader("Corridor");
                    targ.corridorConn060 = ObjField("60° Inner", targ.corridorConn060, typeof(GameObject)) as GameObject;
                    targ.corridorConn300 = ObjField("60° Outer", targ.corridorConn300, typeof(GameObject)) as GameObject;
                    targ.corridorConn120 = ObjField("120° Inner", targ.corridorConn120, typeof(GameObject)) as GameObject;
                    targ.corridorConn240 = ObjField("120° Outer", targ.corridorConn240, typeof(GameObject)) as GameObject;
                    targ.corridorConn180 = ObjField("Straight", targ.corridorConn180, typeof(GameObject)) as GameObject;
                    targ.corridorConn360 = ObjField("End", targ.corridorConn360, typeof(GameObject)) as GameObject;
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Room");
                    targ.roomWall = ObjField("Wall", targ.roomWall, typeof(GameObject)) as GameObject;
                    targ.roomDoorway = ObjField("Doorway", targ.roomDoorway, typeof(GameObject)) as GameObject;
                    targ.roomWallCorner = ObjField("Wall Corner", targ.roomWallCorner, typeof(GameObject)) as GameObject;
                    targ.roomConnectorL = ObjField("Connector (Left)", targ.roomConnectorL, typeof(GameObject)) as GameObject;
                    targ.roomConnectorR = ObjField("Connector (Right)", targ.roomConnectorR, typeof(GameObject)) as GameObject;
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Floor");
                    targ.floorInner = ObjField("Inner", targ.floorInner, typeof(GameObject)) as GameObject;
                    targ.floorEdge = ObjField("Edge", targ.floorEdge, typeof(GameObject)) as GameObject;
                    targ.floorLeftI2E = ObjField("Left To Edge", targ.floorLeftI2E, typeof(GameObject)) as GameObject;
                    targ.floorRightI2E = ObjField("Right To Edge", targ.floorRightI2E, typeof(GameObject)) as GameObject;
                    targ.floorBothI2E = ObjField("Both To Edge", targ.floorBothI2E, typeof(GameObject)) as GameObject;
                    EditorGUILayout.Space(0);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);
            
            if (showCollComponents = EditorGUILayout.Foldout(showCollComponents, new GUIContent("Collider Mesh Objects"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    EditorElements.SectionHeader("Corridor");
                    targ.coll_corridorConn060 = ObjField("60° Inner", targ.coll_corridorConn060, typeof(GameObject)) as GameObject;
                    targ.coll_corridorConn300 = ObjField("60° Outer", targ.coll_corridorConn300, typeof(GameObject)) as GameObject;
                    targ.coll_corridorConn120 = ObjField("120° Inner", targ.coll_corridorConn120, typeof(GameObject)) as GameObject;
                    targ.coll_corridorConn240 = ObjField("120° Outer", targ.coll_corridorConn240, typeof(GameObject)) as GameObject;
                    targ.coll_corridorConn180 = ObjField("Straight", targ.coll_corridorConn180, typeof(GameObject)) as GameObject;
                    targ.coll_corridorConn360 = ObjField("End", targ.coll_corridorConn360, typeof(GameObject)) as GameObject;
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Room");
                    targ.coll_roomWall = ObjField("Wall", targ.coll_roomWall, typeof(GameObject)) as GameObject;
                    targ.coll_roomDoorway = ObjField("Doorway", targ.coll_roomDoorway, typeof(GameObject)) as GameObject;
                    targ.coll_roomConnectorL = ObjField("Connector (Left)", targ.coll_roomConnectorL, typeof(GameObject)) as GameObject;
                    targ.coll_roomConnectorR = ObjField("Connector (Right)", targ.coll_roomConnectorR, typeof(GameObject)) as GameObject;
                    EditorGUILayout.Space(0);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);

            if (showMaterials = EditorGUILayout.Foldout(showMaterials, new GUIContent("Materials"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    targ.matWallRoom = MatField("Wall (Room)", targ.matWallRoom);
                    targ.matWallCorridor = MatField("Wall (Corridor)", targ.matWallCorridor);
                    targ.matMiniMap = MatField("Wall (Minimap)", targ.matMiniMap);
                    targ.matFloor = MatField("Floor", targ.matFloor);
                    targ.matDoor = MatField("Door", targ.matDoor);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);

            if (showSettings = EditorGUILayout.Foldout(showSettings, new GUIContent("Generation Settings"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    targ.tileRadius = FloatField("Tile Radius (Outer)", targ.tileRadius);
                    targ.tileInnerRadius = FloatField("Tile Radius (Inner)", targ.tileInnerRadius);
                    targ.switchRadiusAxis = Toggle("Switch Radius Axis", targ.switchRadiusAxis);
                    targ.radiusToCorner = Toggle("Radius Measures To Corner", targ.radiusToCorner);
                    label.text = "Mesh Rotation";
                    rect = EditorElements.PrefixLabel(label);
                    targ.meshRotation = EditorElements.Slider(rect, targ.meshRotation, 0, 360);
                    EditorGUILayout.Space(1);
                    label.text = "Room Merge Chance";
                    rect = EditorElements.PrefixLabel(label);
                    targ.mergeRooms = EditorElements.PercentSlider(rect, targ.mergeRooms, 2, false);
                    EditorGUILayout.Space(1);
                    targ.wallHeight = FloatField("Wall Height", targ.wallHeight);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);

            if (showDebug = EditorGUILayout.Foldout(showDebug, new GUIContent("Debug Toggles"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    EditorElements.SectionHeader("Rooms");
                    targ._TileUpdateFail = Toggle("Tile Update Failure", targ._TileUpdateFail);
                    targ._NoAvPositions = Toggle("No Available Positions", targ._NoAvPositions);
                    targ._InternalConns = Toggle("Inernal Connections", targ._InternalConns);
                    targ._ConnectCount = Toggle("Connection Count", targ._ConnectCount);
                    targ._CorridorStarts = Toggle("Corridor Starts", targ._CorridorStarts);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Corridors");
                    targ._SourceTileNull = Toggle("Source Tile Null Check", targ._SourceTileNull);
                    targ._TileConnectFail = Toggle("Tile Connection Failure", targ._TileConnectFail);
                    targ._TerminateCauses = Toggle("Termination Causes", targ._TerminateCauses);
                    targ._WeightedDirs = Toggle("Weighted Directions", targ._WeightedDirs);
                    targ._Endpoints = Toggle("Endpoints", targ._Endpoints);
                    targ._ConnectTileInfo = Toggle("Tile Connection Info", targ._ConnectTileInfo);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Meshes (General)");
                    targ._MeshResult = Toggle("Mesh Result Info", targ._MeshResult);
                    targ._ComponentCount = Toggle("Component Count", targ._ComponentCount);
                    targ._ComponentVerts = Toggle("Vertex Count Per Component", targ._ComponentVerts);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Meshes (Room)");
                    targ._RoomInitialInfo = Toggle("Initial Info", targ._RoomInitialInfo);
                    targ._RoomDirOffsets = Toggle("Direction Offsets", targ._RoomDirOffsets);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Meshes (Corridor)");
                    targ._CrdrOptionSelect = Toggle("Layout Option Selected", targ._CrdrOptionSelect);
                    EditorGUILayout.Space(4);

                    targ.endingDebug = Toggle("Process End Information", targ.endingDebug);
                }
                EditorElements.EndSubSection();
            }
        }
        EditorElements.EndHorizVert();
    }
}


public static class RoomGenTemplatePoints
{
    public static float cos30 = Mathf.Cos(30.0f.ToRad());
    public static float cos60 = Mathf.Cos(60.0f.ToRad());
    public static float sin30 { get { return cos60; } }
    public static float sin60 { get { return cos30; } }

    /* "rm" ---> room */
    /* "cr" ---> corridor */
    /* "wl" ---> wall */
    /* "inr" --> inner */
    /* "otr" --> outer */
    /* "rad" --> radius*/
    /* "wdt" --> width */

    public static float tileRad = 6.0f;
    public static float tileRadC60 { get { return tileRad * cos60; } }
    public static float tileRadS60 { get { return tileRad * sin60; } }
    public static float wlWdt = 0.5f;

    public static float rmInrRad = 5.0f;
    public static float rmInrRadC60 { get { return tileRad * cos60; } }
    public static float rmInrRadS60 { get { return tileRad * sin60; } }
    public static float rmOtrRad { get { return rmInrRad + (wlWdt / Mathf.Sin(60.0f.ToRad())); } }
    public static float rmOtrRadC60 { get { return tileRad * cos60; } }
    public static float rmOtrRadS60 { get { return tileRad * sin60; } }

    public static float crOtrWdt { get { return rmInrRadC60 - 0.2f; } }
    public static float crInrWdt { get { return crOtrWdt - wlWdt; } }

    private static Vector3 Rotate(Vector3 point, float rotRadCos, float rotRadSin)
    {
        float x = point.x * rotRadCos - point.z * rotRadSin;
        float z = point.z * rotRadCos + point.x * rotRadSin;
        return new Vector3(x, point.y, z);
    }

    private static Vector3 A1P = new Vector3(rmInrRadC60, -0.1f, rmInrRadS60);
    private static Vector3 A1N = new Vector3(-rmInrRadC60, -0.1f, rmInrRadS60);
    private static Vector3 A2P = new Vector3(crInrWdt, -0.1f, rmInrRadS60);
    private static Vector3 A2N = new Vector3(-crInrWdt, -0.1f, rmInrRadS60);
    private static Vector3 A3P = new Vector3(2 * rmOtrRadC60 - rmInrRadC60, -0.1f, rmInrRadS60);
    private static Vector3 A3N = new Vector3(-(2 * rmOtrRadC60 - rmInrRadC60), -0.1f, rmInrRadS60);

    private static Vector3 B1P = new Vector3(rmOtrRadC60, -0.1f, rmOtrRadS60);
    private static Vector3 B1N = new Vector3(-rmOtrRadC60, -0.1f, rmOtrRadS60);
    private static Vector3 B2P = new Vector3(rmInrRadC60 - (rmOtrRadC60 - rmInrRadC60) * 2, -0.1f, rmOtrRadS60);
    private static Vector3 B2N = new Vector3(-(rmInrRadC60 - (rmOtrRadC60 - rmInrRadC60) * 2), -0.1f, rmOtrRadS60);

    private static Vector3 AB1P = new Vector3(rmInrRadC60 + wlWdt, -0.1f, rmInrRadS60 + wlWdt * Mathf.Sin(15.0f.ToDeg()));
    private static Vector3 AB1N = new Vector3(-(rmInrRadC60 + wlWdt), -0.1f, rmInrRadS60 + wlWdt * Mathf.Sin(15.0f.ToDeg()));

    private static Vector3 C1P = new Vector3(crOtrWdt, -0.1f, tileRadS60);
    private static Vector3 C1N = new Vector3(crOtrWdt, -0.1f, tileRadS60);
    private static Vector3 C2P = new Vector3(crInrWdt, -0.1f, tileRadS60);
    private static Vector3 C2N = new Vector3(crInrWdt, -0.1f, tileRadS60);
    private static Vector3 C3P = new Vector3(rmInrRadC60, -0.1f, tileRadS60);
    private static Vector3 C3N = new Vector3(-rmInrRadC60, -0.1f, tileRadS60);
    private static Vector3 C4P = new Vector3(tileRadC60, -0.1f, tileRadS60);
    private static Vector3 C4N = new Vector3(-tileRadC60, -0.1f, tileRadS60);

    public static MeshData WallBasic(float height, float rotDeg)
    {
        float rCos = Mathf.Cos(rotDeg.ToRad());
        float rSin = Mathf.Sin(rotDeg.ToRad());
        Vector3 hOffset = Vector3.up * (height + 0.1f);
        Vector3[] verts = new Vector3[8]
        {
            Rotate(A1N, rCos, rSin),
            Vector3.zero,
            Rotate(B2N, rCos, rSin),
            Vector3.zero,
            Rotate(A1P, rCos, rSin),
            Vector3.zero,
            Rotate(B2P, rCos, rSin),
            Vector3.zero,
        };
        verts[1] = verts[0] + hOffset;
        verts[3] = verts[2] + hOffset;
        verts[5] = verts[4] + hOffset;
        verts[7] = verts[6] + hOffset;
        int[] tris = new int[18]
        {
            // 0 1 5 4
            0, 1, 4,
            0, 5, 4,
            // 1 3 7 5
            1, 3, 7,
            1, 7, 5,
            // 3 2 7 6
            3, 2, 7,
            3, 7, 6,
        };
        return new MeshData(verts, tris, false);
    }

    public static MeshData WallDoor(float height, float rotDeg)
    {
        float rCos = Mathf.Cos(rotDeg.ToRad());
        float rSin = Mathf.Sin(rotDeg.ToRad());
        Vector3 hOffset = Vector3.up * (height + 0.1f);
        Vector3[] verts = new Vector3[20]
        {
            Rotate(A1N, rCos, rSin),
            Vector3.zero,
            Rotate(A2N, rCos, rSin),
            Vector3.zero,
            Rotate(C2N, rCos, rSin),
            Vector3.zero,
            Rotate(C1N, rCos, rSin),
            Vector3.zero,
            Rotate(B2N, rCos, rSin),
            Vector3.zero,
            Rotate(A1P, rCos, rSin),
            Vector3.zero,
            Rotate(A2P, rCos, rSin),
            Vector3.zero,
            Rotate(C2P, rCos, rSin),
            Vector3.zero,
            Rotate(C1P, rCos, rSin),
            Vector3.zero,
            Rotate(B2P, rCos, rSin),
            Vector3.zero,
        };
        verts[1] = verts[0] + hOffset;
        verts[3] = verts[2] + hOffset;
        verts[5] = verts[4] + hOffset;
        verts[7] = verts[6] + hOffset;
        verts[9] = verts[8] + hOffset;
        verts[11] = verts[10] + hOffset;
        verts[13] = verts[12] + hOffset;
        verts[15] = verts[14] + hOffset;
        verts[17] = verts[16] + hOffset;
        verts[19] = verts[18] + hOffset;
        int[] tris = new int[54]
        {
            // 0 1 3 2
            0, 1, 3,
            0, 3, 2,
            // 2 3 5 4
            2, 3, 5,
            2, 5, 4,
            // 6 7 9 8
            6, 7, 9,
            6, 9, 8,
            // 0 2 4 6 8
            8, 2, 0,
            8, 4, 2,
            8, 6, 4,

            // 10 11 13 12
            10, 11, 13,
            10, 13, 12,
            // 12 13 15 14
            12, 13, 15,
            12, 15, 14,
            // 16 17 19 18
            16, 17, 19,
            16, 19, 18,
            // 10 12 14 16 18
            18, 12, 10,
            18, 14, 12,
            18, 16, 14,
        };
        return new MeshData(verts, tris, false);
    }

    public static MeshData WallCornerInner(float height, float rotDeg)
    {
        float rCos = Mathf.Cos(rotDeg.ToRad());
        float rSin = Mathf.Sin(rotDeg.ToRad());
        Vector3 hOffset = Vector3.up * (height + 0.1f);
        Vector3[] verts = new Vector3[8]
        {
            Rotate(A1P, rCos, rSin),
            Vector3.zero,
            Rotate(B2P, rCos, rSin),
            Vector3.zero,
            Rotate(B1P, rCos, rSin),
            Vector3.zero,
            Rotate(A3P, rCos, rSin),
            Vector3.zero,
        };
        verts[1] = verts[0] + hOffset;
        verts[3] = verts[2] + hOffset;
        verts[5] = verts[4] + hOffset;
        verts[7] = verts[6] + hOffset;
        int[] tris = new int[18]
        {
            // 1 3 5 7
            1, 3, 5,
            1, 5, 7,
            // 3 2 4 5
            3, 2, 4,
            3, 4, 5,
            // 5 4 6 7
            5, 4, 6,
            5, 6, 7,
        };
        return new MeshData(verts, tris, false);
    }

    public static MeshData WallCornerOuterL(float height, float rotDeg)
    {
        float rCos = Mathf.Cos(rotDeg.ToRad());
        float rSin = Mathf.Sin(rotDeg.ToRad());
        Vector3 hOffset = Vector3.up * (height + 0.1f);
        Vector3[] verts = new Vector3[10]
        {
            Rotate(A3N, rCos, rSin),
            Vector3.zero,
            Rotate(A1N, rCos, rSin),
            Vector3.zero,
            Rotate(AB1N, rCos, rSin),
            Vector3.zero,
            Rotate(C3N, rCos, rSin),
            Vector3.zero,
            Rotate(C4N, rCos, rSin),
            Vector3.zero,
        };
        verts[1] = verts[0] + hOffset;
        verts[3] = verts[2] + hOffset;
        verts[5] = verts[4] + hOffset;
        verts[7] = verts[6] + hOffset;
        int[] tris = new int[27]
        {
            // 0 1 9 8
            0, 1, 9,
            0, 9, 8,
            // 3 2 4 5
            3, 2, 4,
            3, 4, 5,
            // 5 4 6 7
            5, 4, 6,
            5, 6, 7,
            // 5 7 9 1 3
            5, 7, 9,
            5, 9, 1,
            5, 1, 3,
        };
        return new MeshData(verts, tris, false);
    }

    public static MeshData WallCornerOuterR(float height, float rotDeg)
    {
        float rCos = Mathf.Cos(rotDeg.ToRad());
        float rSin = Mathf.Sin(rotDeg.ToRad());
        Vector3 hOffset = Vector3.up * (height + 0.1f);
        Vector3[] verts = new Vector3[10]
        {
            Rotate(A3P, rCos, rSin),
            Vector3.zero,
            Rotate(A1P, rCos, rSin),
            Vector3.zero,
            Rotate(AB1P, rCos, rSin),
            Vector3.zero,
            Rotate(C3P, rCos, rSin),
            Vector3.zero,
            Rotate(C4P, rCos, rSin),
            Vector3.zero,
        };
        verts[1] = verts[0] + hOffset;
        verts[3] = verts[2] + hOffset;
        verts[5] = verts[4] + hOffset;
        verts[7] = verts[6] + hOffset;
        int[] tris = new int[27]
        {
            // 0 1 9 8
            0, 1, 9,
            0, 9, 8,
            // 3 2 4 5
            3, 2, 4,
            3, 4, 5,
            // 5 4 6 7
            5, 4, 6,
            5, 6, 7,
            // 5 7 9 1 3
            5, 7, 9,
            5, 9, 1,
            5, 1, 3,
        };
        return new MeshData(verts, tris, false);
    }
}
