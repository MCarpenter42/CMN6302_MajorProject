using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Maths;
using NeoCambion.Random.Unity;
using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public interface LevelArea : IEnumerable
{
    public enum AreaType { Null = -1, Room, Corridor }
    public struct AreaID : System.IEquatable<AreaID>, System.IComparable<AreaID>
    {
        public AreaType type;
        public int value;

        public AreaID(AreaType type, int ID)
        {
            this.type = type;
            this.value = ID;
        }

        public static AreaID Null => new AreaID(AreaType.Null, int.MinValue);
        public bool isNull => type == AreaType.Null && value == int.MinValue;

        public override bool Equals(object obj)
        {
            if (isNull && obj == null)
                return true;
            if (obj.GetType() != typeof(AreaID))
                return false;
            return Equals((AreaID)obj);
        }
        public bool Equals(AreaID other) => type == other.type && value == other.value;
        public int CompareTo(AreaID compareTo)
        {
            if (type == compareTo.type)
            {
                if (value == compareTo.value)
                    return 2;
                else
                    return 1;
            }
            return 0;
        }
        public override int GetHashCode() => (type, value).GetHashCode();
        public static bool operator ==(AreaID operand1, AreaID operand2) => operand1.Equals(operand2);
        public static bool operator !=(AreaID operand1, AreaID operand2) => !operand1.Equals(operand2);

        public override readonly string ToString() => type + ": " + value;
    }
    public static AreaID RoomID(int ID) => new AreaID(AreaType.Room, ID);
    public static AreaID CorridorID(int ID) => new AreaID(AreaType.Corridor, ID);
    public static AreaID NullID => AreaID.Null;

    public GameObject gameObject { get; }
    public Transform transform { get; }

    public AreaID ID { get; }

    public List<Vec2Int> tilePositions { get; }
    public List<LevelTile> tiles { get; }
    public Vec2Int startTile { get { return tilePositions.HasContents() ? tilePositions[0] : Vec2Int.zero; } }

    public bool containsEnemy { get; set; }
    public int enemyCount { get; }
    public WorldEnemySet enemies { get; set; }
    public int itemCount { get; }
    public int itemCapacity { get; }
    public List<WorldItem> items { get; set; }

    public bool Contains(Vec2Int position);

    public void UpdatePositions();

    public Vector3 RandInternalPosition();
    public bool IncrementItemCount();
    public (Vector3[], Vector3[]) SpawnPositions();
    public (PositionsInArea, PositionsInArea) SpawnPositionsInArea(int indexOverride);
}

public struct PositionInArea
{
    LevelArea.AreaID areaID;
    public Vector3 worldPosition;

    public PositionInArea(LevelArea.AreaID areaID, Vector3 worldPosition)
    {
        this.areaID = areaID;
        this.worldPosition = worldPosition;
    }

    public PositionInArea Room(int roomIndex, Vector3 worldPosition) => new PositionInArea(LevelArea.RoomID(roomIndex), worldPosition);
    public PositionInArea Corridor(int corridorIndex, Vector3 worldPosition) => new PositionInArea(LevelArea.CorridorID(corridorIndex), worldPosition);

    public PositionInArea Offset(Vector3 offset) => new PositionInArea(areaID, worldPosition + offset);
}
public struct PositionsInArea
{
    LevelArea.AreaID areaID;
    public Vector3[] worldPositions;

    public Vector3 this[int index]
    {
        get { return worldPositions[index]; }
        set { worldPositions[index] = value; }
    }
    public int Length => worldPositions.Length;

    public PositionsInArea(LevelArea.AreaID areaID, int posCount)
    {
        this.areaID = areaID;
        this.worldPositions = new Vector3[posCount];
    }
    public PositionsInArea(LevelArea.AreaID areaID, Vector3[] worldPositions)
    {
        this.areaID = areaID;
        this.worldPositions = worldPositions;
    }

    public PositionsInArea Room(int roomIndex, Vector3[] worldPositions) => new PositionsInArea(LevelArea.RoomID(roomIndex), worldPositions);
    public PositionsInArea Corridor(int corridorIndex, Vector3[] worldPositions) => new PositionsInArea(LevelArea.CorridorID(corridorIndex), worldPositions);

