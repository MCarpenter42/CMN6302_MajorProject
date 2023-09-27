using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Encryption;
using NeoCambion.IO;
using UnityEngine.Playables;
using JetBrains.Annotations;

public class GameDataStorage
{
    private static char pathSep => Application.isEditor ? '/' : Path.DirectorySeparatorChar;

    public static GameDataStorage Data => GameManager.Instance.GameData;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private static string cacheFolder => Application.dataPath + "/Resources/Data/";
    private static string cacheFile_Enemies = "_EnemyData.json";
    private static string cachePath_Enemies => cacheFolder + cacheFile_Enemies;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private List<EnemyData> enemyData = null;
    public List<EnemyData> EnemyData
    {
        get
        {
            if (enemyData == null || enemyData.Count < 1)
                enemyData = LoadEnemyCache();
            return enemyData;
        }
    }
    private List<ItemData> itemData = null;
    public List<ItemData> ItemData
    {
        get
        {
            if (itemData == null || itemData.Count < 1)
                itemData = new List<ItemData>();
            return itemData;
        }
    }

    public void RuntimeLoad()
    {
        enemyData = new List<EnemyData>();
        itemData = new List<ItemData>();

        string enemyJson, itemJson;
        string[] enemyStrings, itemStrings;

        enemyJson = GameManager.Instance.dataJSON_enemies.text;
        if (enemyJson != null && enemyJson.Length > 0)
        {
            enemyStrings = enemyJson.Split(System.Environment.NewLine, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string dataString in enemyStrings)
            {
                enemyData.Add(JsonUtility.FromJson<EnemyData>(dataString));
            }
        }
        /*itemJson = GameManager.Instance.dataJSON_enemies.text;
        if (itemJson != null && itemJson.Length > 0)
        {
            itemStrings = itemJson.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            foreach (string dataString in itemStrings)
            {
                itemData.Add(JsonUtility.FromJson<ItemData>(dataString));
            }
        }*/
    }

    public void EditorLoad()
    {
        enemyData = LoadEnemyCache();
    }

