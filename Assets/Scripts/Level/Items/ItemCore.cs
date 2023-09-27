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
using NeoCambion.IO;
using NeoCambion.IO.Unity;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Random.Unity;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;

[System.Serializable]
public enum ItemRarity { Common, Rare, Epic, Legendary }

public static class ItemUtility
{

}

[System.Serializable]
public class ItemCore
{
    public string displayName = "";
}

[System.Serializable]
public class ItemEquippable : ItemCore
{
    public int level;
}

[System.Serializable]
public class ItemEquipElement : ItemEquippable
{
    public DamageType.Type damageType;
}

[System.Serializable]
public class ItemEquipStats : ItemEquippable
{
    public CombatantAttribute attribute;
    public float statModifier;

    public float actionBasicModifier;
    public float actionSkillModifier;
    public float actionUltModifier;
}

[System.Serializable]
public class ItemConsumable : ItemCore
{

}
