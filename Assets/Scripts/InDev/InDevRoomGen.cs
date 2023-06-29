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
    [SerializeField] Mesh connectorL;
    [SerializeField] Mesh connectorR;
    [SerializeField] Mesh corridorConn120;
    [SerializeField] Mesh corridorConn180;
    [SerializeField] Mesh corridorConn240;
    [SerializeField] Mesh corridorConn300;
    [SerializeField] Mesh corridorEnd;
    [SerializeField] Mesh doorway;
    [SerializeField] Mesh wall;
    [SerializeField] Mesh wallCorner;

    private Grid2D<LevelTile> tiles = new Grid2D<LevelTile>();

    #endregion

    #region [ PROPERTIES ]

    public static float zOffset { get { return Mathf.Sin(60.0f.ToRad()); } }
    public float roomRadius = 6.0f;
    [SerializeField] float meshRotation = 180.0f;
    [SerializeField] Vector2Int size = Vector2Int.one;
    [SerializeField] Vector2Int centre = Vector2Int.zero;

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
        Generate(size, centre);
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

    private Mesh TileMesh(Vector2Int coords, float rotateByRadians = 0.0f)
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
    }
    
    private void TileModel(Vector2Int coords, float rotateByRadians = 0.0f)
    {
        GameObject baseObj = new GameObject();
        baseObj.AddComponent<MeshFilter>();
        baseObj.AddComponent<MeshRenderer>().material = tileTemplate == null ? null : tileTemplate.GetComponent<MeshRenderer>().sharedMaterial;
        LevelTile.ConnectionState[] connections = tiles[coords.x, coords.y].connections;
        if (tiles[coords.x, coords.y].type == LevelTile.TileType.Room)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                int iMinus = i > 0 ? i - 1 : connections.Length - 1;
                int iPlus = i < connections.Length - 1 ? i + 1 : 0;
                Vector3 rot = new Vector3(0.0f, (float)i * 60.0f + rotateByRadians.ToDeg(), 0.0f);
                if (connections[i] != LevelTile.ConnectionState.Merge)
                {
                    if (connections[i] == LevelTile.ConnectionState.None || connections[i] == LevelTile.ConnectionState.Block)
                    {
                        GameObject child = Instantiate(baseObj, tiles[coords.x, coords.y].gameObject.transform);
                        child.GetComponent<MeshFilter>().mesh = wall;
                        child.transform.eulerAngles = rot;
                    }
                    else if (connections[i] == LevelTile.ConnectionState.Connect)
                    {
                        GameObject child = Instantiate(baseObj, tiles[coords.x, coords.y].gameObject.transform);
                        child.GetComponent<MeshFilter>().mesh = doorway;
                        child.transform.eulerAngles = rot;
                    }
                    if (connections[iPlus] != LevelTile.ConnectionState.Merge)
                    {
                        GameObject child = Instantiate(baseObj, tiles[coords.x, coords.y].gameObject.transform);
                        child.GetComponent<MeshFilter>().mesh = wallCorner;
                        child.transform.eulerAngles = rot;
                    }
                }
                else if (connections[i] == LevelTile.ConnectionState.Merge)
                {
                    if (connections[iMinus] == LevelTile.ConnectionState.Merge)
                    { }
                    else
                    {
                        GameObject child = Instantiate(baseObj, tiles[coords.x, coords.y].gameObject.transform);
                        child.GetComponent<MeshFilter>().mesh = connectorL;
                        child.transform.eulerAngles = rot;
                    }

                    if (connections[iPlus] == LevelTile.ConnectionState.Merge)
                    { }
                    else
                    {
                        GameObject child = Instantiate(baseObj, tiles[coords.x, coords.y].gameObject.transform);
                        child.GetComponent<MeshFilter>().mesh = connectorR;
                        child.transform.eulerAngles = rot;
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
                Vector3 rot = new Vector3(0.0f, (float)i * 60.0f + rotateByRadians.ToDeg(), 0.0f);
                if (connections[i] == LevelTile.ConnectionState.Connect || connections[i] == LevelTile.ConnectionState.Merge)
                {
                    GameObject child = Instantiate(baseObj, tiles[coords.x, coords.y].gameObject.transform);
                    child.GetComponent<MeshFilter>().mesh = corridorEnd;
                    child.transform.eulerAngles = rot;
                }
            }
        }
        Destroy(baseObj);
    }

    public void Generate(Vector2Int size)
    {
        Generate(size, new Vector2Int(-1, -1));
    }

    public void Generate(Vector2Int size, Vector2Int centre)
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
        if (centre.y >= size.y)
            offset.y += 1;

        tiles.Clear();

        int x, x_, y, y_;
        for (x_ = 0; x_ < size.x; x_++)
        {
            for (y_ = 0; y_ < size.y; y_++)
            {
                x = x_ + offset.x;
                y = y_ + offset.y;
                GameObject tileObj = Instantiate(baseObj, transform);
                tileObj.transform.position = TilePosition(x, y);
                LevelTile tile = tileObj.GetComponent<LevelTile>();
                tiles[x + offset.x, y + offset.y] = tile;
                tileObj.name = "Tile [" + x + ", " + y + "] - " + tile.type;
            }
        }

        for (x_ = 0; x_ < size.x; x_++)
        {
            for (y_ = 0; y_ < size.y; y_++)
            {
                x = x_ + offset.x;
                y = y_ + offset.y;
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
                            corridor = tiles[x, y].type == LevelTile.TileType.Corridor || tiles[x2, y2].type == LevelTile.TileType.Corridor;
                            if (tiles[x2, y2] == null || tiles[x2, y2].emptySpace)
                            {
                                tiles[x, y].connections[i] = LevelTile.ConnectionState.Block;
                            }
                            else
                            {
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

        for (x_ = 0; x_ < size.x; x_++)
        {
            for (y_ = 0; y_ < size.y; y_++)
            {
                x = x_ + offset.x;
                y = y_ + offset.y;
                if (tiles[x, y] != null)
                {
                    if (!tiles[x, y].emptySpace)
                    {
                        //tiles[x, y].gameObject.GetComponent<MeshFilter>().mesh = TileMesh(new Vector2Int(x, y), meshRotation.ToRad());
                        tiles[x, y].gameObject.GetComponent<MeshRenderer>().enabled = false;
                        TileModel(new Vector2Int(x, y), meshRotation.ToRad());
                    }
                    else
                    {
                        tiles[x, y].gameObject.GetComponent<MeshRenderer>().enabled = false;
                    }
                }
            }
        }

        if (destroyAtEnd)
            Destroy(baseObj);
    }
}