    public PositionInArea Single(int index) => new PositionInArea(areaID, worldPositions[index]);
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
            public int Count => tiles.Count;
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
            public LevelTile[] GetAll() => tiles.Values.ToArray();

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
                    tile.SetAttributes(position, position.TilePosition(), type);
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
                        tile = tiles.AddCloneValue(x, generator.tileTemplate, grid.generator.mapTransform).Value;
                        tile.SetAttributes(position, position.TilePosition(), type);
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
                    foreach (LevelTile tile in tiles.Values)
                    {
                        if (tile != null)
                            tile.gameObject.DestroyThis(1f);
                    }
                }
                tiles.Clear();
            }

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            /*public void GenerateMeshes(bool[] debug = null)
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

                    if (debug != null && debug[0] && !(debug != null && debug[3] && debug[5]))
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
            }*/

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

        public Generation generator;

        private Dictionary<int, GridRow> rows;
        public int Count
        {
            get
            {
                int n = 0;
                foreach (GridRow row in rows.Values)
                {
                    n += row.Count;
                }
                return n;
            }
        }

        public Grid(Generation generator)
        {
            this.generator = generator;
            rows = new Dictionary<int, GridRow>();
        }
        public Grid(Generation generator, ICollection<LevelTile> tiles)
        {
            this.generator = generator;
            rows = new Dictionary<int, GridRow>();
            OverwriteTiles(tiles);
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

        public LevelTile[] GetTiles()
        {
            LevelTile[] allTiles = new LevelTile[Count];
            int i = 0;
            foreach (GridRow row in rows.Values)
            {
                foreach (LevelTile tile in row.GetAll())
                {
                    allTiles[i++] = tile;
                }
            }
            return allTiles;
        }

        public void OverwriteTiles(ICollection<LevelTile> tiles)
        {
            if (Count > 0)
                Clear();
            foreach (LevelTile tile in tiles)
            {
                Add(tile);
            }
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
        public bool Add(LevelTile tile)
        {
            Vec2Int position = tile.gridPosition;
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
            }
            rows.Clear();
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

        /*public void GenerateMeshes(bool[] debug = null)
        {
            foreach (GridRow row in rows.Values)
                row?.GenerateMeshes(debug);
        }*/

        public void GenerateMiniTiles(Transform parentTransform, Material miniTileMat)
        {
            foreach (LevelRoom room in LevelManager.Rooms)
            {
                foreach (LevelTile tile in room.tiles)
                {
                    tile.SpawnMiniTile(parentTransform, miniTileMat, room.revealed);
                }
            }
            foreach (LevelCorridor corr in LevelManager.Corridors)
            {
                foreach (LevelTile tile in corr.tiles)
                {
                    tile.SpawnMiniTile(parentTransform, miniTileMat, corr.revealed);
                }
            }
            /*foreach (GridRow row in rows.Values)
            {
                row.GenerateMiniTiles(parentTransform, miniTileMat);
            }*/
        }
    }

    public Grid TileGrid => LevelManager.TileGrid;

    #region [ OBJECTS / COMPONENTS ]

    public Transform mapTransform;
    public Transform entityTransform;
    public Transform objectTransform;
    public Transform miniMapTransform;

    public LevelTile tileTemplate;
    public StageStart spawnPoint;
    public StageEnd endTarget;
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


    [SerializeField] Material matWallRoom;
    public Material MatWallRoom { get { return matWallRoom ?? Ext_Material.DefaultDiffuse; } }
    [SerializeField] Material matWallCorridor;
    public Material MatWallCorridor { get { return matWallCorridor ?? Ext_Material.DefaultDiffuse; } }
    [SerializeField] Material matDoor;
    public Material MatDoor { get { return matDoor ?? Ext_Material.DefaultDiffuse; } }
    [SerializeField] Material matFloor;
    public Material MatFloor { get { return matFloor ?? Ext_Material.DefaultDiffuse; } }
    [SerializeField] Material matMiniMap;
    public Material MatMiniMap { get { return matMiniMap ?? Ext_Material.DefaultDiffuse; } }

    #endregion

    #region [ PROPERTIES ]

    public static LevelTile.TileType tNone = LevelTile.TileType.None;
    public static LevelTile.TileType tEmpty = LevelTile.TileType.Empty;
    public static LevelTile.TileType tCorridor = LevelTile.TileType.Corridor;
    public static LevelTile.TileType tRoom = LevelTile.TileType.Room;
    public static LevelArea.AreaType aRoom = LevelArea.AreaType.Room;
    public static LevelArea.AreaType aCorridor = LevelArea.AreaType.Corridor;

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

    public (List<LevelRoom>, List<LevelCorridor>) GenerateMap(int maxIterations, RoomStructureVariance roomVariance, CorridorStructureVariance corrVariance, bool connectToExisting)
    {
        int i, j, n;
        int nRooms = 1, nCorridors = 0, iterCount = 0;

        List<RoomSourceRef> newRooms = new List<RoomSourceRef>();
        List<LevelRoom> rooms = new List<LevelRoom>();
        List<CorridorSourceRef> newCorridors = new List<CorridorSourceRef>();
        List<LevelCorridor> corridors = new List<LevelCorridor>();

        LevelRoom newRoom = NewRoom(roomVariance, true, 0, roomDebug);
        rooms.Add(newRoom);
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
                    corridors.Add(newCorridor);
                    newRooms.AddRange(newCorridor.GetRoomStarts());
                    foreach (Vec2Int pos in newCorridor.tilePositions)
                    {
                        if (TileGrid[pos] != null)
                            TileGrid[pos].connections.Replace();
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
                    rooms.Add(newRoom);
                    if (i < maxIterations - 1)
                        nConnect = newRoom.ConnectionCount(roomDebug[3]) - (i > 2 ? branchRestrict++ : 0);
                    newCorridors.AddRange(newRoom.GetCorridorStarts(nConnect, roomDebug[4]));
                    foreach (Vec2Int pos in newRoom.tilePositions)
                    {
                        if (TileGrid[pos] != null)
                            TileGrid[pos].connections.Replace();
                    }
                    nRooms++;
                }
            }
            newRooms.Clear();

            if (newCorridors.Count == 0)
                break;
        }

        if (endingDebug)
            Debug.Log("Iterations: " + iterCount + " | Total rooms: " + nRooms + " | Total corridors: " + nCorridors);

        return (rooms, corridors);
    }
    /*public (List<LevelRoom>, List<LevelCorridor>) GenerateMap(MapGenSaveData existingData)
    {
        List<LevelRoom> rooms = new List<LevelRoom>();
        List<LevelCorridor> corridors = new List<LevelCorridor>();

        foreach (TileSaveData tileData in existingData.tiles)
        {
            LevelTile newTile = UpdateTile(tileData.gridPosition, tileData.type, true);
            newTile.connections.Overwrite(tileData.connections);
            TileGrid.Add(newTile);
        }
        foreach (RoomSaveData roomData in existingData.rooms)
        {
            LevelRoom newRoom = NewObject("Room " + roomData.ID, mapTransform, typeof(LevelRoom)).GetComponent<LevelRoom>();
            newRoom.Initialise(this, roomData);
            rooms.Add(newRoom);
        }
        foreach (CorridorSaveData corrData in existingData.corridors)
        {
            LevelCorridor newCorr = NewObject("Room " + corrData.ID, mapTransform, typeof(LevelCorridor)).GetComponent<LevelCorridor>();
            newCorr.Initialise(this, corrData);
            corridors.Add(newCorr);
        }

        return (rooms, corridors);
    }*/

    public struct LevelPopulationData
    {
        public Vector3 spawnPosition;
        public Vector3 endPosition;
        public Dictionary<LevelArea.AreaID, WorldEnemySet> enemySets;
        public Dictionary<LevelArea.AreaID, List<WorldItem>> items;

        public LevelPopulationData(Vector3 spawnPosition, Vector3 endPosition, Dictionary<LevelArea.AreaID, WorldEnemySet> enemySets, Dictionary<LevelArea.AreaID, List<WorldItem>> items)
        {
            this.spawnPosition = spawnPosition;
            this.endPosition = endPosition;
            this.enemySets = enemySets;
            this.items = items;
        }
    }
    public LevelPopulationData PopulateLevel(FloatRange enemyDensityRange, FloatRange itemDensityRange)
    {
        int i, j, iR, roomCount = LevelManager.Rooms.Count;
        float enemyDensity = Random.Range(enemyDensityRange.lower, enemyDensityRange.upper);
        float itemDensity = Random.Range(itemDensityRange.lower, itemDensityRange.upper);

        Vector3 spawnPosition, endPosition;
        LevelArea.AreaID spawnArea, endArea;
        Dictionary<LevelArea.AreaID, WorldEnemySet> enemySets = new Dictionary<LevelArea.AreaID, WorldEnemySet>();
        Dictionary<LevelArea.AreaID, List<WorldItem>> items = new Dictionary<LevelArea.AreaID, List<WorldItem>>();

        List<LevelArea.AreaID> playerAreas, enemyAreas, itemAreas, areasToPopulate = new List<LevelArea.AreaID>();
        playerAreas = LevelManager.AllAreas.GetIDs();
        enemyAreas = LevelManager.Rooms.GetIDs();
        itemAreas = LevelManager.Rooms.GetIDs();
        foreach (LevelCorridor corr in LevelManager.Corridors)
        {
            if (corr.ValidItemPlacements.Length > 0)
                itemAreas.Add(corr.ID);
        }

        iR = playerAreas.Count > 1 ? Random.Range(1, playerAreas.Count) : 0;
        spawnArea = playerAreas[iR];
        spawnPosition = LevelManager.RandPos(spawnArea);
        if (playerAreas.Count > 1)
            playerAreas.Remove(spawnArea);

        if (enemyAreas.Contains(spawnArea) && enemyAreas.Count > 1)
            enemyAreas.Remove(spawnArea);

        iR = Random.Range(0, playerAreas.Count);
        endArea = playerAreas[iR];
        endPosition = LevelManager.RandPos(endArea);

        int enemyCount = Mathf.RoundToInt(roomCount * enemyDensity);
        if (enemyCount < 2)
            enemyCount = 2;
        int itemCount = Mathf.RoundToInt(roomCount * itemDensity);
        if (itemCount < 1)
            itemCount = 1;

        for (i = 0; i < enemyCount; i++)
        {
            iR = Random.Range(0, enemyAreas.Count);
            LevelArea area = LevelManager.GetArea(enemyAreas[iR]);
            if (!area.containsEnemy)
            {
                area.containsEnemy = true;
                enemyAreas.Transfer(iR, areasToPopulate);
            }
            if (enemyAreas.Count < 1)
            {
                if (i == enemyCount - 1)
                    Debug.Log("Terminating enemy spawn selection early");
                break;
            }
        }

        for (i = 0; i < itemCount; i++)
        {
            iR = Random.Range(0, itemAreas.Count);
            LevelArea area = LevelManager.GetArea(itemAreas[iR]);
            if (area.itemCount == 0)
                areasToPopulate.AddIfUnique(itemAreas[iR]);
            if (!area.IncrementItemCount())
                itemAreas.RemoveAt(iR);
        }

        foreach (LevelArea.AreaID ID in areasToPopulate)
        {
            LevelArea area = LevelManager.GetArea(ID);
            if (area != null)
            {
                (Vector3[] enemyPositions, Vector3[] itemPositions) = area.SpawnPositions();
                if (enemyPositions.Length > 0)
                {
                    area.enemies = new WorldEnemySet(area);
                    enemySets.TryAdd(area.ID, area.enemies);
                    for (i = 0; i < enemyPositions.Length; i++)
                    {
                        WorldEnemy newEnemy = Instantiate(worldEnemy, entityTransform).GetComponent<WorldEnemy>();
                        area.enemies.Add(newEnemy, enemyPositions[i]);
                        if (newEnemy == null) Debug.Log("Null world enemy!");
                    }
                }
                if (itemPositions.Length > 0)
                {
                    area.items = new List<WorldItem>();
                    items.TryAdd(area.ID, area.items);
                    for (i = 0; i < itemPositions.Length; i++)
                    {
                        WorldItem newItem = Instantiate(worldItem, objectTransform).GetComponent<WorldItem>();
                        area.items.Add(newItem);
                        newItem.Setup(area, itemPositions[i]);
                    }
                }
            }
        }
        return new LevelPopulationData(spawnPosition, endPosition, enemySets, items);
    }

    public (List<LevelRoom>, List<LevelCorridor>) GenerateRestMap() => (new List<LevelRoom>() { NewRestArea() }, new List<LevelCorridor>());

    public LevelPopulationData PopulateRestLevel()
    {
        LevelPopulationData popData = new LevelPopulationData();
        popData.spawnPosition = new Vector3(0f, 0f, -3f);
        popData.endPosition = LevelManager.TileGrid.GetTiles().Last().position;
        popData.enemySets = new Dictionary<LevelArea.AreaID, WorldEnemySet>();
        WorldItem newItem = Instantiate(worldItem, objectTransform).GetComponent<WorldItem>();
        if (LevelManager.Rooms[0].items == null)
            LevelManager.Rooms[0].items = new List<WorldItem>() { newItem };
        else
            LevelManager.Rooms[0].items.Add(newItem);
        newItem.Setup(LevelManager.Rooms[0], Vector3.zero);
        popData.items = new Dictionary<LevelArea.AreaID, List<WorldItem>>() { { new LevelArea.AreaID(LevelArea.AreaType.Room, 0), new List<WorldItem>() { newItem } } };
        return popData;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public LevelTile[] LoadTiles(TileSaveData[] data)
    {
        LevelTile[] tiles = new LevelTile[data.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = Instantiate(tileTemplate, mapTransform);
            tiles[i].SetAttributes(data[i].gridPosition, data[i].gridPosition.TilePosition(), data[i].type);
            tiles[i].connections.Overwrite(data[i].connections);
        }
        return tiles;
    }
    public (List<LevelRoom>, List<LevelCorridor>) ReGenerateMap(MapGenSaveData mapGenData)
    {
        TileGrid.OverwriteTiles(LoadTiles(mapGenData.tiles));
        List<LevelRoom> rooms = new List<LevelRoom>();
        foreach (RoomSaveData roomData in mapGenData.rooms)
        {
            rooms.ReturnAdd(NewObject(mapTransform, typeof(LevelRoom)).GetComponent<LevelRoom>()).Initialise(this, roomData);
        }
        List<LevelCorridor> corridors = new List<LevelCorridor>();
        foreach (CorridorSaveData corrData in mapGenData.corridors)
        {
            corridors.ReturnAdd(NewObject(mapTransform, typeof(LevelCorridor)).GetComponent<LevelCorridor>()).Initialise(this, corrData);
        }
        //Debug.Log(rooms.Count + " rooms, " + corridors.Count + " corridors");
        return (rooms, corridors);
    }

    public LevelPopulationData RePopulateLevel(LevelPopSaveData lvlPopData)
    {
        Vector3 spawnPosition, endPosition;
        Dictionary<LevelArea.AreaID, WorldEnemySet> enemySets = new Dictionary<LevelArea.AreaID, WorldEnemySet>();
        Dictionary<LevelArea.AreaID, List<WorldItem>> items = new Dictionary<LevelArea.AreaID, List<WorldItem>>();

        spawnPosition = new Vector3(lvlPopData.spawnPosition.x, 0f, lvlPopData.spawnPosition.y);
        endPosition = new Vector3(lvlPopData.endPosition.x, 0f, lvlPopData.endPosition.y);

        LevelArea.AreaID areaID;
        LevelArea area;
        foreach (EnemySetSaveData setData in lvlPopData.enemySetData)
        {
            areaID = new LevelArea.AreaID(setData.areaType, setData.areaID);
            area = LevelManager.GetArea(areaID);
            WorldEnemySet set = new WorldEnemySet(area);
            enemySets.TryAdd(area.ID, set);
            foreach (int ind in setData.enemyDataInds)
            {
                WorldEnemy newEnemy = Instantiate(worldEnemy, entityTransform).GetComponent<WorldEnemy>();
                set.Add(newEnemy, area.RandInternalPosition());
                newEnemy.SetData(ind);
            }
        }

        List<WorldItem> list;
        foreach (WorldItemSaveData itemData in lvlPopData.itemData)
        {
            areaID = new LevelArea.AreaID(itemData.areaType, itemData.areaID);
            area = LevelManager.GetArea(areaID);
            if (area.items == null)
                area.items = new List<WorldItem>();
            if (!items.ContainsKey(areaID))
            {
                list = new List<WorldItem>();
                items.Add(areaID, list);
            }
            WorldItem newItem = Instantiate(worldItem, objectTransform).GetComponent<WorldItem>();
            area.items.Add(newItem);
            newItem.Setup(area, new Vector3(itemData.position.x, 0f, itemData.position.y));
        }

        return new LevelPopulationData(spawnPosition, endPosition, enemySets, items);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public LevelTile UpdateTile(Vec2Int position, LevelTile.TileType type, bool overwrite = false)
    {
        LevelTile tile = TileGrid.GetOrCreate(position, type);
        bool typeSupercedes = type != tNone && tile.type == tNone;
        if (typeSupercedes || overwrite)
            tile.SetAttributes(position, position.TilePosition(), type);
        return tile;
    }

    public bool SetConnectionStates(Vec2Int position, HexGridDirection direction, LevelTile.ConnectionState state)
    {
        Vec2Int pos2 = HexGrid2D.Adjacent(position, direction);
        LevelTile tileA = TileGrid[position], tileB = TileGrid[pos2];
        if (tileA != null && tileB != null)
        {
            tileA.connections[direction] = state;
            tileB.connections[direction.Invert()] = state;
            return true;
        }
        return false;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

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
        bool cont = !source.isNull || TileGrid[startTile] == null || TileGrid[startTile].emptySpace;

        if (cont)
        {
            room = NewObject("Room " + ID, mapTransform, typeof(LevelRoom)).GetComponent<LevelRoom>();
            room.Initialise(this, ID);

            int sizeMin = roomVar.Min(largeRoom), sizeMax = roomVar.Max(largeRoom) + 1;
            WeightingCurve weighting = largeRoom ? WeightingCurve.None : WeightingCurve.Power;
            int tileCount = Ext_Random.RangeWeighted(sizeMin, sizeMax, weighting, false, 2.0f);

            LevelTile newTile = UpdateTile(startTile, tRoom, true);
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
                    localFree = room.tilePositions[j].Adjacent(tNone);
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
                    newTile = TileGrid.GetOrCreate(targ, tRoom);
                    if (newTile == null)
                    {
                        if (debug != null && debug[0])
                            Debug.Log("Failed to update tile at " + targ.ToString() + " on iteration " + i);
                    }
                    else
                    {
                        room.Add(newTile);
                    }
                }
                else
                {
                    if (debug != null && debug[1])
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
            if (debug != null && debug[2])
                room.DebugInternalConnections();
            else
                room.InternalConnections();
        }
        return room;
    }
    public LevelRoom NewRestArea()
    {
        LevelTile[] tiles = new LevelTile[]
        {
            UpdateTile(new Vec2Int(0, 0), tRoom),
            UpdateTile(new Vec2Int(0, 1), tRoom),
        };
        tiles[0].connections.Overwrite(new int[] { 2, 1, 1, 1, 1, 1 });
        tiles[1].connections.Overwrite(new int[] { 1, 1, 1, 2, 1, 1 });

        LevelRoom room = NewObject("Room " + 0, mapTransform, typeof(LevelRoom)).GetComponent<LevelRoom>();
        room.Initialise(this, 0);
        room.Add(tiles[0]);
        room.Add(tiles[1]);

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

        List<LevelTile> tiles = new List<LevelTile>();
        if (debug != null && debug[0] && TileGrid[initPos] == null)
            Debug.LogError("Corridor source tile " + initPos.ToString() + " is null!");
        firstTile = corr.mainBranch.tiles.Last();
        tiles.Add(UpdateTile(firstTile, tCorridor));

        if (!SetConnectionStates(initPos, initDir, LevelTile.ConnectionState.Connect) && debug[1])
        {
            string logString = "Can't connect a null tile!" + (TileGrid[initPos] == null ? " | From tile " + initPos.ToString() + " is null" : null) + (TileGrid[firstTile] == null ? " | To tile " + firstTile.ToString() + " is null" : null);
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

                (localNone, _, localCorridor, localRoom) = lastPos.CategorisedAdjacent();
                ending = false;
                connecting = false;
                chance = Random.Range(0.0f, 1.0f);
                noFreeSpace = localNone.Count == 0;
                splitting = (branch.child == null) && (ending ? false : (i > 1 ? chance >= (1.0f - corr.branchChance) : false));

                if (noFreeSpace)
                {
                    ending = true;
                    if (debug != null && debug[5])
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
                    if (debug != null && debug[5])
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
                        if (debug != null && debug[5])
                            Debug.Log("Branch terminating due to random chance\nOrigin tile: " + corr.startTile + "\nBranch index: " + branch.index);
                        next = branch.GetConnectionTile(lastPos, localRoom, lastDir, true, debug[2]);
                        ending = true;
                        connecting = true;
                    }
                    else if (localCorridor.Count > 0 && chance <= 0.10f)
                    {
                        if (debug != null && debug[5])
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
                        if (debug != null && debug[4])
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
                        tiles.Add(UpdateTile(nextPos, tCorridor));
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

        corr.GetTiles(true);
        corr.AddTiles(tiles);
        foreach (LevelTile tile in corr.tiles)
            tile.AddAdditionalInfo("Included in corridor " + corr.ID.value);

        db_endPositions = db_endPositions.Substring(0, db_endPositions.Length - 2);
        debugInfo += "\nMax length: " + maxLength + " | Total branches: " + db_totalCorridorBranches + "\n" + db_endPositions + " ]";

        if (debug != null && debug[3])
            Debug.Log(debugInfo);

        /*if (corr.tileCountMismatch)
            Debug.Log("Corridor " + corr.ID.value + "'s tile count is mismatched!");*/

        return corr;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public (Mesh, Mesh) RoomTileMesh(Vec2Int tilePos) => CorridorTileMesh(TileGrid[tilePos].connections);
    public (Mesh, Mesh) RoomTileMesh(LevelTile.HexConnections connections, bool[] debug = null)
    {
        if (debug != null && debug[1])
            Debug.Log("Generating room tile mesh with the following connections:\n" + connections.ToString());

        List<GameObject> components = new List<GameObject>(), coll_components = new List<GameObject>();
        (components, coll_components) = GetComponents(connections, debug != null && debug[2]);

        int nComponents = components.Count;
        string compVertCounts = nComponents > 0 ? "Vertex count per component:" : "Vertex count per component:\n < No components >";
        if (debug != null && debug[5])
        {
            Mesh _mesh;
            for (int i = 0; i < components.Count; i++)
            {
                _mesh = components[i].GetMesh();
                if (_mesh != null)
                    compVertCounts += "\n - Component " + i + ": " + _mesh.vertexCount;
            }
        }

        components.Rescale(wallHeight, Axis.Y);
        Mesh meshOut = Ext_Mesh.MergeFrom(components, true);

        coll_components.Rescale(wallHeight, Axis.Y);
        Mesh coll_meshOut = Ext_Mesh.MergeFrom(coll_components, true);

        if (debug != null && debug[3])
        {
            if (debug != null && debug[5])
                Debug.Log("[Room] " + nComponents + " components generated for mesh" + (debug != null && debug[0] ? " with " + meshOut.vertexCount + " vertices" : null) + "\n" + compVertCounts);
            else
                Debug.Log("[Room] " + nComponents + " components generated for mesh");
        }
        else if (debug != null && debug[5])
        {
            Debug.Log(compVertCounts);
        }

        return (meshOut, coll_meshOut);
    }
    private (List<GameObject>, List<GameObject>) GetComponents(LevelTile.HexConnections connections, bool debug = false)
    {
        List<GameObject> components = new List<GameObject>(), coll_components = new List<GameObject>();
        Quaternion rot;
        HexGridDirection dir;
        LevelTile.ConnectionState conn, connL, connR;

        int i, iters = 0;
        for (i = 0; i < connections.Length; ++i)
        {
            rot = Quaternion.Euler(new Vector3(0.0f, (float)i * 60.0f + meshRotation, 0.0f));
            dir = (HexGridDirection)i;
            conn = connections[dir]; connL = connections[dir.Rotate(-1)]; connR = connections[dir.Rotate(1)];

            if (debug)
                Debug.Log(dir.ToString() + " --> [L] " + dir.Rotate(-1).ToString() + " / [R] " + dir.Rotate(1).ToString());

            if (conn == LevelTile.ConnectionState.Merge)
            {
                if (connL != LevelTile.ConnectionState.Merge)
                {
                    components.AddClone(roomConnectorL, Vector3.zero, rot, null);
                    coll_components.AddClone(coll_roomConnectorL, Vector3.zero, rot, null);
                }
                if (connR != LevelTile.ConnectionState.Merge)
                {
                    components.AddClone(roomConnectorR, Vector3.zero, rot, null);
                    coll_components.AddClone(coll_roomConnectorR, Vector3.zero, rot, null);
                }
            }
            else
            {
                if (conn == LevelTile.ConnectionState.Connect)
                {
                    components.AddClone(roomDoorway, Vector3.zero, rot, null);
                    coll_components.AddClone(coll_roomDoorway, Vector3.zero, rot, null);
                }
                else
                {
                    components.AddClone(roomWall, Vector3.zero, rot, null);
                    coll_components.AddClone(coll_roomWall, Vector3.zero, rot, null);
                }

                if (connR != LevelTile.ConnectionState.Merge)
                    components.AddClone(roomWallCorner, Vector3.zero, rot, null);
            }
            iters++;
        }
        return (components, coll_components);
    }
    
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
            new KeyValuePair<int, float>(5, r[5]),
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
        if (debug != null && debug[1])
            Debug.Log("Generating corridor tile mesh with the following connections:\n" + connections.ToString());

        List<GameObject> components, coll_components;
        (components, coll_components) = CorridorTileMeshComponents(connections, debug != null && debug[4]);

        int i, nComponents = components.Count;
        string compVertCounts = nComponents > 0 ? "Vertex count per component:" : "Vertex count per component:\n < No components >";
        if (debug != null && debug[5])
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
        Mesh meshOut = Ext_Mesh.MergeFrom(components, true);
        Mesh coll_meshOut = Ext_Mesh.MergeFrom(coll_components, true);

        if (debug != null && debug[3])
        {
            if (debug != null && debug[5])
                Debug.Log("[Corridor] " + nComponents + " components " + (debug != null && debug[0] ? " --> " + meshOut.vertexCount + " vertices" : "generated for mesh") + "\n" + compVertCounts);
            else
                Debug.Log("[Corridor] " + nComponents + " components generated for mesh");
        }
        else if (debug != null && debug[5])
        {
            Debug.Log(compVertCounts);
        }

        return (meshOut, coll_meshOut);
    }
    public (Mesh, Mesh) CorridorTileMesh(Vec2Int tilePos) => CorridorTileMesh(TileGrid[tilePos].connections);

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
                emptyAdj[i] = TileGrid.EmptyAt(adj[i]);
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
        return Ext_Mesh.MergeFrom(components, true);
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
                obj.GetComponent<MeshFilter>().sharedMesh = Ext_Mesh.MergeFrom(components, false);
                obj.GetComponent<MeshRenderer>().sharedMaterial = Ext_Material.DefaultDiffuse;
                foreach (GameObject cObj in components)
                    cObj.transform.SetParent(obj.transform, false);
                components.Clear();
            }
        }
    }*/
}

