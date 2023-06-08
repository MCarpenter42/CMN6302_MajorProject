using System.Collections;
using System.Collections.Generic;
using System.IO;
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

[CustomEditor(typeof(ElementDataStorage))]
public class ElementDataStorageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle centreLabel = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.Space(2.0f);
                EditorGUILayout.LabelField("This component stores data for item handling, and is required for game functionality.", centreLabel);
                EditorGUILayout.Space(2.0f);
                EditorGUILayout.LabelField("Enemies:", GameManager.Instance.ElementDataStorage.Enemies.Count.ToString());
                EditorGUILayout.LabelField("Items:", GameManager.Instance.ElementDataStorage.Items.Count.ToString());
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
}

[System.Serializable]
public class ElementDataStorage : Core
{
    public EnemyList enemyList;
    public List<EnemyData> Enemies { get { return enemyList.Enemies; } set { enemyList = new EnemyList(value); } }
    public ItemList itemList;
    public List<ItemData> Items { get { return itemList.Items; } set { itemList = new ItemList(value); } }

    private static string cachePath_Enemies { get { return Application.dataPath + "/Editor/Data/CACHE_EnemyData.json"; } }

    public static void SaveEnemyCache(List<EnemyData> list)
    {
        string jsonString = JsonUtility.ToJson(new EnemyList(list));
        File.WriteAllText(cachePath_Enemies, jsonString);
    }

    public static List<EnemyData> LoadEnemyCache()
    {
        List<EnemyData> list = null;
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
        return list;
    }
}

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

[System.Serializable]
public struct EnemyList
{
    public List<EnemyData> Enemies;
    public EnemyList(List<EnemyData> enemies)
    {
        Enemies = enemies;
    }
}

[System.Serializable]
public class EnemyData
{
    public string displayName;
    public EntityModel model;
    // Behaviour
    // Health
    // Damage reduction
    // Speed

    public EnemyData(string displayName)
    {
        this.displayName = displayName;
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
            GameManager.Instance.UpdateElementData();
            return typeInd > -1 ? GameManager.Instance.ElementDataStorage.Enemies[typeInd] : null;
        }
    }
    public string displayName { get { return data.displayName; } }

    public EnemyType(int typeInd = -1)
    {
        this.typeInd = typeInd;
    }
}

[CustomPropertyDrawer(typeof(EnemyType))]
public class EnemyTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<EnemyData> data = GameManager.Instance.ElementDataStorage.Enemies;
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
public struct ItemList
{
    public List<ItemData> Items;
    public ItemList(List<ItemData> enemies)
    {
        Items = enemies;
    }
}

[System.Serializable]
public class ItemData
{
    public string displayName;

    public ItemData(string displayName)
    {
        this.displayName = displayName;
    }
}

[System.Serializable]
public class ItemType
{
    public int typeInd;
    public ItemData data { get { return typeInd > -1 ? GameManager.Instance.ElementDataStorage.Items[typeInd] : null; } }
    public string displayName { get { return data.displayName; } }

    public ItemType(int typeInd = -1)
    {
        this.typeInd = typeInd;
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
