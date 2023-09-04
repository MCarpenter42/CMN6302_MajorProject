using NeoCambion;
using NeoCambion.Collections;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Generation;

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

public class LevelCorridor : Core, LevelArea
{
    public Generation generator { get; private set; }
    public LevelRoom sourceRoom { get; private set; }

    public static LevelTile.TileType tNone = LevelTile.TileType.None;
    public static LevelTile.TileType tEmpty = LevelTile.TileType.Empty;
    public static LevelTile.TileType tCorridor = LevelTile.TileType.Corridor;
    public static LevelTile.TileType tRoom = LevelTile.TileType.Room;

    public int ID { get; private set; }

    public CorridorBranch mainBranch => branches.HasContents() ? branches[0] : null;
    private List<CorridorBranch> branches = null;
    private CorridorBranch GetBranch(int index) => branches.InBounds(index) ? branches[index] : null;

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
    public List<LevelTile> tiles { get { return _tiles; } }
    private List<KeyValuePair<int, int>> internalConns = new List<KeyValuePair<int, int>>();

    public bool terminated => mainBranch.terminated;
    public bool fullTerminated => mainBranch.fullTerminated;

    public bool revealed { get; private set; }

    /*public bool containsEnemy { get; set; }
    public int enemyCount => containsEnemy ? 1 : 0;*/
    public bool containsEnemy { get { return true; } set { } }
    public int enemyCount => 0;
    //private WorldEnemySet _enemies = null;
    public WorldEnemySet enemies { get; private set; }
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
    public int itemCapacity
    {
        get
        {
            return ValidItemPlacements().Length;
        }
    }
    //private WorldItem[] _items = null;
    public WorldItem[] items { get; private set; }
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

    public int[] ValidItemPlacements()
    {
        List<int> valid = new List<int>();
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].isCorridorEndcap)
                valid.Add(i);
        }
        return valid.ToArray();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Initialise(Generation generator, LevelRoom sourceRoom, int ID, Vec2Int startTile, HexGridDirection firstStep, float initialBranchChance = 0.3f)
    {
        this.generator = generator;
        this.sourceRoom = sourceRoom;
        this.ID = ID;
        if (branches == null)
            branches = new List<CorridorBranch>();
        branches.ReturnAdd(new CorridorBranch(this, 0, startTile, HexGridDirection.INVALID)).AddStep(firstStep);
        branchChance = initialBranchChance;
        _tiles = new List<LevelTile>();
        revealed = false;
        containsEnemy = false;
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

    public void AddTile(LevelTile newTile)
    {
        tiles.Add(newTile);
        newTile.SetTransformParent(this);
    }

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
                posF = generator.TilePosition(pos);
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

    public Vector3 RandInternalPosition()
    {

        return Vector3.zero;
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
            tile.transform.position = generator.TilePosition(tile.gridPosition);
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

    public (PositionsInArea, PositionsInArea) SpawnPositions(int indexOverride = -1)
    {
        int indToUse = indexOverride >= 0 ? indexOverride : ID;
        PositionsInArea enemyPositions = new PositionsInArea(true, indToUse, enemyCount), itemPositions = new PositionsInArea(false, indToUse, itemCount);
        for (int i = 0; i < enemyCount; i++)
        {
            enemyPositions[i] = RandInternalPosition();
        }
        for (int i = 0; i < itemCount; i++)
        {
            itemPositions[i] = RandInternalPosition();
        }
        return (enemyPositions, itemPositions);
    }

    public void OverwriteEnemies(int enemyCount)
    {
        enemies = new WorldEnemySet(enemyCount, this);
        if (LevelManager.worldEnemies.ContainsKey((false, ID)))
            LevelManager.worldEnemies[(false, ID)] = enemies;
        else
            LevelManager.worldEnemies.Add((false, ID), enemies);
    }

    public void OverwriteItems(int itemCount)
    {
        items = new WorldItem[itemCount];
        if (LevelManager.worldItems.ContainsKey((false, ID)))
            LevelManager.worldItems[(false, ID)] = items;
        else
            LevelManager.worldItems.Add((false, ID), items);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool PlayerInRange()
    {
        foreach (LevelTile tile in tiles)
        {
            if (tile.distToPlayer < generator.tileRadius)
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

    private int parentind = -1, childInd = -1;
    public CorridorBranch parent { get { return parentind > 0 ? container[parentind] : null; } }
    public CorridorBranch child { get { return childInd > 0 ? container[childInd] : null; } }
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
        this.parentind = parentInd;
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
        this.parentind = parentInd;
        childStartsAt = -1;
        if (tiles == null)
            tiles = new List<Vec2Int>();
        if (steps == null)
            steps = new List<HexGridDirection>();
        this.startTile = startTile;
        this.startDir = startDir;
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
        if (!terminated && tiles.Count > 0)
        {
            childStartsAt = tiles.Count - 1;
            return new CorridorBranch(container, childInd, tiles.Last(), steps.Last().Rotate(2), index);
        }
        return null;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public List<Vec2Int> GetTilePositions()
    {
        List<Vec2Int> tilesOut = new List<Vec2Int>();
        if (tiles.Count > 0)
            tilesOut.AddRange(tiles);
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

    private bool ConnectionFiler(Vec2Int position, bool filterOutSourceRoom) => container.Contains(position) || (filterOutSourceRoom ? container.sourceRoom?.Contains(position) : false).Value;

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
        return new AdjacentRef(HexGridDirection.INVALID, endTile.Value, tCorridor);
    }

    public AdjacentRef GetConnectionTile(Vec2Int lastPos, List<AdjacentRef> available, HexGridDirection lastDir, bool filterOutSourceRoom, bool debug = false)
    {
        for (int i = available.Count - 1; i >= 0; i--)
        {
            if (ConnectionFiler(available[i].position, filterOutSourceRoom))
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