#if UNITY_EDITOR
[CustomEditor(typeof(Generation))]
[CanEditMultipleObjects]
public class GenerationEditor : Editor
{
    private Generation targ { get { return target as Generation; } }
    private Rect rect, toggleRect;
    private GUIContent label = new GUIContent() { tooltip = null };

    private static bool showObjects;
    private static bool showComponents;
    private static bool showCollComponents;
    private static bool showMaterials;
    private static bool showSettings;
    private static bool showDebug;

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
                    label.text = "Map Transform";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("mapTransform"), label);
                    label.text = "Entity Transform";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("entityTransform"), label);
                    label.text = "Object Transform";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("objectTransform"), label);
                    label.text = "Minimap Transform";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("miniMapTransform"), label);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Templates");
                    label.text = "Tile Template";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tileTemplate"), label);
                    label.text = "Spawn Point";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnPoint"), label);
                    label.text = "End Target";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("endTarget"), label);
                    label.text = "World Enemy";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("worldEnemy"), label);
                    /*label.text = "Path Anchor";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyPathAnchor"), label);*/
                    label.text = "World Item";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("worldItem"), label);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);

            if (showComponents = EditorGUILayout.Foldout(showComponents, new GUIContent("Component Mesh Objects"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    EditorElements.SectionHeader("Corridor");
                    label.text = "60° Inner";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("corridorConn060"), label);
                    label.text = "60° Outer";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("corridorConn300"), label);
                    label.text = "120° Inner";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("corridorConn120"), label);
                    label.text = "120° Outer";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("corridorConn240"), label);
                    label.text = "Straight";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("corridorConn180"), label);
                    label.text = "End";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("corridorConn360"), label);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Room");
                    label.text = "Wall";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("roomWall"), label);
                    label.text = "Doorway";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("roomDoorway"), label);
                    label.text = "Wall Corner";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("roomWallCorner"), label);
                    label.text = "Connector (Left)";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("roomConnectorL"), label);
                    label.text = "Connector (Right)";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("roomConnectorR"), label);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Floor");
                    label.text = "Inner";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floorInner"), label);
                    label.text = "Edge";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floorEdge"), label);
                    label.text = "Left To Edge";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floorLeftI2E"), label);
                    label.text = "Right To Edge";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floorRightI2E"), label);
                    label.text = "Both To Edge";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floorBothI2E"), label);
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
                    label.text = "60° Inner";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_corridorConn060"), label);
                    label.text = "60° Outer";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_corridorConn300"), label);
                    label.text = "120° Inner";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_corridorConn120"), label);
                    label.text = "120° Outer";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_corridorConn240"), label);
                    label.text = "Straight";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_corridorConn180"), label);
                    label.text = "End";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_corridorConn360"), label);
                    EditorGUILayout.Space(4);

                    EditorElements.SectionHeader("Room");
                    label.text = "Wall";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_roomWall"), label);
                    label.text = "Doorway";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_roomDoorway"), label);
                    label.text = "Connector (Left)";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_roomConnectorL"), label);
                    label.text = "Connector (Right)";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coll_roomConnectorR"), label);
                    EditorGUILayout.Space(0);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);

            if (showMaterials = EditorGUILayout.Foldout(showMaterials, new GUIContent("Materials"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    label.text = "Wall (Room)";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("matWallRoom"), label);
                    label.text = "Wall (Corridor)";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("matWallCorridor"), label);
                    label.text = "Wall (Minimap)";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("matMiniMap"), label);
                    label.text = "Floor";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("matFloor"), label);
                    label.text = "Door";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("matDoor"), label);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(6);

            if (showSettings = EditorGUILayout.Foldout(showSettings, new GUIContent("Generation Settings"), EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    label.text = "Tile Radius";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tileRadius"), label);
                    label.text = "Internal Tile Radius";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("tileInnerRadius"), label);
                    label.text = "Switch Radius Axis";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("switchRadiusAxis"), label);
                    label.text = "Measure To Corner";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("radiusToCorner"), label);
                    /*label.text = "Mesh Rotation";
                    rect = EditorElements.PrefixLabel(label);
                    targ.meshRotation = EditorElements.Slider(rect, targ.meshRotation, 0, 360);
                    EditorGUILayout.Space(1);
                    label.text = "Room Merge Chance";
                    rect = EditorElements.PrefixLabel(label);
                    targ.mergeRooms = EditorElements.PercentSlider(rect, targ.mergeRooms, 2, false);
                    EditorGUILayout.Space(1);*/
                    label.text = "Wall Height";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallHeight"), label);
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
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