    public static void SaveEnemyCache(List<EnemyData> list)
    {
        if (!Application.isPlaying)
        {
            string[] dataStrings = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                dataStrings[i] = JsonUtility.ToJson(list[i]);
            }
            File.WriteAllLines(cachePath_Enemies, dataStrings);
        }
    }

    public static List<EnemyData> LoadEnemyCache()
    {
        List<EnemyData> list = new List<EnemyData>();
        if (File.Exists(cachePath_Enemies))
        {
            string[] dataStrings = File.ReadAllLines(cachePath_Enemies);
            foreach (string dataString in dataStrings)
            {
                list.Add(JsonUtility.FromJson<EnemyData>(dataString));
            }
        }
        else
        {
            File.Create(cachePath_Enemies);
        }
        return list;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private static string appDirectory => (Application.isEditor ? (Application.dataPath + "/Resources/Data") : Application.persistentDataPath);
    private static string metaDataExt => Application.isEditor ? ".metadata" : ".meta";
    private static string metadata => appDirectory + pathSep + "data" + metaDataExt;

    [System.Serializable]
    public class MetaStorage
    {
        [SerializeField] int latestRun = -1;
        public int LatestRun
        {
            get { return latestRun; }
            set { latestRun = value; WriteMetadata(); }
        }
        [SerializeField] bool activeRun = false;
        public bool ActiveRun
        {
            get { return activeRun; }
            set { activeRun = value; WriteMetadata(); }
        }
        [SerializeField] Random.State? randState = null;
        public Random.State? RandState
        {
            get { return randState; }
            set { randState = value; WriteMetadata(); }
        }
        [SerializeField] RandTuningSaveData tuningData = null;
        public RandTuningSaveData TuningData
        {
            get { return tuningData; }
            set { tuningData = value; WriteMetadata(); }
        }

        public MetaStorage(int latestRun, bool activeRun, Random.State? randState, RandTuningSaveData tuningData)
        {
            this.latestRun = latestRun;
            this.activeRun = activeRun;
            this.randState = randState;
            this.tuningData = tuningData;
        }

        public static MetaStorage New => new MetaStorage(0, false, null, null);
    }
    public static MetaStorage MetaData = null;
    public static bool FreshInstall = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ReadMetadata()
    {
        FileHandler.ValidateFile(metadata);
        MetaData = JsonUtility.FromJson<MetaStorage>(FileHandler.ReadString(metadata));
        if (MetaData == null)
        {
            MetaData = MetaStorage.New;
            WriteMetadata();
        }
    }

    public static void WriteMetadata() => FileHandler.Write(JsonUtility.ToJson(MetaData), metadata);

    public static void GetRandomState()
    {
        if (MetaData == null || MetaData.RandState == null)
            Random.InitState((int)System.DateTime.Now.Ticks);
        else
            Random.state = MetaData.RandState.Value;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private static string runDirectory => appDirectory + pathSep + "CurrentRun";
    private static string currentMapGen => runDirectory + pathSep + "generation_latest.dat";
    private static string currentLvlPop => runDirectory + pathSep + "population_latest.dat";
    private static string runProgression => runDirectory + pathSep + "progression.dat";

    private static string runArchive => appDirectory + pathSep + "RunArchive";
    private static string RunArchive(int runIndex) => runArchive + pathSep + "Run_" + runIndex.ToString();
    private static string runArchiveMetadata => runArchive + pathSep + "archive" + metaDataExt;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public RunDataActive runData = RunDataActive.New();
    public PlayerData[] playerData = null;
    //public List<ItemCore> inventory = new List<ItemCore>();

    public static bool CheckForActiveRun()
    {
        bool mapFile = File.Exists(currentMapGen);
        bool popFile = File.Exists(currentLvlPop);
        bool runFile = File.Exists(runProgression);
        return mapFile && popFile && runFile;
    }

    public static void SavePlayData(MapGenSaveData mapGenData)
    {
        if (!Directory.Exists(runDirectory))
            Directory.CreateDirectory(runDirectory);
        string[] dataStrings = mapGenData.DataStrings;
        FileHandler.ValidateFile(currentMapGen);
        FileHandler.Write(dataStrings, currentMapGen, true);
    }
    public static void SavePlayData(LevelPopSaveData lvlPopData)
    {
        if (!Directory.Exists(runDirectory))
            Directory.CreateDirectory(runDirectory);
        string[] dataStrings = lvlPopData.DataStrings;
        FileHandler.ValidateFile(currentLvlPop);
        FileHandler.Write(dataStrings, currentLvlPop, true);
    }
    public static void SavePlayData(RunSaveData runData)
    {
        if (!Directory.Exists(runDirectory))
            Directory.CreateDirectory(runDirectory);
        string[] dataStrings = runData.DataStrings;
        FileHandler.ValidateFile(runProgression);
        FileHandler.Write(dataStrings, runProgression, true);
    }

    public void SaveRunData() => SavePlayData(new RunSaveData(runData));
    private void ValidateAchive(int runIndex)
    {
        string directory = RunArchive(runIndex);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            List<string> existingData = new List<string>(FileHandler.ReadLines(runArchiveMetadata)) { runIndex.ToString() + ">>" + directory };
            FileHandler.Write(existingData.ToArray(), runArchiveMetadata);
        }
    }
    public void ArchiveRun()
    {
        ValidateAchive(MetaData.LatestRun);
        SaveRunData();
        string[] fileContents = FileHandler.ReadLines(runProgression);
        FileHandler.Write(fileContents, RunArchive(MetaData.LatestRun) + pathSep + "runInfo.dat");
        MetaData.ActiveRun = false;
        FileHandler.Delete(currentMapGen, currentLvlPop, runProgression);
    }

    public static SaveDataWrapper LoadPlayData()
    {
        if (!Directory.Exists(runDirectory))
            Directory.CreateDirectory(runDirectory);
        SaveDataWrapper saveData = new SaveDataWrapper();
        saveData.mapGenData = MapGenSaveData.FromDataStrings(FileHandler.ReadLines(currentMapGen));
        saveData.lvlPopData = LevelPopSaveData.FromDataStrings(FileHandler.ReadLines(currentLvlPop));
        saveData.runData = RunSaveData.FromDataStrings(FileHandler.ReadLines(runProgression));

        if (Data.runData == null)
            Data.runData = new RunDataActive();
        Data.GetStartingPlayerData();
        Data.runData.GetFromSave(saveData.runData);
        for (int i = 0; i < 4; i++)
        {
            Data.playerData[i].currentHealthPercent = Data.runData.p_healthPercentages[i];
        }

        if (MetaData.RandState != null)
            Random.state = MetaData.RandState.Value;
        Core.RandTuning.SetFromSaveData(MetaData.TuningData);
        return saveData;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void NewRunData()
    {
        GetStartingPlayerData();
        for (int i = 0; i < 4; i++)
        {
            runData.p_healthValues[i] = new int[] { playerData[i].baseHealth, playerData[i].baseHealth };
        }
        Core.RandTuning.NewRandomness();
        MetaData.LatestRun++;
        MetaData.ActiveRun = true;
        MetaData.TuningData = Core.RandTuning.CurrentToSaveData();
    }

    public void GetStartingPlayerData()
    {
        if (playerData == null || playerData.Length != 4)
            playerData = new PlayerData[4];
        playerData[0] = new PlayerData()
        {
            displayName = "Player Character 1",
            modelHexUID = "8663CC29",
            friendly = true, playerControlled = true,
             baseHealth = 190,  healthScaling = 060,
             baseAttack = 060,  attackScaling = 040,
            baseDefence = 015, defenceScaling = 020,
            speeds = new SpeedAtLevel[] { new SpeedAtLevel(0, 130) },
            attackType = DamageType.Type.None,
            actionSet = ActionSetName.PLAYER_A,
            currentHealthPercent = 1f,
        };
        playerData[1] = new PlayerData()
        {
            displayName = "Player Character 2",
            modelHexUID = "E0E81A8C",
            friendly = true, playerControlled = true,
             baseHealth = 280,  healthScaling = 060,
             baseAttack = 044,  attackScaling = 040,
            baseDefence = 015, defenceScaling = 020,
            speeds = new SpeedAtLevel[] { new SpeedAtLevel(0, 118) },
            attackType = DamageType.Type.None,
            actionSet = ActionSetName.PLAYER_B,
            currentHealthPercent = 1f,
        };
        playerData[2] = new PlayerData()
        {
            displayName = "Player Character 3",
            modelHexUID = "ED15DEC8",
            friendly = true, playerControlled = true,
             baseHealth = 215,  healthScaling = 060,
             baseAttack = 064,  attackScaling = 040,
            baseDefence = 015, defenceScaling = 020,
            speeds = new SpeedAtLevel[] { new SpeedAtLevel(0, 106) },
            attackType = DamageType.Type.None,
            actionSet = ActionSetName.PLAYER_C,
            currentHealthPercent = 1f,
        };
        playerData[3] = new PlayerData()
        {
            displayName = "Player Character 4",
            modelHexUID = "ECBADC05",
            friendly = true, playerControlled = true,
             baseHealth = 250,  healthScaling = 060,
             baseAttack = 036,  attackScaling = 040,
            baseDefence = 015, defenceScaling = 020,
            speeds = new SpeedAtLevel[] { new SpeedAtLevel(0, 098) },
            attackType = DamageType.Type.None,
            actionSet = ActionSetName.PLAYER_D,
            currentHealthPercent = 1f,
        };
    }
}

public static class SaveDataUtility
{
    public static TileSaveData[] GetSaveData(this ICollection<LevelTile> tiles)
    {
        TileSaveData[] data = new TileSaveData[tiles.Count];
        int i = 0;
        foreach (LevelTile tile in tiles)
        {
            data[i++] = new TileSaveData(tile);
        }
        return data;
    }
    public static RoomSaveData[] GetSaveData(this ICollection<LevelRoom> rooms)
    {
        RoomSaveData[] data = new RoomSaveData[rooms.Count];
        int i = 0;
        foreach (LevelRoom room in rooms)
        {
            data[i++] = new RoomSaveData(room);
        }
        return data;
    }
    public static CorridorSaveData[] GetSaveData(this ICollection<LevelCorridor> corridors)
    {
        CorridorSaveData[] data = new CorridorSaveData[corridors.Count];
        int i = 0;
        foreach (LevelCorridor corridor in corridors)
        {
            data[i++] = new CorridorSaveData(corridor);
        }
        return data;
    }
    public static EnemySetSaveData[] GetSaveData(this ICollection<WorldEnemySet> enemies)
    {
        EnemySetSaveData[] result = new EnemySetSaveData[enemies.Count];
        int i = 0;
        foreach (WorldEnemySet set in enemies)
        {
            result[i++] = new EnemySetSaveData(set);
        }
        return result;
    }
    public static WorldItemSaveData[] GetSaveData(this ICollection<WorldItem> items)
    {
        WorldItemSaveData[] result = new WorldItemSaveData[items.Count];
        int i = 0;
        foreach (WorldItem item in items)
        {
            result[i++] = new WorldItemSaveData(item);
        }
        return result;
    }
}

public class SaveDataWrapper
{
    public MapGenSaveData mapGenData;
    public LevelPopSaveData lvlPopData;
    public RunSaveData runData;
}

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

public static class ElementDataUtility
{
    public static CombatantData[] AsData(this CombatantType[] enemies)
    {
        CombatantData[] output = new CombatantData[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            output[i] = enemies[i].data;
        }
        return output;
    }

    public static List<EnemyData>[] Categorise(this List<EnemyData> data)
    {
        List<EnemyData> standard = new List<EnemyData>(), elite = new List<EnemyData>(), boss = new List<EnemyData>(), minion = new List<EnemyData>();
        foreach (EnemyData enemy in data)
        {
            switch (enemy.Class)
            {
                default: break;
                case EnemyClass.Standard: standard.Add(enemy); break;
                case EnemyClass.Elite: elite.Add(enemy); break;
                case EnemyClass.Boss: boss.Add(enemy); break;
                case EnemyClass.Minion: minion.Add(enemy); break;
            }
        }
        return new List<EnemyData>[] { standard, elite, boss, minion };
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ElementList<>))]
public class ElementListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int count = property.FindPropertyRelative("Data").arraySize;
        string lblText = property.FindPropertyRelative("label").stringValue;
        EditorGUI.BeginProperty(position, label, property);
        {
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(lblText));
            EditorGUI.LabelField(position, new GUIContent(count.ToString()));
        }
        EditorGUI.EndProperty();
    }
}
#endif

