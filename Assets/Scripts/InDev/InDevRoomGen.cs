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
        GenerateV2(size, centre);
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
                        tiles[x, y].gameObject.GetComponent<MeshFilter>().sharedMesh = TileMesh(new Vector2Int(x, y), meshRotation);
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
                tiles[x + offset.x, y + offset.y] = tile;
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
                tiles[x + offset.x, y + offset.y] = tile;
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
                                emptyChance = emptyChance <= 0.98f ? emptyChance += 0.02f : 1.0f;
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
                /*string str = "(" + x + ", " + y + ") --> ";
                foreach (Vector2Int vect in toAdd)
                {
                    str += "(" + vect.x + ", " + vect.y + "), ";
                }
                Debug.Log(str);*/
                newGen.AddRange(toAdd);
            }
            generateFrom.Clear();
            generateFrom.CopyFrom(newGen);
        }

        for (x = boundsMin.x; x <= boundsMax.x; x++)
        {
            for (y = boundsMin.y; y <= boundsMax.y; y++)
            {
                /*if (x < 0 || y < 0)
                    Debug.Log(x + ", " + y + " | " + (tiles[x, y] != null));*/
                if (tiles[x, y] != null && tiles[x, y].emptySpace)
                {
                    Destroy(tiles[x, y].gameObject);
                }
            }
        }

        GenerateAllMeshes();

        if (destroyAtEnd)
            Destroy(baseObj);
    }
}
