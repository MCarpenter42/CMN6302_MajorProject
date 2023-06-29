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

    public TileType type = TileType.None;
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
    public float chanceForEmpty = 0.0f;

    public bool ConnectedAt(int ind)
    {
        ind.WrapClamp(0, 5);
        return !(connections[ind] == ConnectionState.None || connections[ind] == ConnectionState.Block);
    }
}