[System.Serializable]
public struct ElementList<ElementData>
{
    public List<ElementData> Data;
    public string label;
    public ElementList(List<ElementData> data, string label)
    {
        Data = data == null ? new List<ElementData>() : data;
        this.label = label;
    }
}

[System.Serializable]
public class ElementData
{
    public string displayName;
}

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

[System.Serializable]
public class EnemyDataWrapper
{
    public EnemyData[] data;
    public EnemyDataWrapper(EnemyData[] data) { this.data = data; }
    public EnemyDataWrapper(List<EnemyData> data) { this.data = data.ToArray(); }
}

[System.Serializable]
public class CombatantData : ElementData
{
    public string modelHexUID;
    public bool friendly;
    public bool playerControlled;

    public int baseHealth = 50;
    public int healthScaling;

    public int baseAttack;
    public int attackScaling;

    public int baseDefence;
    public int defenceScaling;

    public SpeedAtLevel[] speeds = new SpeedAtLevel[0];

    public int inflictChance = 40;
    public int inflictResist = 0;

    public DamageType.Type attackType;
    public bool[] weakAgainst = new bool[DamageType.TypeCount];

    public ActionSetName actionSet;

    public override string ToString()
    {
        return $"Combatant Type \"{displayName}\"";
    }
}

[System.Serializable]
public class PlayerData : CombatantData
{
    public float currentHealthPercent = 1f;
    public Equipment equipment;
}

