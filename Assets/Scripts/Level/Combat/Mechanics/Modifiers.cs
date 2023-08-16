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
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;
using NeoCambion.Unity.Interpolation;

public struct DamageDealtModifier
{
    public string modifierName;
    public int typeID;
    public float mod;

    public DamageDealtModifier(string modifierName, int typeID, float mod)
    {
        this.modifierName = modifierName;
        this.typeID = typeID;
        this.mod = mod;
    }

    public DamageDealtModifier(string modifierName, int typeID, int percentMod)
    {
        this.modifierName = modifierName;
        this.typeID = typeID;
        this.mod = percentMod / 100.0f;
    }
}

public struct DamageTakenModifier
{
    public enum ModType { Weak, Resist, Immune }

    public string modifierName;
    public int typeID;
    public ModType modType;
    public float mod { get { return modType == ModType.Weak ? 2.0f : (modType == ModType.Resist ? 0.5f : 0.0f); } }

    public DamageTakenModifier(string modifierName, int typeID, ModType modType)
    {
        this.modifierName = modifierName;
        this.typeID = typeID;
        this.modType = modType;
    }
}

public struct HealingModifier
{
    public string modifierName;
    public float mod;

    public HealingModifier(string modifierName, float mod)
    {
        this.modifierName = modifierName;
        this.mod = mod;
    }

    public HealingModifier(string modifierName, int percentMod)
    {
        this.modifierName = modifierName;
        this.mod = percentMod / 100.0f;
    }
}

public struct ShieldModifier
{
    public string modifierName;
    public float mod;

    public ShieldModifier(string modifierName, float mod)
    {
        this.modifierName = modifierName;
        this.mod = mod;
    }

    public ShieldModifier(string modifierName, int percentMod)
    {
        this.modifierName = modifierName;
        this.mod = percentMod / 100.0f;
    }
}

[System.Serializable]
public struct StatusModifier
{
    public enum ModType { Weak, Resist, Immune }

    public string modifierName;
    public string internalName;
    public ModType modType;
    public float modifier { get { return modType == ModType.Weak ? 2.0f : (modType == ModType.Resist ? 0.5f : 0.0f); } }

    public StatusModifier(string modifierName, string internalName, ModType modType)
    {
        this.modifierName = modifierName;
        this.internalName = internalName;
        this.modType = modType;
    }
}
