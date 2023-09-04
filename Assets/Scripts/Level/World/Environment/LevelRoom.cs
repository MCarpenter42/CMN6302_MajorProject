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
using Unity.VisualScripting;

public class LevelRoom : Core, LevelArea
{
    private Generation generator;

    public static LevelTile.TileType tNone = LevelTile.TileType.None;
    public static LevelTile.TileType tEmpty = LevelTile.TileType.Empty;
    public static LevelTile.TileType tCorridor = LevelTile.TileType.Corridor;
    public static LevelTile.TileType tRoom = LevelTile.TileType.Room;

    public int ID { get; private set; }
    private List<Vec2Int> _tilePositions = null;
    public List<Vec2Int> tilePositions { get { return _tilePositions ?? (_tilePositions = new List<Vec2Int>()); } private set { _tilePositions = value; } }
    private List<KeyValuePair<int, int>> internalConns = new List<KeyValuePair<int, int>>();
    private Vec2Int boundsMin = Vec2Int.zero;
    private Vec2Int boundsMax = Vec2Int.zero;
    public int Size => tilePositions == null ? 0 : tilePositions.Count;

    private List<LevelTile> _tiles = null;
    public List<LevelTile> tiles { get { return _tiles; } }

    public bool isStartRoom = false;
    public bool isEndRoom = false;

    public bool revealed { get; private set; }

    public bool containsEnemy { get; set; }
    private int _enemyCount = -1;
    public int enemyCount
    {
        get
        {
            if (containsEnemy)
            {
                if (_enemyCount == -1)
                {
                    _enemyCount = EnemySpawnCount();

                }
                return _enemyCount;
            }
            else
                return 0;
        }
    }
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
            if (Size <= 0)
                return 0;
            else if (Size <= 2)
                return 1;
            else if (Size == 3)
                return 2;
            else if (Size == 4)
                return 3;
            else
                return 5;
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

    public void Initialise(Generation generatior, int ID)
    {
        this.generator = generatior;
        this.ID = ID;
        tilePositions = new List<Vec2Int>();
        _tiles = new List<LevelTile>();
        revealed = false;
        containsEnemy = false;
    }

    public void InternalConnections()
    {
        int i, j, adjInd;
        Vec2Int pos, adj;
        LevelTile tile;
        HexGridDirection dir;
        for (i = 0; i < tilePositions.Count; i++)
        {
            pos = tilePositions[i];
            tile = generator.tiles[pos];
            if (tile != null)
            {
                for (j = 0; j < 6; j++)
                {
                    dir = (HexGridDirection)j;
                    if (tile.connections[dir] == LevelTile.ConnectionState.None)
                    {
                        adj = HexGrid2D.Adjacent(pos, dir);
                        adjInd = IndexOf(adj);
                        if (adjInd > -1)
                        {
                            tile.connections[dir] = LevelTile.ConnectionState.Merge;
                            generator.tiles[adj].connections[dir.Invert()] = LevelTile.ConnectionState.Merge;
                            internalConns.Add(new KeyValuePair<int, int>(i, adjInd));
                        }
                        else
                            tile.connections[dir] = LevelTile.ConnectionState.Block;
                    }
                }
                tile.AddAdditionalInfo("Connections: " + tile.connections.ToString());
            }
            else
            {
                Debug.LogError("Room tile merge error at index " + i + ": tile at corresponding position " + pos.ToString() + " is null!");
            }
        }
    }