[System.Serializable]
public class EnemyData : CombatantData
{
    public EnemyClass Class;
    public bool wanderInWorld = true;

    public static EnemyData[] DataFromPool(EnemyClass enemyClass, int count)
    {
        EnemyData[] dataOut = new EnemyData[count];
        List<EnemyData>[] categorised = GameManager.Instance.GameData.EnemyData.Categorise();
        switch (enemyClass)
        {
            default:
                break;

            case EnemyClass.Standard:
                for (int i = 0; i < count; i++)
                {
                    dataOut[i] = categorised[0][Random.Range(0, categorised[0].Count)];
                }
                break;

            case EnemyClass.Elite:
                for (int i = 0; i < count; i++)
                {
                    dataOut[i] = categorised[1][Random.Range(0, categorised[1].Count)];
                }
                break;

            case EnemyClass.Boss:
                for (int i = 0; i < count; i++)
                {
                    dataOut[i] = categorised[2][Random.Range(0, categorised[2].Count)];
                }
                break;

            case EnemyClass.Minion:
                for (int i = 0; i < count; i++)
                {
                    dataOut[i] = categorised[3][Random.Range(0, categorised[3].Count)];
                }
                break;
        }
        return dataOut;
    }
    public static EnemyData[] DataFromPool(EnemyClass[] classes)
    {
        EnemyData[] dataOut = new EnemyData[classes.Length];
        List<EnemyData>[] categorised = GameManager.Instance.GameData.EnemyData.Categorise();
        for (int i = 0; i < classes.Length; i++)
        {
            switch (classes[i])
            {
                default:
                    break;

                case EnemyClass.Standard:
                    dataOut[i] = categorised[0][Random.Range(0, categorised[0].Count)];
                    break;

                case EnemyClass.Elite:
                    dataOut[i] = categorised[1][Random.Range(0, categorised[1].Count)];
                    break;

                case EnemyClass.Boss:
                    dataOut[i] = categorised[2][Random.Range(0, categorised[2].Count)];
                    break;

                case EnemyClass.Minion:
                    dataOut[i] = categorised[3][Random.Range(0, categorised[3].Count)];
                    break;
            }
        }
        return dataOut;
    }
    public static int ClassDataInd(EnemyClass enemyClass)
    {
        List<EnemyData>[] categorised = GameManager.Instance.GameData.EnemyData.Categorise();
        return enemyClass switch
        {
            EnemyClass.Standard => Random.Range(0, categorised[0].Count),
            EnemyClass.Elite => Random.Range(0, categorised[1].Count),
            EnemyClass.Boss => Random.Range(0, categorised[2].Count),
            EnemyClass.Minion => Random.Range(0, categorised[3].Count),
            _ => -1
        };
    }
    public static int[] ClassDataInds(EnemyClass enemyClass, int count)
    {
        List<EnemyData>[] categorised = GameManager.Instance.GameData.EnemyData.Categorise();
        int[] inds = new int[count];
        switch (enemyClass)
        {
            case EnemyClass.Standard:
                for (int i = 0; i < count; i++)
                {
                    inds[i] = Random.Range(0, categorised[0].Count);
                }
                break;

            case EnemyClass.Elite:
                for (int i = 0; i < count; i++)
                {
                    inds[i] = Random.Range(0, categorised[1].Count);
                }
                break;

            case EnemyClass.Boss:
                for (int i = 0; i < count; i++)
                {
                    inds[i] = Random.Range(0, categorised[2].Count);
                }
                break;

            case EnemyClass.Minion:
                for (int i = 0; i < count; i++)
                {
                    inds[i] = Random.Range(0, categorised[3].Count);
                }
                break;

            default:
                inds = new int[0];
                break;
        }
        return inds;
    }
}

