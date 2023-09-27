using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using NeoCambion;
using NeoCambion.Collections;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class LevelCorridor : Core, LevelArea
{
    public Generation generator { get; private set; }
    private int _sourceID = -1;
    private LevelRoom _sourceRoom;
    public LevelRoom sourceRoom
    {
        get
        {
            if (_sourceRoom == null && _sourceID >= 0)
                _sourceRoom = LevelManager.Rooms[_sourceID];
            return _sourceRoom;
        }
        set
        {
            _sourceRoom = value;
            _sourceID = _sourceRoom.ID.value;
        }
    }

    public static LevelTile.TileType tNone = LevelTile.TileType.None;
    public static LevelTile.TileType tEmpty = LevelTile.TileType.Empty;
    public static LevelTile.TileType tCorridor = LevelTile.TileType.Corridor;
    public static LevelTile.TileType tRoom = LevelTile.TileType.Room;

    public LevelArea.AreaID ID { get; private set; }

    public CorridorBranch mainBranch => branches.HasContents() ? branches[0] : null;
    public List<CorridorBranch> branches { get; private set; }
    public CorridorBranch GetBranch(int index) => branches.InBounds(index) ? branches[index] : null;

    public float branchChance { get; private set; }

    public Vec2Int? startTile => branches.HasContents() ? branches[0].startTile : null;
    private List<Vec2Int> _tilePositions = null;
    public List<Vec2Int> tilePositions => _tilePositions ?? (terminated ? _tilePositions = mainBranch.GetTilePositions() : mainBranch.GetTilePositions());
    public bool Contains(Vec2Int toCheck)
    {
        foreach (Vec2Int tile in tilePositions)
        {
            if (toCheck == tile)
                return true;
        }
        return false;
    }

    private List<LevelTile> _tiles = null;
    public List<LevelTile> tiles => _tiles ?? (terminated ? GetTiles(true) : GetTiles(false));
    public List<LevelTile> GetTiles(bool overwrite = false)
    {
        List<LevelTile> tiles = new List<LevelTile>();
        additionalInfo.Clear();
        foreach (Vec2Int tilePos in tilePositions)
        {
            additionalInfo.Add(tilePos.ToString());
            tiles.AddUnlessNull(LevelManager.TileGrid[tilePos]);
        }
        if (overwrite)
            _tiles = tiles;
        return tiles;
    }
    public void AddTiles(IList<LevelTile> tiles) => tiles.TransferAllUnique(_tiles);

    public bool tileCountMismatch => tiles.Count > tilePositions.Count;

    public bool terminated => mainBranch.terminated;
    public bool fullTerminated => mainBranch.fullTerminated;

    public bool revealed { get; private set; }

    /*public bool containsEnemy { get; set; }
    public int enemyCount => containsEnemy ? 1 : 0;*/
    public bool containsEnemy { get { return true; } set { } }
    public int enemyCount => 0;
    //private WorldEnemySet _enemies = null;
    public WorldEnemySet enemies { get; set; }
    /*{
        get
        {
            if (containsEnemy)
            {
                if (_enemies == null)
                {
                    _enemies = new WorldEnemySet(1);
                    LevelManager.worldEnemies.Add(_enemies);
                }
                return _enemies;
            }
            else
                return null;
        }
    }*/
    public int itemCount { get; set; }
    public int itemCapacity => ValidItemPlacements.Length;
    //private WorldItem[] _items = null;
    public List<WorldItem> items { get; set; }
    /*{
        get
        {
            if (itemCount > 0)
            {
                if (_items == null)
                    _items = new WorldItem[itemCount];
                return _items;
            }
            else
                return null;
        }
    }*/

    public Vector3[] GetValidItemPositions()
    {
        List<Vector3> valid = new List<Vector3>();
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].isCorridorEndcap)
                valid.Add(tiles[i].position);
        }
        return valid.ToArray();
    }
    public int[] ValidItemPlacements
    {
        get
        {
            List<int> valid = new List<int>();
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].isCorridorEndcap)
                    valid.Add(i);
            }
            return valid.ToArray();
        }
    }

    public List<string> additionalInfo { get; private set; } = new List<string>();

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Initialise(Generation generator, int sourceRoomID, int ID, Vec2Int startTile, HexGridDirection firstStep, float initialBranchChance = 0.3f)
    {
        gameObject.name = "Corridor " + ID;
        this.generator = generator;
        _sourceID = sourceRoomID;
        this.ID = LevelArea.CorridorID(ID);
        if (branches == null)
            branches = new List<CorridorBranch>();
        branches.ReturnAdd(new CorridorBranch(this, 0, startTile, HexGridDirection.INVALID)).AddStep(firstStep);
        branchChance = initialBranchChance;
        _tiles = new List<LevelTile>();
        revealed = false;
        containsEnemy = false;
    }
    public void Initialise(Generation generator, LevelRoom sourceRoom, int ID, Vec2Int startTile, HexGridDirection firstStep, float initialBranchChance = 0.3f)
    {
        gameObject.name = "Corridor " + ID;
        this.generator = generator;
        this.sourceRoom = sourceRoom;
        this.ID = LevelArea.CorridorID(ID);
        if (branches == null)
            branches = new List<CorridorBranch>();
        branches.ReturnAdd(new CorridorBranch(this, 0, startTile, HexGridDirection.INVALID)).AddStep(firstStep);
        branchChance = initialBranchChance;
        _tiles = new List<LevelTile>();
        revealed = false;
        containsEnemy = false;
    }
    public void Initialise(Generation generator, CorridorSaveData existingData, float initialBranchChance = 0.3f)
    {
        gameObject.name = "Corridor " + existingData.ID;
        this.generator = generator;
        _sourceID = existingData.sourceRoomID;
        ID = LevelArea.CorridorID(existingData.ID);
        branches = new List<CorridorBranch>();
        for (int i = 0; i < existingData.branches.Length; i++)
        {
            branches.Add(new CorridorBranch(this, i, existingData.branches[i]));
        }
        branchChance = initialBranchChance;
        _tilePositions = mainBranch.GetTilePositions();
        _tiles = GetTiles();
        revealed = existingData.revealed;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public CorridorBranch this[int index]
    {
        get { return GetBranch(index); }
        set { if (branches.InBounds(index)) branches[index] = value; }
    }

    private class LevelCorridorEnumerator : IEnumerator
    {
        List<CorridorBranch> branches;
        int position = -1;

        public LevelCorridorEnumerator(List<CorridorBranch> branches)
        {
            this.branches = branches;
        }

        public bool MoveNext() => ++position < branches.Count;
        public void Reset() => position = -1;
        public object Current
        {
            get
            {
                try { return branches[position]; }
                catch (System.IndexOutOfRangeException) { throw new System.InvalidOperationException(); }
            }
        }
    }
    public IEnumerator GetEnumerator() => new LevelCorridorEnumerator(branches);

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Update()
    {
        if (!revealed & PlayerInRange())
        {
            revealed = true;
            ShowOnMiniMap();
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public Vector3 CentrepointWorld()
    {
        if (tilePositions.Count == 0)
            return Vector3.zero;
        else
        {
            float xTotal = 0, zTotal = 0;
            Vector3 posF;
            foreach (Vec2Int pos in tilePositions)
            {
                posF = pos.TilePosition();
                xTotal += posF.x;
                zTotal += posF.z;
            }
            float x = xTotal / tilePositions.Count;
            float z = zTotal / tilePositions.Count;
            return new Vector3(x, 0, z);
        }
    }

    public Vec2Int Centrepoint()
    {
        if (tilePositions.Count == 0)
            return Vec2Int.zero;
        else
        {
            int xTotal = 0, yTotal = 0;
            foreach (Vec2Int pos in tilePositions)
            {
                xTotal += pos.x;
                yTotal += pos.y;
            }
            float xF = xTotal / tilePositions.Count;
            float yF = yTotal / tilePositions.Count;
            return new Vec2Int(Mathf.RoundToInt(xF), Mathf.RoundToInt(yF));
        }
    }

    private static Vector3[] hexDirVects = new Vector3[]
    {
        new Vector3(+0.00000f, 0f, +1.000f), new Vector3(+0.86602f, 0f, +0.50000f),
        new Vector3(+0.86602f, 0f, -0.500f), new Vector3(+0.00000f, 0f, -1.00000f),
        new Vector3(-0.86602f, 0f, -0.500f), new Vector3(-0.86602f, 0f, +0.50000f),
    };
    private bool placingItem = false;
    public Vector3 RandInternalPosition()
    {
        float fR = Random.Range(0f, 1f);
        LevelTile tile;
        Vector3[] itemPositions = GetValidItemPositions();
        if (placingItem || fR > 0.8f && itemPositions.Length > 0)
            return itemPositions[Random.Range(0, itemPositions.Length)];
        else
            tile = tiles[Random.Range(0, tiles.Count)];
        List<int> validDirs = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            if (tile.connections.values[i] == LevelTile.ConnectionState.Connect)
                validDirs.Add(i);
        }
        Vector3 dir = hexDirVects[validDirs[Random.Range(0, validDirs.Count)]];
        return tile.position + dir * Random.Range(0f, LevelManager.Generator.tileRadius);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SplitBranch(int index)
    {
        CorridorBranch branch = GetBranch(index), newBranch = null;
        if (branch != null && !branch.hasChild)
            newBranch = branch.Split(branches.Count);
        if (newBranch != null)
        {
            branches.Add(newBranch);
            branchChance *= 0.5f;
        }
    }

    public List<Vec2Int> GetEndTiles() => mainBranch.GetEndTiles();

    public List<RoomSourceRef> GetRoomStarts()
    {
        List<RoomSourceRef> sourceRefs = new List<RoomSourceRef>();
        for (int i = 0; i < branches.Count; i++)
        {
            if (branches[i] == null)
                Debug.Log("Corridor " + ID + " - Null branch found at " + (i + 1) + "/" + branches.Count);
            else if (branches[i].terminated && branches[i].terminateType == CorridorBranch.Termination.NewRoom)
                sourceRefs.Add(new RoomSourceRef(this, i));
        }
        return sourceRefs;
    }

    public void UpdatePositions()
    {
        Vector3 c = CentrepointWorld();
        transform.position = c;
        foreach (LevelTile tile in tiles)
        {
            tile.transform.position = tile.gridPosition.TilePosition();
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool IncrementItemCount()
    {
        if (itemCount < itemCapacity)
        {
            itemCount++;
        }
        return itemCount < itemCapacity;
    }

    public (Vector3[], Vector3[]) SpawnPositions()
    {
        Vector3[] enemyPositions = new Vector3[enemyCount], itemPositions = new Vector3[itemCount];
        placingItem = false;
        for (int i = 0; i < enemyCount; i++)
        {
            enemyPositions[i] = RandInternalPosition();
        }
        placingItem = true;
        for (int i = 0; i < itemCount; i++)
        {
            itemPositions[i] = RandInternalPosition();
        }
        placingItem = false;
        return (enemyPositions, itemPositions);
    }
    public (PositionsInArea, PositionsInArea) SpawnPositionsInArea(int indexOverride = -1)
    {
        int indToUse = indexOverride >= 0 ? indexOverride : ID.value;
        PositionsInArea enemyPositions = new PositionsInArea(LevelArea.CorridorID(indToUse), enemyCount), itemPositions = new PositionsInArea(LevelArea.CorridorID(indToUse), itemCount);
        placingItem = false;
        for (int i = 0; i < enemyCount; i++)
        {
            enemyPositions[i] = RandInternalPosition();
        }
        placingItem = true;
        for (int i = 0; i < itemCount; i++)
        {
            itemPositions[i] = RandInternalPosition();
        }
        placingItem = false;
        return (enemyPositions, itemPositions);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool PlayerInRange()
    {
        foreach (LevelTile tile in tiles)
        {
            if (tile != null && tile.distToPlayer < generator.tileRadius)
                return true;
        }
        return false;
    }

    public void ShowOnMiniMap()
    {
        foreach (LevelTile tile in tiles)
        {
            tile.ShowMiniTile(true);
        }
    }
}

public class CorridorBranch
{
    public LevelCorridor container { get; private set; }
    public int index { get; private set; }

    public enum Termination { None, NewRoom, ExistingTile, EndCap }

    public int parentInd { get; private set; }
    public int childInd { get; private set; }
    public CorridorBranch parent { get { return parentInd > -1 ? container[parentInd] : null; } }
    public CorridorBranch child { get { return childInd > -1 ? container[childInd] : null; } }
    public bool hasChild { get { return child != null; } }
    public int childStartsAt { get; private set; }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public Vec2Int startTile;
    public List<Vec2Int> tiles;
    public Vec2Int? endTile => tiles.Count > 0 ? tiles.Last() : null;
    public int Length => steps.Count;

    public HexGridDirection startDir = HexGridDirection.INVALID;
    public List<HexGridDirection> steps = null;

    public bool terminated = false;
    public bool fullTerminated
    {
        get
        {
            if (child == null)
                return terminated;
            else
                return child.fullTerminated;
        }
    }
    public Termination terminateType = Termination.None;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public CorridorBranch(LevelCorridor container, int index, int parentInd = -1)
    {
        this.container = container;
        this.index = index;
        this.parentInd = parentInd;
        childInd = -1;
        childStartsAt = -1;
        if (tiles == null)
            tiles = new List<Vec2Int>();
        if (steps == null)
            steps = new List<HexGridDirection>();
    }
    public CorridorBranch(LevelCorridor container, int index, Vec2Int startTile, HexGridDirection startDir, int parentInd = -1)
    {
        this.container = container;
        this.index = index;
        this.parentInd = parentInd;
        childInd = -1;
        childStartsAt = -1;
        if (tiles == null)
            tiles = new List<Vec2Int>();
        if (steps == null)
            steps = new List<HexGridDirection>();
        this.startTile = startTile;
        this.startDir = startDir;
    }
    public CorridorBranch(LevelCorridor container, int index, BranchSaveData existingData)
    {
        this.container = container;
        this.index = index;
        startDir = existingData.steps[0];
        steps = new List<HexGridDirection>();
        steps.AddRange(existingData.steps[1..]);
        startTile = existingData.tilePositions[0];
        tiles = new List<Vec2Int>();
        tiles.AddRange(existingData.tilePositions[1..]);
        parentInd = existingData.parentInd;
        childInd = existingData.childInd;
        childStartsAt = existingData.branchesAt;
        terminated = true;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool AddStep(HexGridDirection stepDir)
    {
        if (!terminated)
        {
            if (stepDir != HexGridDirection.INVALID)
            {
                steps.Add(stepDir);
                Vec2Int lastTile = tiles.Count > 0 ? tiles.Last() : startTile;
                tiles.Add(HexGrid2D.Adjacent(lastTile, stepDir));
                return true;
            }
        }
        return false;
    }

    public CorridorBranch Split(int childInd)
    {
        if (this.childInd < 0 && !terminated && tiles.Count > 0)
        {
            childStartsAt = tiles.Count - 1;
            return new CorridorBranch(container, childInd, tiles.Last(), steps.Last().Rotate(2), index);
        }
        return null;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public List<Vec2Int> GetTilePositions(bool includeEndTile = false)
    {
        List<Vec2Int> tilesOut = new List<Vec2Int>();
        if (tiles.Count > 0)
        {
            tilesOut.AddRange(tiles);
            if (!includeEndTile)
                tilesOut.RemoveAt(tilesOut.Count - 1);
        }
        if (child != null)
            tilesOut.AddRange(child.GetTilePositions());
        return tilesOut;
    }

    public List<Vec2Int> GetEndTiles()
    {
        List<Vec2Int> tilesOut = new List<Vec2Int>();
        if (endTile != null)
            tilesOut.Add(endTile.Value);
        if (child != null)
            tilesOut.AddRange(child.GetEndTiles());
        return tilesOut;
    }

    private bool ConnectionFilter(Vec2Int position, bool filterOutSourceRoom) => container.Contains(position) || filterOutSourceRoom ? container.sourceRoom.Contains(position) : false;

    private static int w0 = 0, w1 = 1, w2 = 3, w3 = 12;
    private static int[,] dirWeightTable = new int[6, 6]
    {  /* +0  +1  +2  +3  +4  +5  */
    /* 0 */ { w3, w2, w1, w0, w1, w2, },
    /* 1 */ { w2, w3, w2, w1, w0, w1, },
    /* 2 */ { w1, w2, w3, w2, w1, w0, },
    /* 3 */ { w0, w1, w2, w3, w2, w1, },
    /* 4 */ { w1, w0, w1, w2, w3, w2, },
    /* 5 */ { w2, w1, w0, w1, w2, w3, }
    };
    public AdjacentRef WeightedDir(Vec2Int lastPos, List<AdjacentRef> available, HexGridDirection lastDir, bool debug = false)
    {
        string debugStr = "Checking from " + lastPos.ToString() + "\nInitial direction: " + lastDir.ToString() + " (" + (int)lastDir + ")\n--> [";
        if (available != null && available.Count > 0)
        {
            int i;
            if (available.Count == 1)
                return available[0];

            if (lastDir == HexGridDirection.INVALID)
                return available[Random.Range(0, available.Count)];

            int[] weights = new int[available.Count];
            debugStr += (int)available[0].direction + "/";
            int x = dirWeightTable[(int)lastDir, (int)available[0].direction];
            debugStr += x.ToString();
            weights[0] = x;
            for (i = 1; i < available.Count; i++)
            {
                x = dirWeightTable[(int)lastDir, (int)available[i].direction];
                debugStr += ", " + (int)available[i].direction + "/" + x.ToString();
                weights[i] = weights[i - 1] + x;
            }
            float r = Random.Range(0.0f, weights.Last());
            debugStr += "]\n--> r = " + r.Round(2) + "/" + weights.Last();
            for (i = 0; i < weights.Length; i++)
            {
                if (r <= weights[i])
                {
                    debugStr += " --> " + i + " (" + (HexGridDirection)i + ")";
                    if (debug)
                        Debug.Log(debugStr);
                    return available[i];
                }
            }
        }
        return new AdjacentRef(HexGridDirection.INVALID, endTile.Value, LevelTile.TileType.Corridor);
    }

    public AdjacentRef GetConnectionTile(Vec2Int lastPos, List<AdjacentRef> available, HexGridDirection lastDir, bool filterOutSourceRoom, bool debug = false)
    {
        for (int i = available.Count - 1; i >= 0; i--)
        {
            if (ConnectionFilter(available[i].position, filterOutSourceRoom))
                available.RemoveAt(i);
        }

        if (available == null || available.Count == 0)
            return AdjacentRef.Null;
        if (available.Count == 1)
            return available[0];
        else
            return WeightedDir(lastPos, available, lastDir, debug);
    }
}

public struct CorridorSourceRef : AreaSourceRef
{
    public LevelArea source { get { return sourceRoom; } }
    public int indexInSource { get { return tileInd; } set { tileInd = value; } }

    public LevelRoom sourceRoom;
    public int tileInd;
    public HexGridDirection startDirection;

    public bool isNull { get { return source == null && indexInSource < 0; } }

    public CorridorSourceRef(LevelRoom sourceRoom, int tileInd, HexGridDirection startDirection)
    {
        this.sourceRoom = sourceRoom;
        this.tileInd = tileInd;
        this.startDirection = startDirection;
    }

    public static CorridorSourceRef Null { get { return new CorridorSourceRef(null, -1, HexGridDirection.INVALID); } }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelCorridor))]
public class LevelLevelCorridor : Editor
{
    LevelCorridor targ { get { return target as LevelCorridor; } }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStylesExtras.noMarginsNoPadding);
        {
            EditorGUILayout.BeginVertical(EditorStylesExtras.noMarginsNoPadding);
            {
                EditorGUILayout.Space(2);
                if (targ.additionalInfo != null)
                {
                    EditorElements.SectionHeader("Additional Information");
                    EditorGUILayout.Space(0);
                    for (int i = 0; i < targ.additionalInfo.Count; i++)
                    {
                        EditorGUILayout.LabelField(targ.additionalInfo[i]);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

public class CorridorSaveData
{
    private const char item = ':';
    private const char sctn = '|';

    public int ID;
    public int sourceRoomID;
    public BranchSaveData[] branches;
    public bool revealed;
    public string DataString => ToDataString();

    public CorridorSaveData(int ID, int sourceRoomID, BranchSaveData[] branches, bool revealed)
    {
        this.ID = ID;
        this.sourceRoomID = sourceRoomID;
        this.branches = branches;
        this.revealed = revealed;
    }
    public CorridorSaveData(LevelCorridor source)
    {
        ID = source.ID.value;
        sourceRoomID = source.sourceRoom.ID.value;
        branches = new BranchSaveData[source.branches.Count];
        for (int i = 0; i < branches.Length; i++)
        {
            branches[i] = new BranchSaveData(source.GetBranch(i));
        }
        revealed = source.revealed;
    }

    public static CorridorSaveData FromDataString(string dataString)
    {
        int ID, roomID, search = 0, delim, section = dataString.IndexOf(sctn);
        List<BranchSaveData> branches = new List<BranchSaveData>();
        ID = int.Parse(dataString[search..section]);
        search = section + 1;
        section = dataString.IndexOf(sctn, search);
        roomID = dataString.RangeToInt(search, section);
        search = section + 1;
        section = dataString.IndexOf(sctn, search);
        while (search < section)
        {
            delim = dataString.IndexOf(item, search);
            branches.Add(BranchSaveData.FromDataString(dataString[search..delim]));
            search = delim + 1;
        }
        return new CorridorSaveData(ID, roomID, branches.ToArray(), dataString.Last() == 'T');
    }
    public string ToDataString()
    {
        int i;
        string result = "" + ID + sctn + sourceRoomID + sctn;
        for (i = 0; i < branches.Length; i++)
        {
            result += "" + branches[i].DataString + item;
        }
        result += "" + sctn + (revealed ? 'T' : 'F');
        return result;
    }
}

public class BranchSaveData
{
    private const char vctr = ',';
    private const char item = ';';
    private const char sctn = '/';

    public Vec2Int[] tilePositions;
    public HexGridDirection[] steps;
    public int parentInd;
    public int childInd;
    public int branchesAt;
    public string DataString => ToDataString();

    public BranchSaveData(Vec2Int[] tilePositions, HexGridDirection[] steps, int parentInd = -1, int childInd = -1, int branchesAt = -1)
    {
        this.tilePositions = tilePositions;
        this.steps = steps;
        this.branchesAt = branchesAt;
        this.parentInd = parentInd;
        this.childInd = childInd;
        this.branchesAt = branchesAt;
    }
    public BranchSaveData(CorridorBranch source)
    {
        List<Vec2Int> _tilePositions = new List<Vec2Int>() { source.startTile };
        _tilePositions.AddRange(source.tiles);
        tilePositions = _tilePositions.ToArray();
        List<HexGridDirection> _steps = new List<HexGridDirection>() { source.startDir };
        _steps.AddRange(source.steps);
        steps = _steps.ToArray();
        parentInd = source.parentInd;
        childInd = source.childInd;
        branchesAt = source.childStartsAt;
    }

    public static BranchSaveData FromDataString(string dataString)
    {
        if (dataString[0] == '{')
            dataString = dataString[1..^1];

        List<Vec2Int> tilePositions = new List<Vec2Int>();
        Vec2Int nextPos = Vec2Int.zero;
        List<HexGridDirection> steps = new List<HexGridDirection>();
        int parentInd, childInd, branchesAt, search = 0, comma, delim, section = dataString.IndexOf(sctn);
        while (search < section)
        {
            comma = dataString.IndexOf(vctr, search);
            delim = dataString.IndexOf(item, search);
            nextPos.x = dataString.RangeToInt(search, comma);
            nextPos.y = dataString.RangeToInt(comma + 1, delim);
            tilePositions.Add(nextPos);
            search = delim + 1;
        }
        search = section + 1; section = dataString.IndexOf(sctn, search);
        while (search < section)
        {
            delim = dataString.IndexOf(item, search);
            steps.Add((HexGridDirection)dataString.RangeToInt(search, delim));
            search = delim + 1;
        }
        search += 1; section = dataString.IndexOf(sctn, search);
        parentInd = dataString.RangeToInt(search, section);
        search = section + 1; section = dataString.IndexOf(sctn, search);
        childInd = dataString.RangeToInt(search, section);
        search = section + 1;
        branchesAt = dataString.RangeToInt(search);
        return new BranchSaveData(tilePositions.ToArray(), steps.ToArray(), parentInd, childInd, branchesAt);
    }
    public string ToDataString()
    {
        int i;
        string result = "{";
        for (i = 0; i < tilePositions.Length; i++)
        {
            result += "" + tilePositions[i].x + vctr + tilePositions[i].y + item;
        }
        result += "" + sctn;
        for (i = 0; i < steps.Length; i++)
        {
            result += "" + (int)steps[i] + item;
        }
        return result += "" + sctn + parentInd + sctn + childInd + sctn + branchesAt + '}';
    }
}
