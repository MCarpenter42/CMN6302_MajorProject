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
//using NeoCambion.Random;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;

public class LevelTile : MonoBehaviour
{
    public enum TileType { None, Empty, Corridor, Room }
    public enum ConnectionState { None, Block, Connect, Merge }

    private TileType _type = TileType.None;
    public TileType type
    {
        get
        {
            if (_type == TileType.None)
            {
                int t = Random.Range(1, 4);
                _type = (TileType)t;
            }
            return _type;
        }
        set
        {
            _type = value;
        }
    }
    public bool emptySpace { get { return type == TileType.None || type == TileType.Empty; } }
    public ConnectionState[] connections = new ConnectionState[6]
    {
        ConnectionState.None,
        ConnectionState.None,
        ConnectionState.None,
        ConnectionState.None,
        ConnectionState.None,
        ConnectionState.None
    };
}
