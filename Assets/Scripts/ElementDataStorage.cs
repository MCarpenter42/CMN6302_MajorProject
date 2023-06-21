using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

/*[CustomPropertyDrawer(typeof(ElementDataStorage))]
public class UpdateElementDataStorageDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
        Rect rect = new Rect(position);
        if (EditorGUI.PropertyField(position, property, label))
        {
            SerializedProperty prop = property.Copy();
            while (prop.NextVisible(true))
            {
                position.y += position.height;
                EditorGUI.PropertyField(position, prop);
            }
        }
        rect.x += rect.width = 60;
        rect.width = 60;
        if (GUI.Button(rect, "Refresh"))
        {

        }
    }
}*/

[System.Serializable]
public class ElementDataStorage
{
    [HideInInspector] public bool loaded = false;

    public ElementList<EnemyData> enemyList = new ElementList<EnemyData>(null, "Enemies:");
    public List<EnemyData> Enemies { get { return enemyList.Data; } set { enemyList = new ElementList<EnemyData>(value, "Enemies:"); } }

    public ElementList<ItemData> itemList = new ElementList<ItemData>(null, "Items:");
    public List<ItemData> Items { get { return itemList.Data; } set { itemList = new ElementList<ItemData>(value, "Items:"); } }

    private static ElementDataStorage ElementData { get { return GameManager.Instance.ElementDataStorage; } }

    public void Overwrite(ElementDataStorage newData)
    {
        enemyList = newData.enemyList;
        itemList = newData.itemList;
    }

    public void LoadData()
    {
        ElementDataStorage data;
        if (GameManager.applicationPlaying)
        {
            string jsonString = GameManager.Instance.dataJSON.text;
            data = jsonString.Length > 2 ? JsonUtility.FromJson<ElementDataStorage>(jsonString) : new ElementDataStorage();
        }
        else
        {
            data = LoadCache();
        }
        Overwrite(data);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private static string cachePath { get { return Application.dataPath + "/Resources/Data/_ElementData.json"; } }

    public static void SaveCache()
    {
        if (!GameManager.applicationPlaying)
        {
            string jsonString = JsonUtility.ToJson(ElementData);
            File.WriteAllText(cachePath, jsonString);
        }
    }

    public static void SaveCache(List<EnemyData> list, bool overwriteIfEmpty = false)
    {
        if (!GameManager.applicationPlaying)
        {
            if (overwriteIfEmpty || (!overwriteIfEmpty && list.Count > 0))
            {
                ElementData.Enemies = list;
                SaveCache();
            }
        }
    }
    
    public static void SaveCache(List<ItemData> list, bool overwriteIfEmpty = false)
    {
        if (!GameManager.applicationPlaying)
        {
            if (overwriteIfEmpty || (!overwriteIfEmpty && list.Count > 0))
            {
                ElementData.Items = list;
                SaveCache();
            }
        }
    }

    public static ElementDataStorage LoadCache()
    {
        ElementDataStorage data;
        if (File.Exists(cachePath))
        {
            string jsonString = File.ReadAllText(cachePath);
            data = jsonString.Length > 2 ? JsonUtility.FromJson<ElementDataStorage>(jsonString) : ElementData;
        }
        else
        {
            data = new ElementDataStorage();
            File.Create(cachePath);
            string jsonString = JsonUtility.ToJson(data);
            File.WriteAllText(cachePath, jsonString);
        }
        ElementData.Overwrite(data);
        return data;
    }

    public static List<T> LoadCache<T>()
    {
        ElementDataStorage data = LoadCache();
        if (typeof(T) == typeof(EnemyData))
        {
            return data.Enemies as List<T>;
        }
        else if (typeof(T) == typeof(ItemData))
        {
            return data.Items as List<T>;
        }
        else
        {
            return new List<T>();
        }
    }

    // private static string cachePath_Enemies { get { return Application.dataPath + "/Editor/Data/CACHE_EnemyData.json"; } }

    public static void SaveEnemyCache(List<EnemyData> list, bool overwriteIfEmpty = false)
    {
        /*string jsonString = JsonUtility.ToJson(new EnemyList(list));
        File.WriteAllText(cachePath_Enemies, jsonString);*/
        if (list.Count > 0 || overwriteIfEmpty)
        {
            ElementData.Enemies = list;
            SaveCache();
        }
    }

    public static List<EnemyData> LoadEnemyCache()
    {
        /*List<EnemyData> list = null;
        if (File.Exists(cachePath_Enemies))
        {
            string jsonString = File.ReadAllText(cachePath_Enemies);
            list = jsonString.Length > 2 ? JsonUtility.FromJson<EnemyList>(jsonString).Enemies : new List<EnemyData>();
        }
        else
        {
            list = new List<EnemyData>();
            File.Create(cachePath_Enemies);
            string jsonString = JsonUtility.ToJson(new EnemyList(list));
            File.WriteAllText(cachePath_Enemies, jsonString);
        }
        return list;*/
        LoadCache();
        return ElementData.Enemies;
    }
}

public static class ElementDataExtensions
{
    public static EnemyData[] AsData(this EnemyType[] enemies)
    {
        EnemyData[] output = new EnemyData[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            output[i] = enemies[i].data;
        }
        return output;
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
public class EnemyData : ElementData
{
    public EntityModel model;
    // Behaviour
    // Health
    // Damage reduction
    // Speed

    public EnemyData(string displayName)
    {
        this.displayName = displayName;
    }

    public override string ToString()
    {
        return $"Enemy Type \"{displayName}\"";
    }
}

[System.Serializable]
public class EnemyType
{
    public int typeInd;
    public EnemyData data
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !EditorApplication.isPlaying)
            {
                GameManager.Instance.UpdateElementData();
            }
#endif
            if (!GameManager.Instance.ElementDataStorage.Enemies.InBounds(typeInd))
                Debug.Log("No enemy data loaded! | " + typeInd + "/" + GameManager.Instance.ElementDataStorage.Enemies.Count);
            List<EnemyData> enemies = GameManager.Instance.ElementDataStorage.Enemies;
            return enemies.InBounds(typeInd) ? enemies[typeInd] : null;
        }
    }
    public string displayName { get { return data.displayName; } }

