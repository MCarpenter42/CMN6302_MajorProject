using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Maths;
using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class LevelTile : Core
{
    public enum TileType { None, Empty, Corridor, Room }
    public enum ConnectionState { None, Block, Connect, Merge }
    public struct HexConnections
    {
        public readonly int Length { get { return _values.Length; } }

        private ConnectionState[] _values;
        public ConnectionState[] values
        {
            get
            {
                if (_values == null || _values.Length < 6)
                    _values = new ConnectionState[6];
                return _values;
            }
            set
            {
                _values = value;
            }
        }
        public ConnectionState this[int index]
        {
            get
            {
                if (index < 0)
                    return values[0];
                else if (index > 5)
                    return values[5];
                else
                    return values[index];
            }
            set
            {
                if (index < 0)
                    values[0] = value;
                else if (index > 5)
                    values[5] = value;
                else
                    values[index] = value;
            }
        }
        public ConnectionState this[HexGridDirection direction]
        {
            get
            {
                switch (direction)
                {
                    default:
                    case HexGridDirection.Up: return values[0];
                    case HexGridDirection.UpRight: return values[1];
                    case HexGridDirection.DownRight: return values[2];
                    case HexGridDirection.Down: return values[3];
                    case HexGridDirection.DownLeft: return values[4];
                    case HexGridDirection.UpLeft: return values[5];
                }
            }
            set
            {
                switch (direction)
                {
                    default:
                    case HexGridDirection.Up: values[0] = value; break;
                    case HexGridDirection.UpRight: values[1] = value; break;
                    case HexGridDirection.DownRight: values[2] = value; break;
                    case HexGridDirection.Down: values[3] = value; break;
                    case HexGridDirection.DownLeft: values[4] = value; break;
                    case HexGridDirection.UpLeft: values[5] = value; break;
                }
            }
        }

        public HexConnections(ConnectionState[] values)
        {
            _values = values;
        }
        public HexConnections(ConnectionState setAllTo = ConnectionState.None)
        {
            _values = new ConnectionState[6];
            for (int i = 0; i < 6; i++)
            {
                values[i] = setAllTo;
            }
        }

        public bool fullyMerged
        {
            get
            {
                foreach (ConnectionState conn in values)
                {
                    if (conn != ConnectionState.Merge)
                        return false;
                }
                return true;
            }
        }

        public bool Connected(int index) => !(values[index.WrapClamp(0, 5)] == ConnectionState.None || values[index.WrapClamp(0, 5)] == ConnectionState.Block);
        public bool Connected(HexGridDirection direction) => !(this[direction] == ConnectionState.None || this[direction] == ConnectionState.Block);

        public void Overwrite(IList<int> newValues)
        {
            if (_values == null)
                values = new ConnectionState[6];
            for (int i = 0; i < _values.Length && i < newValues.Count; i++)
            {
                _values[i] = (ConnectionState)newValues[i];
            }
        }
        public void Overwrite(IList<ConnectionState> newValues)
        {
            if (_values == null)
                values = new ConnectionState[6];
            for (int i = 0; i < _values.Length && i < newValues.Count; i++)
            {
                _values[i] = newValues[i];
            }
        }
        public void Replace(ConnectionState replace = ConnectionState.None, ConnectionState replaceWith = ConnectionState.Block)
        {
            for (int i = 0; i < Length; i++)
            {
                if (values[i] == replace)
                    values[i] = replaceWith;
            }
        }

        public int CountOf(ConnectionState checkState)
        {
            int i = 0;
            foreach (ConnectionState state in _values)
            {
                if (state == checkState)
                    i++;
            }
            return i;
        }

        private static char StateChar(ConnectionState state)
        {
            switch (state)
            {
                default: return '?';
                case ConnectionState.None: return 'N';
                case ConnectionState.Block: return 'B';
                case ConnectionState.Connect: return 'C';
                case ConnectionState.Merge: return 'M';
            }
        }
        public override string ToString() => "[ " + values[0].ToString() + " | " + values[1].ToString() + " | " + values[2].ToString() + " | " + values[3].ToString() + " | " + values[4].ToString() + " | " + values[5].ToString() + " ]";
        public string ToString(bool singleCharacters) => singleCharacters ? "[ " + StateChar(values[0]) + " | " + StateChar(values[1]) + " | " + StateChar(values[2]) + " | " + StateChar(values[3]) + " | " + StateChar(values[4]) + " | " + StateChar(values[5]) + " ]" : ToString();
    }

    public TileType type = TileType.None;
    public bool emptySpace { get { return type == TileType.None || type == TileType.Empty; } }
    public float chanceForEmpty = 0f;

    public HexConnections connections = new HexConnections();
    public bool fullyMerged => connections.fullyMerged;

    public bool isCorridorEndcap
    {
        get
        {
            return type == TileType.Corridor && connections.CountOf(ConnectionState.Block) == 5;
        }
    }

    public bool ConnectedAt(int ind) => connections.Connected(ind);
    public bool ConnectedAt(HexGridDirection direction) => connections.Connected(direction);

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static string NoName = "UNASSIGNED";
    public string objName { get { return gameObject.name; } set { gameObject.name = value; } }

    public static Vector3 NoPos = new Vector3(0, -20, 0);
    public Vector3 position { get { return gameObject.transform.position; } set { gameObject.transform.position = value; } }
    public Vector3 localPosition { get { return gameObject.transform.localPosition; } set { gameObject.transform.localPosition = value; } }
    public Vec2Int gridPosition { get; private set; }

    public Mesh mesh { get { return GetComponent<MeshFilter>().sharedMesh; } set { GetComponent<MeshFilter>().sharedMesh = value; } }
    public MeshFilter meshFilter => GetComponent<MeshFilter>();
    public MeshRenderer meshRenderer => GetComponent<MeshRenderer>();
    public Material material { get { return meshRenderer.sharedMaterial; } set { meshRenderer.sharedMaterial = value; } }

    private GameObject _colliderObj = null;
    public GameObject colliderObj => _colliderObj ?? (_colliderObj = NewObject("Collider", transform, transform.position, typeof(MeshFilter), typeof(MeshCollider)));
    public Mesh colliderMesh { get { return colliderObj.GetComponent<MeshCollider>().sharedMesh; } set { colliderObj.GetComponent<MeshCollider>().sharedMesh = value; } }

    public List<string> additionalInfo { get; private set; }

    public void AddAdditionalInfo(string info)
    {
        if (additionalInfo == null)
            additionalInfo = new List<string>();
        if (info != null)
            additionalInfo.Add(" • " + info);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SetTransformParent(LevelRoom parentRoom)
    {
        if (type != TileType.Corridor)
            transform.SetParent(parentRoom.transform, true);
    }
    
    public void SetTransformParent(LevelCorridor parentCorridor)
    {
        if (type != TileType.Room)
            transform.SetParent(parentCorridor.transform, true);
    }

    public void SetAttributes(Vec2Int gridPosition, Vector3 worldPosition, TileType type)
    {
        this.gridPosition = gridPosition;
        position = worldPosition;
        name = "Tile " + gridPosition.ToString(AsciiBracketing.BracketSquare) + " (" + type.ToString() + ")";
        this.type = type;
    }

    public void ReplaceConnections(ConnectionState replace, ConnectionState repaceWith)
    {
        for (int i = 0; i < 6; i++)
        {
            if (connections[i] == replace)
                connections[i] = repaceWith;
        }
    }

    public void SetVisuals(Mesh mesh, Material material)
    {
        this.mesh = mesh;
        this.material = material;
    }

    public void SetCollider(Mesh mesh) => colliderMesh = mesh;
    
    public void SetMeshesAndMaterial()
    {
        Mesh mesh = null, cMesh = null;
        Material material = null;
        if (type == TileType.Corridor)
        {
            (mesh, cMesh) = LevelManager.Generator.CorridorTileMesh(connections);
            material = LevelManager.Generator.MatWallCorridor;
        }
        else if (type == TileType.Room)
        {
            (mesh, cMesh) = LevelManager.Generator.RoomTileMesh(connections);
            material = LevelManager.Generator.MatWallRoom;
        }
        GetComponent<MeshFilter>().sharedMesh = mesh;
        colliderObj.GetComponent<MeshCollider>().sharedMesh = cMesh;
        GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public GameObject miniTile { get; private set; }

    public void SpawnMiniTile(Transform parentTransform, Material miniTileMat, bool revealed = false)
    {
        miniTile = NewObject(objName + " (Minimap)", parentTransform, typeof(MeshFilter), typeof(MeshRenderer));
        miniTile.GetComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
        MeshRenderer renderer = miniTile.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = miniTileMat == null ? Ext_Material.DefaultDiffuse : miniTileMat;
        renderer.receiveShadows = false;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        miniTile.transform.localPosition = position;
        miniTile.transform.localScale = Vector3.one;
        ShowMiniTile(revealed);
    }

    public void ShowMiniTile(bool show)
    {
        if (miniTile != null)
            miniTile.SetActive(show);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public float distToPlayer => (GameManager.Instance.Player.transform.position - transform.position).magnitude;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelTile))]
public class LevelTileEditor : Editor
{
    LevelTile targ { get { return target as LevelTile; } }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStylesExtras.noMarginsNoPadding);
        {
            EditorGUILayout.BeginVertical(EditorStylesExtras.noMarginsNoPadding);
            {
                EditorGUILayout.Space(2);
                EditorElements.SectionHeader("Tile Information");
                EditorGUILayout.Space(0);
                EditorGUI.LabelField(EditorElements.PrefixLabel(new GUIContent("Tile Type")), new GUIContent(targ.type.ToString()));
                if (targ.additionalInfo != null)
                {
                    EditorGUILayout.Space(6);
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

public class TileSaveData
{
    public Vec2Int gridPosition;
    public LevelTile.TileType type;
    public LevelTile.ConnectionState[] connections;
    public string DataString => ToDataString();

    public TileSaveData(Vec2Int gridPosition, LevelTile.TileType type, LevelTile.ConnectionState[] connections)
    {
        this.gridPosition = gridPosition;
        this.type = type;
        this.connections = connections;
    }
    public TileSaveData(LevelTile source)
    {
        gridPosition = source.gridPosition;
        type = source.type;
        connections = source.connections.values;
    }

    public static TileSaveData FromDataString(string dataString)
    {
        if (dataString[0] == '{')
            dataString = dataString[1..^1];

        Vec2Int gridPosition = Vec2Int.zero;
        LevelTile.TileType type;
        LevelTile.ConnectionState[] connections = new LevelTile.ConnectionState[6];

        int i = 0, search = 0, delim = dataString.IndexOf(',', search);
        gridPosition.x = dataString.RangeToInt(search, delim);
        search = delim + 1; delim = dataString.IndexOf(';', search);
        gridPosition.y = dataString.RangeToInt(search, delim);

        search = delim + 1; delim = dataString.IndexOf(';', search);
        type = (LevelTile.TileType)dataString.RangeToInt(search, delim);
        
        search = delim + 1; delim = dataString.IndexOf(',', search);
        connections[0] = (LevelTile.ConnectionState)dataString.RangeToInt(search, delim);
        search = delim + 1; delim = dataString.IndexOf(',', search);
        connections[1] = (LevelTile.ConnectionState)dataString.RangeToInt(search, delim);
        search = delim + 1; delim = dataString.IndexOf(',', search);
        connections[2] = (LevelTile.ConnectionState)dataString.RangeToInt(search, delim);
        search = delim + 1; delim = dataString.IndexOf(',', search);
        connections[3] = (LevelTile.ConnectionState)dataString.RangeToInt(search, delim);
        search = delim + 1; delim = dataString.IndexOf(',', search);
        connections[4] = (LevelTile.ConnectionState)dataString.RangeToInt(search, delim);
        search = delim + 1; delim = dataString.IndexOf(';', search);
        connections[5] = (LevelTile.ConnectionState)dataString.RangeToInt(search, delim);

        return new TileSaveData(gridPosition, type, connections);
    }
    public string ToDataString()
    {
        string dataString = "{" + gridPosition.x + ',' + gridPosition.y + ';' + (int)type + ';' + (int)(connections[0]);
        for (int i = 1; i < connections.Length; i++)
        {
            dataString += "" + ',' + (int)(connections[i]);
        }
        return dataString += ";}";
    }
}