/*[System.Serializable]
public class EnemyData : CombatantData
{
    public EnemyData(string displayName)
    {
        this.displayName = displayName;
    }

    public override string ToString()
    {
        return $"Enemy Type \"{displayName}\"";
    }
}*/

[System.Serializable]
public class CombatantType
{
    public int typeInd;
    public CombatantData data
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameManager.Instance.UpdateElementData();
            }
#endif
            if (!GameManager.Instance.GameData.EnemyData.InBounds(typeInd))
                Debug.Log("No enemy data loaded! | " + typeInd + "/" + GameManager.Instance.GameData.EnemyData.Count);
            List<EnemyData> enemies = GameManager.Instance.GameData.EnemyData;
            return enemies.InBounds(typeInd) ? enemies[typeInd] : null;
        }
    }
    public string displayName { get { return data.displayName; } }

    public CombatantType(int typeInd = -1)
    {
        this.typeInd = typeInd;
    }

    public override string ToString()
    {
        return $"Type Index: {typeInd} | Type Name: {displayName}";
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CombatantType))]
public class CombatantTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<EnemyData> data = GameDataStorage.LoadEnemyCache();
        string[] options = new string[data.Count];
        int[] optInds = new int[data.Count].IncrementalPopulate();
        int selected = property.FindPropertyRelative("typeInd").intValue;

        EditorGUI.BeginProperty(position, label, property);
        {
            Rect elementRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (options.Length > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    options[i] = data[i].displayName;
                }
                selected = EditorGUI.IntPopup(elementRect, selected, options, optInds);
                property.FindPropertyRelative("typeInd").intValue = selected;
            }
            else
            {
                EditorGUI.LabelField(elementRect, "No enemy types available!");
                property.FindPropertyRelative("typeInd").intValue = -1;
            }
        }
        EditorGUI.EndProperty();
    }
}
#endif

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