    public EnemyType(int typeInd = -1)
    {
        this.typeInd = typeInd;
    }

    public override string ToString()
    {
        return $"Type Index: {typeInd} | Type Name: {displayName}";
    }
}

[CustomPropertyDrawer(typeof(EnemyType))]
public class EnemyTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<EnemyData> data = ElementDataStorage.LoadCache<EnemyData>();
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
            if (!GameManager.Instance.ElementDataStorage.Items.InBounds(typeInd))
                Debug.Log("No item data loaded! | " + typeInd + "/" + GameManager.Instance.ElementDataStorage.Items.Count);
            List<ItemData> items = GameManager.Instance.ElementDataStorage.Items;
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
        List<ItemData> data = GameManager.Instance.ElementDataStorage.Items;
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
public struct DropTableList
{
    public List<DropTable> Tables;
    public DropTableList(List<DropTable> tables = null)
    {
        Tables = tables == null ? new List<DropTable>() : tables;
    }
}

[System.Serializable]
public class DropTable
{
    public bool fillPercentage = true;
    public List<DropTableItem> items;

    public void Recalculate()
    {
        if (items.Count > 0)
        {
            float total = 0.0f;
            foreach (DropTableItem item in items)
            {
                total += item.rollChance;
            }
            if (total > 1.0f)
            {
                foreach (DropTableItem item in items)
                {
                    item.rollChance /= total;
                }
            }
        }
    }

    public DropTableItem GetItem()
    {
        float ranPercent = Random.Range(0.0f, 1.0f);
        return GetItem(ranPercent);
    }

    public DropTableItem GetItem(float ranPercent)
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
public class DropTableItem
{
    public ItemType item = null;
    public ushort count = 1;
    public float rollChance = 0.0f;
}
