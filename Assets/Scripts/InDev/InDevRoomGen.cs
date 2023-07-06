using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;
using Unity.VisualScripting;
using NeoCambion.Random.Unity;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

public class InDevRoomGen : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] GameObject tileTemplate;
    //[SerializeField] Mesh connectorL;
    [SerializeField] GameObject connectorL;
    //[SerializeField] Mesh connectorR;
    [SerializeField] GameObject connectorR;
    //[SerializeField] Mesh corridorConn120;
    [SerializeField] GameObject corridorConn120;
    //[SerializeField] Mesh corridorConn180;
    [SerializeField] GameObject corridorConn180;
    //[SerializeField] Mesh corridorConn240;
    [SerializeField] GameObject corridorConn240;
    //[SerializeField] Mesh corridorConn300;
    [SerializeField] GameObject corridorConn300;
    //[SerializeField] Mesh corridorEnd;
    [SerializeField] GameObject corridorEnd;
    //[SerializeField] Mesh doorway;
    [SerializeField] GameObject doorway;
    //[SerializeField] Mesh wall;
    [SerializeField] GameObject wall;
    //[SerializeField] Mesh wallCorner;
    [SerializeField] GameObject wallCorner;

    private Grid2D<LevelTile> tiles = new Grid2D<LevelTile>();

    #endregion

    #region [ PROPERTIES ]

    public static float zOffset { get { return Mathf.Sin(60.0f.ToRad()); } }
    public float roomRadius = 6.0f;
    [SerializeField] float meshRotation = 180.0f;
    [SerializeField] Vector2Int size = Vector2Int.one;
    [SerializeField] Vector2Int centre = Vector2Int.zero;
    [Range(0.0f, 100.0f)]
    [SerializeField] float connectToExisting = 40.0f;
    [Range(0.0f, 100.0f)]
    [SerializeField] float mergeRooms = 25.0f;

    private Vector2Int boundsMin;
    private Vector2Int boundsMax;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {

    }

    void Start()
    {
        GenerateV3(size, centre);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public Vector2Int[] AdjacentCoords(int tileX, int tileZ)
    {
        // Adjacent coordinates listed starting vertically upwards
        // and moving with a clockwise rotation
        Vector2Int[] coords = new Vector2Int[6];
        if (tileX % 2 == 0)
        {
            coords[0] = new Vector2Int(0, 1);
            coords[1] = new Vector2Int(1, 0);
            coords[2] = new Vector2Int(1, -1);
            coords[3] = new Vector2Int(0, -1);
            coords[4] = new Vector2Int(-1, -1);
            coords[5] = new Vector2Int(-1, 0);
        }
        else
        {
            coords[0] = new Vector2Int(0, 1);
            coords[1] = new Vector2Int(1, 1);
            coords[2] = new Vector2Int(1, 0);
            coords[3] = new Vector2Int(0, -1);
            coords[4] = new Vector2Int(-1, 0);
            coords[5] = new Vector2Int(-1, 1);
        }

        for (int i = 0; i < 6; i++)
        {
            coords[i] = new Vector2Int(coords[i].x + tileX, coords[i].y + tileZ);
        }

        return coords;
    }

    public Vector2Int[] AdjacentCoords(Vector2Int tileCoords)
    {
        return AdjacentCoords(tileCoords.x, tileCoords.y);
    }

    public Vector2Int[] FreeAdjacent(int tileX, int tileZ)
    {
        Vector2Int[] adj = AdjacentCoords(tileX, tileZ);
        List<Vector2Int> free = new List<Vector2Int>();
        for (int i = 0; i < adj.Length; i++)
        {
            if (tiles[adj[i].x, adj[i].y] == null || tiles[adj[i].x, adj[i].y].type == LevelTile.TileType.None)
                free.Add(adj[i]);
        }
        return free.ToArray();
    }
    
    public Vector2Int[] FreeAdjacent(Vector2Int tileCoords)
    {
        return FreeAdjacent(tileCoords.x, tileCoords.y);
    }

    public Vector3 TilePosition(int tileX, int tileZ)
    {
        Vector3 vect = Vector3.zero;
        vect.x = (float)tileX * 1.5f * roomRadius;
        vect.z = ((float)tileZ * 2.0f + (tileX % 2 == 0 ? 0.0f : 1.0f)) * zOffset * roomRadius;
        return vect;
    }

    public Vector3 TilePosition(Vector2Int tileCoords)
    {
        return TilePosition(tileCoords.x, tileCoords.y);
    }

    /*private Mesh TileMesh(Vector2Int coords, float rotateByRadians = 0.0f)
    {
        LevelTile.ConnectionState[] connections = tiles[coords.x, coords.y].connections;
        List<CombineInstance> components = new List<CombineInstance>();
        CombineInstance toAdd;
        if (tiles[coords.x, coords.y].type == LevelTile.TileType.Room)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                int iMinus = i > 0 ? i - 1 : connections.Length - 1;
                int iPlus = i < connections.Length - 1 ? i + 1 : 0;
                Vector3 rot = new Vector3(0.0f, (float)i * 60.0f.ToRad() + rotateByRadians, 0.0f);
                if (connections[i] != LevelTile.ConnectionState.Merge)
                {
                    if (connections[i] == LevelTile.ConnectionState.None || connections[i] == LevelTile.ConnectionState.Block)
                    {
                        toAdd = new CombineInstance();
                        toAdd.mesh = wall.Rotate(rot, Vector3.zero);
                        components.Add(toAdd);
                    }
                    else if (connections[i] == LevelTile.ConnectionState.Connect)
                    {
                        toAdd = new CombineInstance();
                        toAdd.mesh = doorway.Rotate(rot, Vector3.zero);
                        components.Add(toAdd);
                    }
                    if (connections[iPlus] != LevelTile.ConnectionState.Merge)
                    {
                        toAdd = new CombineInstance();
                        toAdd.mesh = wallCorner.Rotate(rot, Vector3.zero);
                        components.Add(toAdd);
                    }
                }
                else if (connections[i] == LevelTile.ConnectionState.Merge)
                {
                    if (connections[iMinus] == LevelTile.ConnectionState.Merge)
                    { }
                    else
                    {
                        toAdd = new CombineInstance();
                        toAdd.mesh = connectorL.Rotate(rot, Vector3.zero);
                        components.Add(toAdd);
                    }

                    if (connections[iPlus] == LevelTile.ConnectionState.Merge)
                    { }
                    else
                    {
                        toAdd = new CombineInstance();
                        toAdd.mesh = connectorR.Rotate(rot, Vector3.zero);
                        components.Add(toAdd);
                    }
                }
            }
        }
        else if (tiles[coords.x, coords.y].type == LevelTile.TileType.Corridor)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                //int iMinus = i > 0 ? i - 1 : connections.Length - 1;
                //int iPlus = i < connections.Length ? i + 1 : 0;
                Vector3 rot = new Vector3(0.0f, (float)i * 60.0f.ToRad() + rotateByRadians, 0.0f);
                if (connections[i] == LevelTile.ConnectionState.Connect || connections[i] == LevelTile.ConnectionState.Merge)
                {
                    toAdd = new CombineInstance();
                    toAdd.mesh = corridorEnd.Rotate(rot, Vector3.zero);
                    components.Add(toAdd);
                }
            }
        }
        Mesh meshOut = null;
        if (components.Count > 1)
        {
            meshOut = new Mesh();
            meshOut.CombineMeshes(components.ToArray());
            Debug.Log("Generated mesh for tile at " + coords.x + ", " + coords.y);
        }
        else if (components.Count == 1)
        {
            meshOut = components[0].mesh;
            Debug.Log("Applied mesh to tile at " + coords.x + ", " + coords.y);
        }
        else
        {
            Debug.Log("Failed to generate mesh for tile at " + coords.x + ", " + coords.y);
        }
        return meshOut;
    }*/
    
    private Mesh TileMesh(Vector2Int coords, float rotateByDegrees = 0.0f)
    {
        LevelTile.ConnectionState[] connections = tiles[coords.x, coords.y].connections;
        List<GameObject> components = new List<GameObject>();
        if (tiles[coords.x, coords.y].type == LevelTile.TileType.Room)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                int iMinus = i > 0 ? i - 1 : connections.Length - 1;
                int iPlus = i < connections.Length - 1 ? i + 1 : 0;
                Vector3 rot = new Vector3(0.0f, (float)i * 60.0f + rotateByDegrees, 0.0f);
                if (connections[i] != LevelTile.ConnectionState.Merge)
                {
                    if (connections[i] == LevelTile.ConnectionState.None || connections[i] == LevelTile.ConnectionState.Block)
                    {
                        GameObject component = Instantiate(wall, null);
                        component.transform.eulerAngles = rot;
                        components.Add(component);
                    }
                    else if (connections[i] == LevelTile.ConnectionState.Connect)
                    {
                        GameObject component = Instantiate(doorway, null);
                        component.transform.eulerAngles = rot;
                        components.Add(component);
                    }
                    if (connections[iPlus] != LevelTile.ConnectionState.Merge)
                    {
                        GameObject component = Instantiate(wallCorner, null);
                        component.transform.eulerAngles = rot;
                        components.Add(component);
                    }
                }
                else if (connections[i] == LevelTile.ConnectionState.Merge)
                {
                    if (connections[iMinus] == LevelTile.ConnectionState.Merge)
                    { }
                    else
                    {
                        GameObject component = Instantiate(connectorL, null);
                        component.transform.eulerAngles = rot;
                        components.Add(component);
                    }

                    if (connections[iPlus] == LevelTile.ConnectionState.Merge)
                    { }
                    else
                    {
                        GameObject component = Instantiate(connectorR, null);
                        component.transform.eulerAngles = rot;
                        components.Add(component);
                    }
                }
            }
        }
        else if (tiles[coords.x, coords.y].type == LevelTile.TileType.Corridor)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                //int iMinus = i > 0 ? i - 1 : connections.Length - 1;
                //int iPlus = i < connections.Length ? i + 1 : 0;
                Vector3 rot = new Vector3(0.0f, (float)i * 60.0f + rotateByDegrees, 0.0f);
                if (connections[i] == LevelTile.ConnectionState.Connect || connections[i] == LevelTile.ConnectionState.Merge)
                {
                    GameObject component = Instantiate(corridorEnd, null);
                    component.transform.eulerAngles = rot;
                    components.Add(component);
                }
            }
        }
        Mesh meshOut = new Mesh();
        meshOut.MergeFrom(components, true);
        return meshOut;
    }

    private void GenerateAllMeshes()
    {
        for (int x = boundsMin.x; x <= boundsMax.x; x++)
        {
            for (int y = boundsMin.y; y <= boundsMax.y; y++)
            {
                if (tiles[x, y] != null)
                {
                    if (!tiles[x, y].emptySpace)
                    {
                        //tiles[x, y].gameObject.GetComponent<MeshFilter>().sharedMesh = TileMesh(new Vector2Int(x, y), meshRotation);
                        //tiles[x, y].gameObject.GetComponent<MeshRenderer>().enabled = true;
                        tiles[x, y].Mesh = TileMesh(new Vector2Int(x, y), meshRotation);
                    }
                    else
                    {
                        tiles[x, y].gameObject.GetComponent<MeshRenderer>().enabled = false;
                    }
                }
            }
        }
    }

    public void GenerateV1(Vector2Int size)
    {
        GenerateV1(size, new Vector2Int(-1, -1));
    }

    public void GenerateV1(Vector2Int size, Vector2Int centre)
    {
        bool destroyAtEnd = tileTemplate == null;
        GameObject baseObj = tileTemplate == null ? new GameObject() : tileTemplate;
        baseObj.GetOrAddComponent<LevelTile>();
        baseObj.GetOrAddComponent<MeshRenderer>();
        baseObj.GetOrAddComponent<MeshFilter>();

        if (size.x == 0)
            size.x = 1;
        else if (size.x < 0)
            size.x *= -1;
        if (size.y == 0)
            size.y = 1;
        else if (size.y < 0)
            size.y *= -1;

        Vector2Int offset = -centre;
        if (centre.x >= size.x)
            offset.x += 1;
        else if (centre.x < 0)
            offset.x = 0;
        if (centre.y >= size.y)
            offset.y += 1;
        else if (centre.y < 0)
            offset.y = 0;

        boundsMin = -centre;
        boundsMax = boundsMin + size;

        tiles.Clear();

        int x, y;
        for (x = boundsMin.x; x <= boundsMax.x; x++)
        {
            for (y = boundsMin.y; y <= boundsMax.y; y++)
            {
                GameObject tileObj = Instantiate(baseObj, transform);
                tileObj.transform.position = TilePosition(x, y);
                LevelTile tile = tileObj.GetComponent<LevelTile>();
                tile.type = (LevelTile.TileType)Random.Range(1, 4);
                tiles[x, y] = tile;
                tileObj.name = "Tile [" + x + ", " + y + "] - " + tile.type;
            }
        }

        for (x = boundsMin.x; x <= boundsMax.x; x++)
        {
            for (y = boundsMin.y; y <= boundsMax.y; y++)
            {
                if (tiles[x, y] != null)
                {
                    if (!tiles[x, y].emptySpace)
                    {
                        Vector2Int[] adjacent = AdjacentCoords(new Vector2Int(x, y));
                        int i2, x2, y2;
                        bool corridor;
                        for (int i = 0; i < 6; i++)
                        {
                            i2 = i < 3 ? i + 3 : i - 3;
                            x2 = adjacent[i].x;
                            y2 = adjacent[i].y;
                            if (tiles[x2, y2] == null || tiles[x2, y2].emptySpace)
                            {
                                tiles[x, y].connections[i] = LevelTile.ConnectionState.Block;
                            }
                            else
                            {
                                corridor = tiles[x, y].type == LevelTile.TileType.Corridor || tiles[x2, y2].type == LevelTile.TileType.Corridor;
                                if (tiles[x2, y2].connections[i2] == LevelTile.ConnectionState.None)
                                {
                                    tiles[x, y].connections[i] = (LevelTile.ConnectionState)Random.Range(1, corridor ? 3 : 4);
                                }
                                else
                                {
                                    tiles[x, y].connections[i] = tiles[x2, y2].connections[i2];
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            tiles[x, y].connections[i] = LevelTile.ConnectionState.None;
                        }
                    }
                }
            }
        }

        GenerateAllMeshes();

        if (destroyAtEnd)
            Destroy(baseObj);
    }

    public void GenerateV2(Vector2Int size, Vector2Int centre)
    {
        bool destroyAtEnd = tileTemplate == null;
        GameObject baseObj = tileTemplate == null ? new GameObject() : tileTemplate;
        baseObj.GetOrAddComponent<LevelTile>();
        baseObj.GetOrAddComponent<MeshRenderer>();
        baseObj.GetOrAddComponent<MeshFilter>();

        Vector2Int offset = -centre;
        if (centre.x >= size.x)
            offset.x += 1;
        else if (centre.x < 0)
            offset.x = 0;
        if (centre.y >= size.y)
            offset.y += 1;
        else if (centre.y < 0)
            offset.y = 0;

        boundsMin = -centre;
        boundsMax = boundsMin + size - Vector2Int.one;
        //Debug.Log(boundsMin + " | " + boundsMax);

        tiles.Clear();

        int x, y;
        for (x = boundsMin.x; x <= boundsMax.x; x++)
        {
            for (y = boundsMin.y; y <= boundsMax.y; y++)
            {
                GameObject tileObj = Instantiate(baseObj, transform);
                tileObj.transform.position = TilePosition(x, y);
                tileObj.name = "Tile [" + x + ", " + y + "]";
                LevelTile tile = tileObj.GetComponent<LevelTile>();
                tiles[x, y] = tile;
            }
        }

        tiles[0, 0].type = LevelTile.TileType.Room;
        List<Vector2Int> generateFrom = new List<Vector2Int>() { new Vector2Int(0, 0) }, toAdd = new List<Vector2Int>(), newGen = new List<Vector2Int>();
        Vector2Int[] adjacent;
        LevelTile.TileType typeAt;
        int i, i2, x2, y2;
        while (generateFrom.Count > 0)
        {
            newGen.Clear();
            foreach (Vector2Int coords in generateFrom)
            {
                toAdd.Clear();
                x = coords.x;
                y = coords.y;
                adjacent = AdjacentCoords(coords);
                typeAt = tiles[x, y].type;
                tiles[x, y].gameObject.name += " - " + typeAt;

                bool allowEmpty;
                float emptyChance = tiles[x, y].chanceForEmpty, newEmptyChance = emptyChance + ((1.0f - emptyChance) / 5.0f);
                for (i = 0; i < 6; i++)
                {
                    allowEmpty = Random.Range(0.0f, 1.0f) < emptyChance;
                    i2 = i < 3 ? i + 3 : i - 3;
                    x2 = adjacent[i].x;
                    y2 = adjacent[i].y;
                    if (tiles[x2, y2] == null)
                    {
                        tiles[x, y].connections[i] = LevelTile.ConnectionState.Block;
                    }
                    else
                    {
                        if (tiles[x2, y2].type == LevelTile.TileType.None)
                        {
                            LevelTile.TileType newType = (LevelTile.TileType)Random.Range((allowEmpty ? 1 : 2), 4);
                            tiles[x2, y2].type = newType;
                            tiles[x2, y2].chanceForEmpty = newEmptyChance;
                            if (newType != LevelTile.TileType.Empty)
                            {
                                toAdd.Add(new Vector2Int(x2, y2));
                                if (newType == LevelTile.TileType.Room && typeAt == LevelTile.TileType.Room && Random.Range(0.0f, 100.0f) > mergeRooms)
                                {
                                    tiles[x, y].connections[i] = LevelTile.ConnectionState.Merge;
                                    tiles[x2, y2].connections[i2] = LevelTile.ConnectionState.Merge;
                                }
                                else if (newType == LevelTile.TileType.Corridor)
                                {
                                    tiles[x, y].connections[i] = LevelTile.ConnectionState.Connect;
                                    tiles[x2, y2].connections[i2] = LevelTile.ConnectionState.Connect;
                                }
                            }
                            else
                            {
                                emptyChance = emptyChance <= 0.9f ? emptyChance += 0.1f : 1.0f;
                            }
                        }
                        else
                        {
                            if (!tiles[x, y].ConnectedAt(i) && Random.Range(0.0f, 100.0f) > connectToExisting)
                            {
                                if (tiles[x2, y2].type == LevelTile.TileType.Room && Random.Range(0.0f, 100.0f) > mergeRooms)
                                {
                                    tiles[x, y].connections[i] = LevelTile.ConnectionState.Merge;
                                    tiles[x2, y2].connections[i2] = LevelTile.ConnectionState.Merge;
                                }
                                else
                                {
                                    tiles[x, y].connections[i] = LevelTile.ConnectionState.Connect;
                                    tiles[x2, y2].connections[i2] = LevelTile.ConnectionState.Connect;
                                }
                            }
                            else
                            {
                                tiles[x, y].connections[i] = LevelTile.ConnectionState.Block;
                                tiles[x2, y2].connections[i2] = LevelTile.ConnectionState.Block;
                            }
                        }
                    }
                }
                newGen.AddRange(toAdd);
            }
            generateFrom.Clear();
            generateFrom.CopyFrom(newGen);
        }

        /*for (x = boundsMin.x; x <= boundsMax.x; x++)
        {
            for (y = boundsMin.y; y <= boundsMax.y; y++)
            {
                if (tiles[x, y] != null && tiles[x, y].emptySpace)
                {
                    Destroy(tiles[x, y].gameObject);
                }
            }
        }*/

        GenerateAllMeshes();

        if (destroyAtEnd)
            Destroy(baseObj);
    }

    private class InDevRoom
    {
        public List<Vector2Int> tiles = new List<Vector2Int>();

        public bool Contains(Vector2Int toCheck)
        {
            foreach (Vector2Int tile in tiles)
            {
                if (toCheck == tile)
                    return true;
            }
            return false;
        }
    }

    private void MergeRoomTiles(InDevRoom room)
    {
        Vector2Int[] adjacent;
        int i2;
        foreach (Vector2Int tilePos in room.tiles)
        {
            adjacent = AdjacentCoords(tilePos);
            for (int i = 0; i < adjacent.Length; i++)
            {
                i2 = i < 3 ? i + 3 : i - 3;
                if (room.Contains(adjacent[i]))
                {
                    tiles[tilePos.x, tilePos.y].connections[i] = LevelTile.ConnectionState.Merge;
                    tiles[adjacent[i].x, adjacent[i].y].connections[i2] = LevelTile.ConnectionState.Merge;
                }
                else
                {
                    tiles[tilePos.x, tilePos.y].connections[i] = LevelTile.ConnectionState.Block;
                }
            }
        }
    }
    
    private class InDevCorridor
    {
        public List<Vector2Int[]> tiles = new List<Vector2Int[]>();
        public List<int[]> endpoints = new List<int[]>();

        public bool Contains(Vector2Int toCheck)
        {
            foreach (Vector2Int[] segment in tiles)
            {
                foreach (Vector2Int tile in segment)
                {
                    if (tile == toCheck)
                        return true;
                }
            }
            return false;
        }
    }

    private InDevRoom NewRoom(Vector2Int startTile)
    {
        if (tiles[startTile.x, startTile.y] == null || tiles[startTile.x, startTile.y].type == LevelTile.TileType.None)
        {
            InDevRoom room = new InDevRoom();
            int tileCount = Ext_Random.RangeWeighted(1, 6, Ext_Random.WeightingCurve.Power, false, 2.0f);
            //Debug.Log("Room size at (" + startTile.x + ", " + startTile.y + "): " + tileCount);

            if (tiles[startTile.x, startTile.y] == null)
            {
                GameObject tileObj = Instantiate(tileTemplate, transform);
                tileObj.transform.position = TilePosition(startTile.x, startTile.y);
                tileObj.name = "Tile [" + startTile.x + ", " + startTile.y + "]";
                tiles[startTile.x, startTile.y] = tileObj.GetComponent<LevelTile>();
            }
            tiles[startTile.x, startTile.y].type = LevelTile.TileType.Room;
            //tiles[startTile.x, startTile.y].gameObject.name += " (Room Start)";
            room.tiles.Add(startTile);

            Vector2Int[] freeAdjacent = new Vector2Int[0];
            int sourceOfNext = -1;
            Vector2Int targ;
            for (int i = 1; i < tileCount; i++)
            {
                for (int j = 0; j < room.tiles.Count; j++)
                {
                    freeAdjacent = FreeAdjacent(room.tiles[j]);
                    if (freeAdjacent.Length > 0)
                    {
                        sourceOfNext = j;
                        break;
                    }
                }

                if (sourceOfNext >= 0)
                {
                    targ = freeAdjacent[Random.Range(0, freeAdjacent.Length)];
                    if (tiles[targ.x, targ.y] == null)
                    {
                        GameObject tileObj = Instantiate(tileTemplate, transform);
                        tileObj.transform.position = TilePosition(targ.x, targ.y);
                        tileObj.name = "Tile [" + targ.x + ", " + targ.y + "]"/* + " (Room)"*/;
                        tiles[targ.x, targ.y] = tileObj.GetComponent<LevelTile>();
                    }
                    tiles[targ.x, targ.y].type = LevelTile.TileType.Room;
                    room.tiles.Add(targ);

                }
                else
                {
                    break;
                }

                sourceOfNext = -1;
            }

            MergeRoomTiles(room);

            return room;
        }
        else
        {
            Debug.Log("Existing tile of type " + tiles[startTile.x, startTile.y].type + " at " + startTile);
            return null;
        }
    }

    private InDevCorridor NewCorridor(Vector2Int sourceTile, InDevRoom sourceRoom = null)
    {
        if (tiles[sourceTile.x, sourceTile.y] != null)
        {
            InDevCorridor corridor = new InDevCorridor();

            int lMax = Ext_Random.RangeWeighted(0, 6, Ext_Random.WeightingCurve.Power, true, 2.0f);
            bool tryBranch = lMax > 2 ? (Random.Range(0.0f, 1.0f) <= 0.4f) : false;
            int branchInd = tryBranch ? Random.Range(0, lMax - 1) : -1;

            //Vector2Int halted = new Vector2Int(int.MinValue, int.MinValue);
            Vector2Int[] nextTiles = new Vector2Int[] { sourceTile };
            corridor.tiles.Add(nextTiles);
            //Vector2Int tileTarg;
            bool branched = false;
            int x, y, x2, y2;
            for (int i = -1; i < lMax && nextTiles.Length > 0; i++)
            {
                List<Vector2Int> next = new List<Vector2Int>();
                List<bool> endpoint = new List<bool>();
                for (int branch = 0; branch < nextTiles.Length; branch++)
                {
                    x = nextTiles[branch].x;
                    y = nextTiles[branch].y;
                    if (tiles[x, y] == null || tiles[x, y].type == LevelTile.TileType.None || nextTiles[branch] == sourceTile)
                    {
                        if (tiles[x, y].type == LevelTile.TileType.None)
                        {
                            tiles[x, y].type = LevelTile.TileType.Corridor;
                            //tiles[x, y].gameObject.name += " (Corridor)";
                            tiles[x, y].ReplaceConnections(LevelTile.ConnectionState.None, LevelTile.ConnectionState.Block);
                        }

                        Vector2Int[] adjacent = AdjacentCoords(nextTiles[branch]);
                        LevelTile.ConnectionState[] connections = tiles[x, y].connections;
                        LevelTile.TileType[] types = new LevelTile.TileType[adjacent.Length];

                        int[] chances = new int[6] { 0, 0, 0, 0, 0, 0 };
                        int mod, totalChance = 0;
                        for (int j = 0; j < 6; j++)
                        {
                            types[j] = tiles[adjacent[j].x, adjacent[j].y] == null ? LevelTile.TileType.None : tiles[adjacent[j].x, adjacent[j].y].type;
                            if (connections[j] == LevelTile.ConnectionState.Block)
                            {
                                switch (types[j])
                                {
                                    case LevelTile.TileType.None:
                                        mod = 8;
                                        break;

                                    case LevelTile.TileType.Room:
                                        mod = sourceRoom != null ? (sourceRoom.Contains(adjacent[j]) ? 0 : 3) : 3;
                                        //mod = 3;
                                        break;

                                    case LevelTile.TileType.Corridor:
                                        mod = corridor.Contains(adjacent[j]) ? 0 : 1;
                                        break;

                                    default:
                                        mod = 0;
                                        break;
                                }
                                chances[j] += mod + (j > 0 ? chances[j - 1] : 0);
                                totalChance += mod;
                            }
                            else
                            {
                                chances[j] += j > 0 ? chances[j - 1] : 0;
                            }
                        }

                        int r = Random.Range(0, totalChance) + 1;
                        int ind = chances.FirstGreaterThan(r, true);
                        int ind2 = ind < 3 ? ind + 3 : ind - 3;

                        //Debug.Log(adjacent.Length + " | " + r + " | " + ind);
                        next.Add(adjacent[ind]);
                        //bool isEndpoint = !(i < lMax && types[ind] == LevelTile.TileType.None);
                        endpoint.Add(!(i < lMax - 1 && types[ind] == LevelTile.TileType.None));

                        x2 = adjacent[ind].x;
                        y2 = adjacent[ind].y;
                        if (tiles[x2, y2] == null)
                        {
                            GameObject tileObj = Instantiate(tileTemplate, transform);
                            tiles[x2, y2] = tileObj.GetComponent<LevelTile>();
                            tileObj.name = "Tile [" + x2 + ", " + y2 + "]";
                            tileObj.transform.position = TilePosition(x2, y2);
                        }

                        tiles[x, y].connections[ind] = LevelTile.ConnectionState.Connect;
                        tiles[x2, y2].connections[ind2] = LevelTile.ConnectionState.Connect;

                        if (tryBranch && !branched && i == branchInd)
                        {
                            chances[ind] = 0;
                            r = Random.Range(0, totalChance) + 1;
                            ind = chances.FirstGreaterThan(r, true);
                            ind2 = ind < 3 ? ind + 3 : ind - 3;

                            next.Add(adjacent[ind]);
                            endpoint.Add(!(i < lMax && types[ind] == LevelTile.TileType.None));

                            x2 = adjacent[ind].x;
                            y2 = adjacent[ind].y;
                            if (tiles[x2, y2] == null)
                            {
                                GameObject tileObj = Instantiate(tileTemplate, transform);
                                tiles[x2, y2] = tileObj.GetComponent<LevelTile>();
                                tileObj.name = "Tile [" + x2 + ", " + y2 + "]";
                                tileObj.transform.position = TilePosition(x2, y2);
                            }

                            tiles[x, y].connections[ind] = LevelTile.ConnectionState.Connect;
                            tiles[x2, y2].connections[ind2] = LevelTile.ConnectionState.Connect;

                            branched = true;
                        }
                    }
                }

                if (next.Count > 0)
                {
                    nextTiles = next.ToArray();
                    corridor.tiles.Add(nextTiles);
                    for (int j = 0; j < endpoint.Count; j++)
                    {
                        if (endpoint[j])
                            corridor.endpoints.Add(new int[] { corridor.tiles.Count - 1, j });
                    }
                }

                /*branched = tryBranch && i > branchInd;
                int br = branched ? 2 : 1;
                for (int branch = 0; branch < br; branch++)
                {
                    tileTarg = nextTiles[branch];
                    if (tileTarg != halted)
                    {
                        x = tileTarg.x;
                        y = tileTarg.y;

                        Vector2Int[] adjacent = AdjacentCoords(tileTarg);
                        LevelTile.ConnectionState[] connections = tiles[x, y].connections;
                        LevelTile.TileType[] types = new LevelTile.TileType[adjacent.Length];

                        int[] chances = new int[6] { 0, 0, 0, 0, 0, 0 };
                        int mod, totalChance = 0;
                        for (int j = 0; j < 6; j++)
                        {
                            types[j] = tiles[adjacent[j].x, adjacent[j].y] == null ? LevelTile.TileType.None : tiles[adjacent[j].x, adjacent[j].y].type;
                            if (connections[j] == LevelTile.ConnectionState.Block)
                            {
                                switch (types[j])
                                {
                                    case LevelTile.TileType.None:
                                        mod = 8;
                                        break;

                                    case LevelTile.TileType.Room:
                                        mod = 3;
                                        break;

                                    case LevelTile.TileType.Corridor:
                                        mod = 1;
                                        break;

                                    default:
                                        mod = 0;
                                        break;
                                }
                                chances[j] += mod + (j > 0 ? chances[j - 1] : 0);
                                totalChance += mod;
                            }
                            else
                            {
                                chances[j] += j > 0 ? chances[j - 1] : 0;
                            }
                        }

                        int r = Random.Range(0, totalChance) + 1;
                        //int ind = r <= chances[0] ? 0 : (r <= chances[1] ? 1 : (r <= chances[2] ? 2 : (r <= chances[3] ? 3 : (r <= chances[4] ? 4 : (r <= chances[5] ? 5 : -1)))));
                        int ind = chances.FirstLessThan(r), ind2 = ind < 3 ? ind + 3 : ind - 3;
                        nextTiles[branch] = adjacent[ind];

                        x2 = nextTiles[branch].x;
                        y2 = nextTiles[branch].y;
                        if (types[ind] != LevelTile.TileType.Empty)
                        {
                            corridor.tiles.Add(nextTiles[branch]);
                            if (types[ind] == LevelTile.TileType.None)
                            {
                                if (tiles[x2, y2] == null)
                                {
                                    GameObject tileObj = Instantiate(tileTemplate);
                                    tiles[x2, y2] = tileObj.GetComponent<LevelTile>();
                                    tileObj.name = "Tile [" + x2 + ", " + y2 + "]";
                                    tileObj.transform.position = TilePosition(x2, y2);
                                }

                                if (i < lMax)
                                {
                                    tiles[x2, y2].type = LevelTile.TileType.Corridor;
                                }
                                else
                                {
                                    corridor.endpoints[branch] = corridor.tiles.Count - 1;
                                    nextTiles[branch] = halted;
                                }
                            }
                            else
                            {
                                corridor.endpoints[branch] = corridor.tiles.Count - 1;
                                nextTiles[branch] = halted;
                            }

                            tiles[x, y].connections[ind] = LevelTile.ConnectionState.Connect;
                            if (i < lMax)
                            {
                                tiles[x2, y2].connections[ind2] = LevelTile.ConnectionState.Connect;
                                tiles[x2, y2].ReplaceConnections(LevelTile.ConnectionState.None, LevelTile.ConnectionState.Block);
                            }
                        }

                        if (!branched && i == branchInd && branch == 0)
                        {
                            chances[ind] = 0;
                            r = Random.Range(0, totalChance) + 1;
                            //ind = r <= chances[0] ? 0 : (r <= chances[1] ? 1 : (r <= chances[2] ? 2 : (r <= chances[3] ? 3 : (r <= chances[4] ? 4 : (r <= chances[5] ? 5 : -1)))));
                            ind = chances.FirstLessThan(r);
                            nextTiles[1] = adjacent[ind];
                        }
                    }
                }*/
            }

            return corridor;
        }
        else
        {
            return null;
        }
    }

    public void GenerateV3(Vector2Int size, Vector2Int centre)
    {
        bool destroyAtEnd = tileTemplate == null;
        GameObject baseObj = tileTemplate == null ? new GameObject() : tileTemplate;
        baseObj.GetOrAddComponent<LevelTile>();
        baseObj.GetOrAddComponent<MeshRenderer>();
        baseObj.GetOrAddComponent<MeshFilter>();

        Vector2Int offset = -centre;
        if (centre.x >= size.x)
            offset.x += 1;
        else if (centre.x < 0)
            offset.x = 0;
        if (centre.y >= size.y)
            offset.y += 1;
        else if (centre.y < 0)
            offset.y = 0;

        boundsMin = -centre;
        boundsMax = boundsMin + size - Vector2Int.one;
        //Debug.Log(boundsMin + " | " + boundsMax);

        tiles.Clear();

        int i, j, nConnections = 0, iR;
        float fR;
        List<Vector2Int> roomStarts = new List<Vector2Int>() { Vector2Int.zero },
            validForConnecting = new List<Vector2Int>(),
            corridorStarts = new List<Vector2Int>();
        List<int> corridorOriginRooms = new List<int>();
        List<InDevRoom> rooms = new List<InDevRoom>();
        List<InDevCorridor> corridors = new List<InDevCorridor>();
        float terminateChance = 0.0f, terminateIncrement = 0.1f;
        bool terminate;
        List<int[]> endpoints;
        while (roomStarts.Count > 0)
        {
            corridorStarts.Clear();
            for (i = 0; i < roomStarts.Count; i++)
            {
                //if (tiles[roomStarts[i].x, roomStarts[i].y] != null)
                    //Debug.Log(tiles[roomStarts[i].x, roomStarts[i].y].type);
                InDevRoom room = NewRoom(roomStarts[i]);
                if (room == null)
                    Debug.Log("Room generation failed at " + roomStarts[i]);
                rooms.Add(room);
                validForConnecting.Clear();
                terminate = Random.Range(0.0f, 1.0f) < terminateChance;
                //Debug.Log("Terminate: " + terminate);
                if (!terminate)
                {
                    //if (room == null)
                        //Debug.Log("Room does not exist!");
                    //else if (room.tiles == null || room.tiles.Count == 0)
                        //Debug.Log("Room does not contain any tiles!");
                    foreach (Vector2Int tilePos in room.tiles)
                    {
                        if (!tiles[tilePos.x, tilePos.y].fullMerged)
                        {
                            validForConnecting.Add(tilePos);
                        }
                    }

                    fR = Random.Range(0.0f, 1.0f);
                    // 1-2 tiles - max 2 corridors
                        // 1 - 75.00% (12/16)
                        // 2 - 25.00% (4/16)
                    if (room.tiles.Count <= 2)
                    {
                        if (fR <= 0.75)
                            nConnections = 1;
                        else
                            nConnections = 2;
                    }
                    // 3-4 tiles - max 3 corridors
                        // 1 - 50.00% (8/16)
                        // 2 - 37.50% (6/16)
                        // 3 - 12.50% (2/16)
                    else if (room.tiles.Count <= 4)
                    {
                        if (fR <= 0.50)
                            nConnections = 1;
                        else if (fR <= 0.875)
                            nConnections = 1;
                        else
                            nConnections = 2;
                    }
                    // 5 tiles - max 4 corridors
                        // 1 - 31.25% (5/16)
                        // 2 - 43.75% (7/16)
                        // 3 - 18.75% (3/16)
                        // 4 - 06.25% (1/16)
                    else if (room.tiles.Count <= 5)
                    {
                        if (fR <= 0.3125)
                            nConnections = 1;
                        else if (fR <= 0.75)
                            nConnections = 1;
                        else if (fR <= 0.9375)
                            nConnections = 1;
                        else
                            nConnections = 2;
                    }
                    // +1 if starting tile
                    if (rooms.Count == 1)
                    {
                        nConnections += 1;
                    }
                    //Debug.Log("Connections from (" + room.tiles.First().x + ", " + room.tiles.First().x + "): " + nConnections);
                    for (j = 0; j < nConnections && validForConnecting.Count > 0; j++)
                    {
                        iR = Random.Range(0, validForConnecting.Count);
                        corridorStarts.Add(validForConnecting[iR]);
                        corridorOriginRooms.Add(rooms.Count - 1);
                        bool allowRemove = validForConnecting.Count >= nConnections - j;
                        if (allowRemove)
                            validForConnecting.RemoveAt(iR);
                    }
                }
            }

            roomStarts.Clear();
            if (corridorStarts.Count > 0)
            {
                for (i = 0; i < corridorStarts.Count; i++)
                {
                    corridors.Add(NewCorridor(corridorStarts[i]/*, rooms[corridorOriginRooms[i]]*/));
                    endpoints = corridors.Last().endpoints;
                    for (j = 0; j < endpoints.Count; j++)
                    {
                        Vector2Int endpoint = corridors.Last().tiles[endpoints[j][0]][endpoints[j][1]];
                        if (!roomStarts.Contains(endpoint))
                        {
                            if (tiles[endpoint.x, endpoint.y] == null || tiles[endpoint.x, endpoint.y].type == LevelTile.TileType.None)
                            {
                                roomStarts.Add(endpoint);
                                Debug.Log("Adding new room starting point at " + endpoint + " - type of " + (tiles[endpoint.x, endpoint.y] == null ? "NULL" : tiles[endpoint.x, endpoint.y].type));
                            }
                        }
                        else
                        {
                            Debug.Log("Room starting point already exists at " + endpoint);
                        }
                    }
                }
            }

            terminateChance += terminateIncrement;
        }

        //GenerateAllMeshes();
        int x, y;
        foreach (InDevRoom room in rooms)
        {
            foreach (Vector2Int tilePos in room.tiles)
            {
                Debug.Log(tilePos);
                x = tilePos.x;
                y = tilePos.y;
                if (tiles[x, y] != null && tiles[x, y].Mesh == null)
                {
                    tiles[x, y].Mesh = TileMesh(tilePos, meshRotation);
                    tiles[x, y].gameObject.name += " (Room)";
                }
            }
        }

        foreach (InDevCorridor corridor in corridors)
        {
            foreach (Vector2Int[] set in corridor.tiles)
            {
                foreach (Vector2Int tilePos in set)
                {
                    x = tilePos.x;
                    y = tilePos.y;
                    if (tiles[x, y] != null && tiles[x, y].Mesh == null)
                    {
                        tiles[x, y].Mesh = TileMesh(tilePos, meshRotation);
                        tiles[x, y].gameObject.name += " (Corridor)";
                    }
                }
            }
        }

        if (destroyAtEnd)
            Destroy(baseObj);
    }
}
