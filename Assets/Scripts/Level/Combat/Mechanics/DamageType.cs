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

public class DamageType
{
    public readonly uint ID = 0;
    public string displayName = "UNASSIGNED";
    public string iconPath = null;

    public StatusEffect defaultEffect;

    protected DamageType(uint ID, string displayName, string iconPath)
    {
        this.ID = ID;
        this.displayName = displayName;
        this.iconPath = iconPath;
    }

    public DamageType(string displayName = "UNASSIGNED", string iconPath = null)
    {
        ID = 0;
        this.displayName = displayName;
        this.iconPath = iconPath;
    }
    
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    enum Type { None, Fire, Ice, Earth, Lightning, Physical, Psychic, Light, Dark }
    public static DamageType Fire = new DamageType(1, "Fire", null);
    public static DamageType Ice = new DamageType(2, "Ice", null);
    public static DamageType Earth = new DamageType(3, "Earth", null);
    public static DamageType Lightning = new DamageType(4, "Lightning", null);
    public static DamageType Physical = new DamageType(5, "Physical", null);
    public static DamageType Psychic = new DamageType(6, "Psychic", null);
    public static DamageType Light = new DamageType(7, "Light", null);
    public static DamageType Dark = new DamageType(8, "Dark", null);
}