public static class GenerationUtility
{
    private static Generation.Grid TileGrid => Core.LevelManager.TileGrid;
    private static Generation Generator => Core.LevelManager.Generator;

    public static List<LevelArea.AreaID> GetIDs<T>(this List<T> areas) where T : LevelArea
    {
        List<LevelArea.AreaID> IDs = new List<LevelArea.AreaID>();
        foreach (LevelArea area in areas)
        {
            IDs.Add(area.ID);
        }
        return IDs;
    }

    public static List<AdjacentRef> Adjacent(this Vec2Int position, LevelTile.TileType tileType)
    {
        List<AdjacentRef> selAdj = new List<AdjacentRef>();
        HexGridDirection dir;
        Vec2Int pos;
        LevelTile tile;
        for (int i = 0; i < 6; i++)
        {
            dir = (HexGridDirection)i;
            pos = HexGrid2D.Adjacent(position, dir);
            tile = TileGrid[pos];
            if (tile == null)
            {
                if (tileType == LevelTile.TileType.None)
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

    public static (List<AdjacentRef>, List<AdjacentRef>, List<AdjacentRef>, List<AdjacentRef>) CategorisedAdjacent(this Vec2Int position)
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
            tile = TileGrid[pos];
            LevelTile.TileType type = tile == null ? LevelTile.TileType.None : tile.type;
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

    public static Vector3 TilePosition(this Vec2Int gridPosition)
    {
        Vector3 vect = Vector3.zero;
        vect.x = (float)gridPosition.x * 1.5f * Generator.xOffset;
        vect.z = ((float)gridPosition.y * 2.0f + (gridPosition.x % 2 == 0 ? 0.0f : 1.0f)) * Generator.zOffset;
        return vect;
    }

    public static void ReParentTiles(this LevelArea area)
    {
        foreach (LevelTile tile in area.tiles)
        {
            tile.transform.SetParent(area.transform, true);
        }
    }
    public static void ReParentTiles(this ICollection<LevelArea> areas)
    {
        foreach (LevelArea area in areas)
        {
            area.ReParentTiles();
        }
    }
    public static void ReParentTiles(this LevelRoom area)
    {
        foreach (LevelTile tile in area.tiles)
        {
            tile.transform.SetParent(area.transform, true);
        }
    }
    public static void ReParentTiles(this ICollection<LevelRoom> areas)
    {
        foreach (LevelArea area in areas)
        {
            area.ReParentTiles();
        }
    }
    public static void ReParentTiles(this LevelCorridor area)
    {
        foreach (LevelTile tile in area.tiles)
        {
            tile.transform.SetParent(area.transform, true);
        }
    }
    public static void ReParentTiles(this ICollection<LevelCorridor> areas)
    {
        foreach (LevelArea area in areas)
        {
            area.ReParentTiles();
        }
    }
}

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

public class MapGenSaveData
{
    private const char info = ';';
    private const char tile = '&';

    public int stageIndex;
    public TileSaveData[] tiles;
    public RoomSaveData[] rooms;
    public CorridorSaveData[] corridors;
    public string[] DataStrings => GetDataStrings(5);

    public MapGenSaveData(int stageIndex, ICollection<TileSaveData> tiles, ICollection<RoomSaveData> rooms, ICollection<CorridorSaveData> corridors)
    {
        this.stageIndex = stageIndex;
        this.tiles = tiles.ToArray();
        this.rooms = rooms.ToArray();
        this.corridors = corridors.ToArray();
    }
    public MapGenSaveData(int stageIndex, Generation.Grid grid, ICollection<LevelRoom> rooms, ICollection<LevelCorridor> corridors)
    {
        this.stageIndex = stageIndex;
        tiles = grid.GetTiles().GetSaveData();
        this.rooms = rooms.GetSaveData();
        this.corridors = corridors.GetSaveData();
    }

    public static MapGenSaveData FromDataStrings(string[] dataStrings)
    {
        List<TileSaveData> tiles = new List<TileSaveData>();
        List<RoomSaveData> rooms = new List<RoomSaveData>();
        List<CorridorSaveData> corridors = new List<CorridorSaveData>();

        IntRange tileLines = IntRange.Max, roomLines = IntRange.Max, corridorLines = new IntRange(0, dataStrings.Length - 1);
        int i, search = 0, delim = dataStrings[0].IndexOf(info);
        int stageIndex = dataStrings[0].RangeToInt(search, delim);

        search = delim + 1; delim = dataStrings[0].IndexOf(info, search);
        tileLines.lower = dataStrings[0].RangeToInt(search, delim);

        search = delim + 1; delim = dataStrings[0].IndexOf(info, search);
        tileLines.upper = dataStrings[0].RangeToInt(search, delim);

        search = delim + 1; delim = dataStrings[0].IndexOf(info, search);
        roomLines.lower = dataStrings[0].RangeToInt(search, delim);

        search = delim + 1; delim = dataStrings[0].IndexOf(info, search);
        roomLines.upper = dataStrings[0].RangeToInt(search, delim);

        search = delim + 1; delim = dataStrings[0].IndexOf(info, search);
        corridorLines.lower = dataStrings[0].RangeToInt(search, delim);

        for (i = tileLines.lower; i <= tileLines.upper; i++)
        {
            search = 0;
            string line = dataStrings[i];
            while (search < line.Length)
            {
                delim = line.IndexOf(tile, search);
                tiles.Add(TileSaveData.FromDataString(line[search..delim]));
                search = delim + 1;
            }
        }

        for (i = roomLines.lower; i <= roomLines.upper; i++)
        {
            rooms.Add(RoomSaveData.FromDataString(dataStrings[i]));
        }
        for (i = corridorLines.lower; i <= corridorLines.upper; i++)
        {
            corridors.Add(CorridorSaveData.FromDataString(dataStrings[i]));
        }

        return new MapGenSaveData(stageIndex, tiles, rooms, corridors);
    }
    public string[] GetDataStrings(int tilesPerLine)
    {
        int i, j = 0, k;
        int tileLines = Ext_Int.MinSets(tilesPerLine, tiles.Length);
        int tStart = 1, tEnd = tStart + tileLines - 1;
        int rStart = tEnd + 1, rEnd = tEnd + rooms.Length;
        int cStart = rEnd + 1, cEnd = rEnd + corridors.Length;

        string lineInfo = stageIndex.ToString() + info + tStart + info + tEnd + info + rStart + info + rEnd + info + cStart + info/* + cEnd + info*/;
        List<string> result = new List<string>() { lineInfo };

        for (i = 0; i < tileLines; i++)
        {
            string str = "";
            for (k = 0; k < tilesPerLine; k++, j++)
            {
                if (j >= tiles.Length)
                    break;
                str += "" + tiles[j].DataString + tile;
            }
            result.Add(str);
        }
        for (i = 0; i < rooms.Length; i++)
        {
            result.Add(rooms[i].DataString);
        }
        for (i = 0; i < corridors.Length; i++)
        {
            result.Add(corridors[i].DataString);
        }
        return result.ToArray();
    }
}

public class LevelPopSaveData
{
    private const char item = ',';
    private const char info = ';';
    private const char sect = '|';

    public Vector2 spawnPosition;
    public Vector2 endPosition;
    public EnemySetSaveData[] enemySetData;
    public WorldItemSaveData[] itemData;
    public string[] DataStrings => GetDataStrings();

    public LevelPopSaveData(Vector2 spawnPosition, Vector2 endPosition, IList<EnemySetSaveData> enemySetData, IList<WorldItemSaveData> itemData)
    {
        this.spawnPosition = spawnPosition;
        this.endPosition = endPosition;
        this.enemySetData = enemySetData.ToArray();
        this.itemData = itemData.ToArray();
    }
    public LevelPopSaveData(Vector3 spawnPosition, Vector3 endPosition, IList<WorldEnemySet> sets, IList<WorldItem> items)
    {
        this.spawnPosition = new Vector2(spawnPosition.x, spawnPosition.z);
        this.endPosition = new Vector2(endPosition.x, endPosition.z);
        enemySetData = new EnemySetSaveData[sets.Count];
        for (int i = 0; i < sets.Count; i++)
        {
            enemySetData[i] = new EnemySetSaveData(sets[i]);
        }
        itemData = new WorldItemSaveData[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            itemData[i] = new WorldItemSaveData(items[i]);
        }
    }

    public static LevelPopSaveData FromDataStrings(string[] dataStrings)
    {
        Vector2 spawnPosition = Vector2.zero, endPosition = Vector2.zero, itemPosition = Vector2.zero;
        List<EnemySetSaveData> enemySetData = new List<EnemySetSaveData>();
        List<WorldItemSaveData> itemData = new List<WorldItemSaveData>();

        int type, id, rarity, yaw, search = 0, comma = dataStrings[0].IndexOf(item), delim, section = dataStrings[0].IndexOf(sect);

        spawnPosition.x = dataStrings[0].RangeToFloat(search, comma);
        spawnPosition.y = dataStrings[0].RangeToFloat(comma + 1, section);
        search = section + 1; comma = dataStrings[0].IndexOf(item, search); section = dataStrings[0].IndexOf(sect, search);
        endPosition.x = dataStrings[0].RangeToFloat(search, comma);
        endPosition.y = dataStrings[0].RangeToFloat(comma + 1, section);

        search = 0;
        List<int> inds = new List<int>();
        while (search < dataStrings[1].Length)
        {
            delim = dataStrings[1].IndexOf(info, search);
            type = dataStrings[1].RangeToInt(search, delim);
            search = delim + 1; delim = dataStrings[1].IndexOf(info, search);
            id = dataStrings[1].RangeToInt(search, delim);
            search = delim + 1; section = dataStrings[1].IndexOf(sect, search);
            while (search < section)
            {
                comma = dataStrings[1].IndexOf(item, search);
                inds.Add(dataStrings[1].RangeToInt(search, comma));
                search = comma + 1;
            }
            enemySetData.Add(new EnemySetSaveData((LevelArea.AreaType)type, id, inds));
            inds.Clear();
            search = section + 1;
        }

        search = 0;
        while (search < dataStrings[2].Length)
        {
            delim = dataStrings[2].IndexOf(info, search);
            type = dataStrings[2].RangeToInt(search, delim);

            search = delim + 1; delim = dataStrings[2].IndexOf(info, search);
            id = dataStrings[2].RangeToInt(search, delim);

            search = delim + 1; delim = dataStrings[2].IndexOf(info, search);
            rarity = dataStrings[2].RangeToInt(search, delim);

            search = delim + 1; comma = dataStrings[2].IndexOf(item, search);
            itemPosition.x = dataStrings[2].RangeToFloat(search, comma);

            search = comma + 1; delim = dataStrings[2].IndexOf(info, search);
            itemPosition.y = dataStrings[2].RangeToFloat(search, delim);

            search = delim + 1; section = dataStrings[2].IndexOf(sect, search);
            yaw = dataStrings[2].RangeToInt(search, section);

            itemData.Add(new WorldItemSaveData((LevelArea.AreaType)type, id, (ItemRarity)rarity, itemPosition, yaw));
            search = section + 1;
        }

        return new LevelPopSaveData(spawnPosition, endPosition, enemySetData, itemData);
    }
    public string[] GetDataStrings()
    {
        string[] result = new string[3];
        result[0] = "" + spawnPosition.x.ToString() + item + spawnPosition.y.ToString() + sect + endPosition.x.ToString() + item + endPosition.y.ToString() + sect;
        int i;
        foreach (EnemySetSaveData setData in enemySetData)
        {
            result[1] += "" + (int)setData.areaType + info + setData.areaID + info;
            for (i = 0; i < setData.enemyDataInds.Length; i++)
            {
                result[1] += "" + setData.enemyDataInds[i] + item;
            }
            result[1] += "" + sect;
        }
        foreach (WorldItemSaveData data in itemData)
        {
            result[2] += "" + (int)data.areaType + info + data.areaID + info + (int)data.rarity + info + data.position.x.ToString() + item + data.position.x.ToString() + info + data.yaw + sect;
        }
        return result;
    }
}

public struct EnemySetSaveData
{
    public LevelArea.AreaType areaType;
    public int areaID;
    public int[] enemyDataInds;

    public EnemySetSaveData(LevelArea.AreaType areaType, int areaID, ICollection<int> enemyDataInds)
    {
        this.areaType = areaType;
        this.areaID = areaID;
        this.enemyDataInds = enemyDataInds.ToArray();
    }
    public EnemySetSaveData(WorldEnemySet set)
    {
        areaType = set.area.ID.type;
        areaID = set.area.ID.value;
        enemyDataInds = set.dataIndices;
    }
}

public struct WorldItemSaveData
{
    public LevelArea.AreaType areaType;
    public int areaID;
    public ItemRarity rarity;
    public Vector2 position;
    public int yaw;

    public WorldItemSaveData(LevelArea.AreaType areaType, int areaID, ItemRarity rarity, Vector2 position, int yaw)
    {
        this.areaType = areaType;
        this.areaID = areaID;
        this.rarity = rarity;
        this.position = position;
        this.yaw = yaw;
    }
    public WorldItemSaveData(WorldItem source)
    {
        areaType = source.areaID.type;
        areaID = source.areaID.value;
        rarity = source.rarity;
        position = new Vector2(source.position.x, source.position.z);
        yaw = source.yaw;
    }
}

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

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
