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
using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System;

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
    // Model
    // Behaviour
    // Health
    // Damage reduction
    // Speed

    public EnemyData(string displayName)
    {
        this.displayName = displayName;
    }
}
/*
public class EnemyTypePopupAttribute : PropertyAttribute
{
    public EnemyTypePopupAttribute()
    {

    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnemyTypePopupAttribute))]
public class EnemyTypePopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnemyTypePopupAttribute attr = attribute as EnemyTypePopupAttribute;
        List<EnemyData> data = GameManager.Instance.ElementDataStorage.Enemies;
        string[] options = new string[data.Count];
        int[] optInds = new int[data.Count].IncrementalPopulate();
        int selected = property.intValue;
        if (options.Length > 0)
        {
            for (int i = 0; i < data.Count; i++)
            {
                options[i] = data[i].displayName;
            }
            Rect elementRect = EditorGUI.PrefixLabel(position, label);
            selected = EditorGUI.IntPopup(elementRect, selected, options, optInds);
            property.intValue = selected;
        }
        else
        {
            Rect elementRect = EditorGUI.PrefixLabel(position, label);
            EditorGUI.LabelField(elementRect, "No enemy types available!");
            property.intValue = -1;
        }
    }
}
#endif
*/
[System.Serializable]
public class EnemyType
{
    public int typeInd;
    public EnemyData data { get { return typeInd > -1 ? GameManager.Instance.ElementDataStorage.Enemies[typeInd] : null; } }
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

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

public class ActionData
{

}