public class ActionData
{
    public bool npcAction = false;

    public ActionData(bool npcAction = false)
    {
        this.npcAction = npcAction;
    }
}

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

[System.Serializable]
public class ItemData : ElementData
{
    public override string ToString()
    {
        return $"Item Type \"{displayName}\"";
    }
}

[System.Serializable]
public class ItemType
{
    public int typeInd;
    public ItemData data
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !EditorApplication.isPlaying)
            {
                GameManager.Instance.UpdateElementData();
            }
#endif
            if (!GameManager.Instance.GameData.ItemData.InBounds(typeInd))
                Debug.Log("No item data loaded! | " + typeInd + "/" + GameManager.Instance.GameData.ItemData.Count);
            List<ItemData> items = GameManager.Instance.GameData.ItemData;
            return items.InBounds(typeInd) ? items[typeInd] : null;
        }
    }
    public string displayName { get { return data.displayName; } }

    public ItemType(int typeInd = -1)
    {
        this.typeInd = typeInd;
    }

    public override string ToString()
    {
        return $"Type Index: {typeInd} | Type Name: {displayName}";
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ItemType))]
public class ItemTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<ItemData> data = GameManager.Instance.GameData.ItemData;
        string[] options = new string[data.Count];
        int[] optInds = new int[data.Count].IncrementalPopulate();
        int selected = property.FindPropertyRelative("typeInd").intValue;

        EditorGUI.BeginProperty(position, label, property);
        {
            Rect elementRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (options.Length > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    options[i] = data[i].displayName;
                }
                selected = EditorGUI.IntPopup(elementRect, selected, options, optInds);
                property.FindPropertyRelative("typeInd").intValue = selected;
            }
            else
            {
                EditorGUI.LabelField(elementRect, "No Item types available!");
                property.FindPropertyRelative("typeInd").intValue = -1;
            }
        }
        EditorGUI.EndProperty();
    }
}
#endif

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

[System.Serializable]
public struct LootTableList
{
    public List<LootTable> Tables;
    public LootTableList(List<LootTable> tables = null)
    {
        Tables = tables == null ? new List<LootTable>() : tables;
    }
}

[System.Serializable]
public class LootTable
{
    public bool fillPercentage = true;
    public List<LootTableItem> items;

    public void Recalculate()
    {
        if (items.Count > 0)
        {
            float total = 0.0f;
            foreach (LootTableItem item in items)
            {
                total += item.rollChance;
            }
            if (total > 1.0f)
            {
                foreach (LootTableItem item in items)
                {
                    item.rollChance /= total;
                }
            }
        }
    }

    public LootTableItem GetItem()
    {
        float ranPercent = Random.Range(0.0f, 1.0f);
        return GetItem(ranPercent);
    }

    public LootTableItem GetItem(float ranPercent)
    {
        float threshold = 0.0f;
        for (int i = 0; i < items.Count; i++)
        {
            threshold += items[i].rollChance;
            if (ranPercent <= threshold)
                return items[i];
        }
        return null;
    }
}

[System.Serializable]
public class LootTableItem
{
    public ItemType item = null;
    public ushort count = 1;
    public float rollChance = 0.0f;
}
