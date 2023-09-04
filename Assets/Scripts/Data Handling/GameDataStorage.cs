using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;

public class GameDataStorage
{
    public static GameDataStorage Data { get { return GameManager.Instance.GameDataStorage; } }

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

    public void LoadData()
    {
        List<EnemyData> enemyData = new List<EnemyData>();
        List<ItemData> itemData = new List<ItemData>();
        if (GameManager.applicationPlaying)
        {
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
        else
        {
            enemyData = LoadEnemyCache();
        }
    }

    private static string cacheFolder { get { return Application.dataPath + "/Resources/Data/"; } }

    private static string cacheFile_Enemies = "_EnemyData.json";
    private static string cachePath_Enemies { get { return cacheFolder + cacheFile_Enemies; } }

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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public PlayerData[] playerData = null;
    public List<ItemCore> inventory = new List<ItemCore>();

    public void SavePlayData()
    {

    }

    public void LoadPlayData()
    {

    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

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
             baseAttack = 054,  attackScaling = 040,
            baseDefence = 015, defenceScaling = 020,
            speeds = new SpeedAtLevel[] { new SpeedAtLevel(0, 130) },
            attackType = DamageType.Type.None,
            actionSet = ActionSetName.PLAYER_A,
            currentHealthPercent = 100f,
        };
        playerData[1] = new PlayerData()
        {
            displayName = "Player Character 2",
            modelHexUID = "E0E81A8C",
            friendly = true, playerControlled = true,
             baseHealth = 280,  healthScaling = 060,
             baseAttack = 048,  attackScaling = 040,
            baseDefence = 015, defenceScaling = 020,
            speeds = new SpeedAtLevel[] { new SpeedAtLevel(0, 118) },
            attackType = DamageType.Type.None,
            actionSet = ActionSetName.PLAYER_B,
            currentHealthPercent = 100f,
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
            currentHealthPercent = 100f,
        };
        playerData[3] = new PlayerData()
        {
            displayName = "Player Character 4",
            modelHexUID = "ECBADC05",
            friendly = true, playerControlled = true,
             baseHealth = 250,  healthScaling = 060,
             baseAttack = 030,  attackScaling = 040,
            baseDefence = 015, defenceScaling = 020,
            speeds = new SpeedAtLevel[] { new SpeedAtLevel(0, 098) },
            attackType = DamageType.Type.None,
            actionSet = ActionSetName.PLAYER_D,
            currentHealthPercent = 100f,
        };
    }
}

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
    public float currentHealthPercent = 100f;
    public Equipment equipment;
}

[System.Serializable]
public class EnemyData : CombatantData
{
    public EnemyClass Class;
    public bool wanderInWorld = true;

    public static EnemyData[] DataFromPool(EnemyClass[] classes)
    {
        EnemyData[] dataOut = new EnemyData[classes.Length];
        List<EnemyData>[] categorised = GameManager.Instance.GameDataStorage.EnemyData.Categorise();
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
            if (!Application.isPlaying && !EditorApplication.isPlaying)
            {
                GameManager.Instance.UpdateElementData();
            }
#endif
            if (!GameManager.Instance.GameDataStorage.EnemyData.InBounds(typeInd))
                Debug.Log("No enemy data loaded! | " + typeInd + "/" + GameManager.Instance.GameDataStorage.EnemyData.Count);
            List<EnemyData> enemies = GameManager.Instance.GameDataStorage.EnemyData;
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
            if (!GameManager.Instance.GameDataStorage.ItemData.InBounds(typeInd))
                Debug.Log("No item data loaded! | " + typeInd + "/" + GameManager.Instance.GameDataStorage.ItemData.Count);
            List<ItemData> items = GameManager.Instance.GameDataStorage.ItemData;
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

[CustomPropertyDrawer(typeof(ItemType))]
public class ItemTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<ItemData> data = GameManager.Instance.GameDataStorage.ItemData;
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