    public void DebugInternalConnections()
    {
        int i, nTiles = tilePositions.Count;
        string[] connsBefore = new string[nTiles];
        string[] connsAfter = new string[nTiles];
        Vec2Int pos;
        LevelTile tile;
        for (i = 0; i < nTiles; i++)
        {
            pos = tilePositions[i];
            tile = generator.tiles[pos];
            if (tile == null)
                connsBefore[i] = i + " - " + pos + ": NULL";
            else
                connsBefore[i] = i + " - " + pos + ": " + tile.connections.ToString();
        }
        InternalConnections();
        for (i = 0; i < nTiles; i++)
        {
            pos = tilePositions[i];
            tile = generator.tiles[pos];
            if (tile == null)
                connsAfter[i] = i + " - " + pos + ": NULL";
            else
                connsAfter[i] = i + " - " + pos + ": " + tile.connections.ToString();
        }
        string logStr = "Room ID: " + ID + " | Size: " + Size;
        for (i = 0; i < nTiles; i++)
        {
            logStr += "\n" + connsBefore[i];
            logStr += "\n" + connsAfter[i];
        }
        Debug.Log(logStr);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private class LevelRoomEnumerator : IEnumerator
    {
        List<Vec2Int> tilePositions;
        int position = -1;

        public LevelRoomEnumerator(List<Vec2Int> tilePositions)
        {
            this.tilePositions = tilePositions;
        }

        public bool MoveNext() => ++position < tilePositions.Count;
        public void Reset() => position = -1;
        public object Current
        {
            get
            {
                try { return tilePositions[position]; }
                catch (System.IndexOutOfRangeException) { throw new System.InvalidOperationException(); }
            }
        }
    }
    public IEnumerator GetEnumerator() => new LevelRoomEnumerator(tilePositions);

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    void Update()
    {
        if (!revealed & PlayerInRange())
        {
            revealed = true;
            ShowOnMiniMap();
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public int IndexOf(Vec2Int position)
    {
        for (int i = 0; i < tilePositions.Count; i++)
        {
            if (tilePositions[i] == position)
                return i;
        }
        return -1;
    }

    public bool Contains(Vec2Int toCheck)
    {
        if (tilePositions == null)
        {
            tilePositions = new List<Vec2Int>();
            return false;
        }
        return IndexOf(toCheck) >= 0;
    }

    public bool Add(Vec2Int newTilePos)
    {
        if (!Contains(newTilePos))
        {
            for (int i = 0; i <= Size; i++)
            {
                if (i < Size)
                {
                    if (newTilePos.y < tilePositions[i].y || (newTilePos.y == tilePositions[i].y && newTilePos.x < tilePositions[i].x))
                    {
                        tilePositions.Insert(i, newTilePos);
                        break;
                    }
                }
                else
                {
                    tilePositions.Add(newTilePos);
                    break;
                }
            }

            if (newTilePos.x < boundsMin.x)
                boundsMin.x = newTilePos.x;
            else if (newTilePos.x > boundsMin.x)
                boundsMax.x = newTilePos.x;
            if (newTilePos.y < boundsMax.y)
                boundsMin.y = newTilePos.y;
            else if (newTilePos.y > boundsMax.y)
                boundsMax.y = newTilePos.y;

            return true;
        }
        return false;
    }
    public void Add(LevelTile newTile)
    {
        if (Add(newTile.gridPosition))
        {
            tiles.Add(newTile);
            newTile.SetTransformParent(this);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

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

    public int CentreTileIndex()
    {
        if (tilePositions.Count == 0)
            return -1;
        else if (tilePositions.Count <= 2)
            return 0;
        Vector2 centre = Centrepoint().ToVector2();
        float dist, distMin = float.MaxValue;
        int iMin = -1;
        for (int i = 0; i < tilePositions.Count; i++)
        {
            dist = (tilePositions[i].ToVector2() - centre).magnitude;
            if (dist < distMin)
            {
                iMin = i;
                distMin = dist;
            }
        }
        return iMin;
    }

    public Vec2Int CentreTile()
    {
        return tilePositions[CentreTileIndex()];
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private Vector3? PosBetween(int connectInd)
    {
        Vector3 a, b;
        if (internalConns.InBounds(connectInd))
        {
            KeyValuePair<int, int> conn = internalConns[connectInd];
            a = tiles[conn.Key].position;
            b = tiles[conn.Value].position;
            return a.Midpoint(b);
        }
        return null;
    }

    private List<int> ConnectionsTo(int tileInd)
    {
        List<int> connInds = new List<int>();
        if (!tiles.InBounds(tileInd))
        {
            for (int i = 0; i < internalConns.Count; i++)
            {
                if (internalConns[i].Key == tileInd || internalConns[i].Value == tileInd)
                    connInds.Add(i);
            }
        }
        return connInds;
    }

    public int ClosestTile(Vector3 position)
    {
        float closest = float.MaxValue, dist;
        int x = -1;
        for (int i = 0; i < tiles.Count; i++)
        {
            dist = (tiles[i].position - position).magnitude;
            if (dist < closest)
            {
                closest = dist;
                x = i;
            }
        }
        return x;
    }

    public Vector3 RandInternalPosition(int tileInd)
    {
        float aR = Random.Range(0.0f, 360.0f), dR = Random.Range(0.0f, 1.0f);
        Vector3 position, offset;
        position = tiles[tileInd].position;
        offset = generator.tileInnerRadius * dR * new Vector3(Mathf.Sin(aR.ToRad()), 0.0f, Mathf.Cos(aR.ToRad()));
        return position + offset;
    }
    
    public Vector3 RandInternalPosition()
    {
        int iR = Random.Range(0, tiles.Count + internalConns.Count);
        float aR = Random.Range(0.0f, 360.0f), dR = Random.Range(0.0f, 1.0f);
        Vector3 position, offset;
        if (iR < tiles.Count)
        {
            position = tiles[iR].position;
            offset = generator.tileInnerRadius * dR * new Vector3(Mathf.Sin(aR.ToRad()), 0.0f, Mathf.Cos(aR.ToRad()));
        }
        else
        {
            iR -= tiles.Count;
            position = PosBetween(iR).Value;
            offset = generator.tileInnerRadius * 0.5f * dR * new Vector3(Mathf.Sin(aR.ToRad()), 0.0f, Mathf.Cos(aR.ToRad()));
        }
        return position + offset;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public int ConnectionCount(bool debug = false)
    {
        int connections;
        float fRand = Random.Range(0.0f, 1.0f);
        if (Size <= 2)
        {
            connections = 1;
        }
        else if (Size == 3)
        {
            if (fRand <= 0.85)
                connections = 1;
            else
                connections = 2;
        }
        else if (Size == 4)
        {
            if (fRand <= 0.70)
                connections = 1;
            else
                connections = 2;
        }
        else
        {
            if (fRand <= 0.30)
                connections = 3;
            else if (fRand <= 0.85)
                connections = 4;
            else
                connections = 5;
        }
        if (debug)
            Debug.Log("Corridor count for room size of " + Size + " --> " + connections);
        return connections;
    }

    public List<AdjacentRef> PickCorridorStarts(int count, bool connectToExisting, bool debug = false)
    {
        string logStr = "Searching for " + count + " new corridor positions --> ";
        int i, j, nA, nB, nTotal, r;
        List<AdjacentRef> picked = new List<AdjacentRef>();
        List<Vec2Int> pickedTiles = new List<Vec2Int>();
        List<AdjacentRef> localNone, localRoom;
        Vec2Int pos;
        if (connectToExisting)
        {
            List<AdjacentRef> optionsA = new List<AdjacentRef>(), optionsB = new List<AdjacentRef>();
            for (i = 0; i < Size; i++)
            {
                pos = tilePositions[i];
                (localNone, _, _, localRoom) = generator.CategorisedAdjacent(pos);
                for (j = 0; j < localNone.Count; j++)
                {
                    if (!pickedTiles.Contains(pos))
                    {
                        optionsA.Add(new AdjacentRef(localNone[j].direction, pos, tNone));
                        pickedTiles.Add(pos);
                    }
                }
                for (j = 0; j < localRoom.Count; j++)
                {
                    if (!Contains(localRoom[j].position))
                        optionsB.Add(new AdjacentRef(localRoom[j].direction, pos, tRoom));
                }
            }
            for (i = 0, nA = optionsA.Count, nB = optionsB.Count, nTotal = nA + nB; i < count && nTotal > 0; i++, nTotal--)
            {
                r = Random.Range(0, nTotal);
                if (r >= nA)
                {
                    r -= nA;
                    picked.Add(optionsB[r]);
                    optionsB.RemoveAt(r);
                    nB--;
                }
                else
                {
                    picked.Add(optionsA[r]);
                    optionsA.RemoveAt(r);
                    nA--;
                }
            }
        }
        else
        {
            List<AdjacentRef> options = new List<AdjacentRef>();
            for (i = 0; i < Size; i++)
            {
                pos = tilePositions[i];
                localNone = generator.Adjacent(pos, tNone);
                for (j = 0; j < localNone.Count; j++)
                {
                    options.Add(new AdjacentRef(localNone[j].direction, pos, tNone));
                }
            }
            for (i = 0; i < count && options.Count > 0; i++)
            {
                r = Random.Range(0, options.Count);
                picked.Add(options[r]);
                options.RemoveAt(r);
            }
        }
        logStr += "found " + picked.Count;
        if (debug)
            Debug.Log(logStr);
        return picked;
    }

    public List<CorridorSourceRef> GetCorridorStarts(int count, bool connectToExisting, bool debug = false)
    {
        if (count > 0)
        {
            List<AdjacentRef> adjRefs = PickCorridorStarts(count, connectToExisting, debug);
            List<CorridorSourceRef> sourceRefs = new List<CorridorSourceRef>();
            foreach (AdjacentRef adj in adjRefs)
            {
                sourceRefs.Add(new CorridorSourceRef(this, IndexOf(adj.position), adj.direction));
            }
            return sourceRefs;
        }
        return new List<CorridorSourceRef>();
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

    private int EnemySpawnCount()
    {
        if (Size <= 0)
            return 0;
        else
        {
            switch (Size)
            {
                case 1:  return RandomInclusive(1, 2);
                case 2:  return RandomInclusive(2, 3);
                case 3:  return RandomInclusive(3, 4);
                case 4:  return RandomInclusive(4, 5);
                default: return RandomInclusive(5, 7);
            }
        }
    }

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

public struct RoomSourceRef : AreaSourceRef
{
    public LevelArea source { get { return sourceCorridor; } }
    public int indexInSource { get { return sourceCorridorBranch; } set { sourceCorridorBranch = value; } }

    public LevelCorridor sourceCorridor;
    public int sourceCorridorBranch;
    public Vec2Int startTile;

    public bool isNull { get { return source == null && indexInSource < 0; } }

    public RoomSourceRef(Vec2Int sourceTile)
    {
        sourceCorridor = null;
        sourceCorridorBranch = -1;
        startTile = sourceTile;
    }
    public RoomSourceRef(LevelCorridor sourceCorridor, int sourceCorridorBranch)
    {
        this.sourceCorridor = sourceCorridor;
        this.sourceCorridorBranch = sourceCorridorBranch;
        startTile = sourceCorridor[sourceCorridorBranch].endTile.Value;
    }

    public static RoomSourceRef Null { get { return new RoomSourceRef(Vec2Int.zero); } }
}
