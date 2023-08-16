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
using System;

public struct DamageType
{
    public readonly uint ID;
    public string displayName;
    public string iconPath;

    private DamageType(uint ID, string displayName, string iconPath)
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

    public bool Matches(int ID)
    {
        return ID == this.ID || ID == int.MaxValue;
    }
    
    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public enum Type { None, Fire, Ice, Earth, Lightning, Physical, Psychic, Light, Dark }

    public static DamageType[] Defaults = new DamageType[]
    {
        /*00*/ new DamageType(0, "None", null),
        /*01*/ new DamageType(1, "Fire", null),
        /*02*/ new DamageType(2, "Ice", null),
        /*03*/ new DamageType(3, "Earth", null),
        /*04*/ new DamageType(4, "Lightning", null),
        /*05*/ new DamageType(5, "Physical", null),
        /*06*/ new DamageType(6, "Psychic", null),
        /*07*/ new DamageType(7, "Light", null),
        /*08*/ new DamageType(8, "Dark", null),
    };

    public static DamageType Get(Type type)
    {
        return Defaults.InBounds((int)type) ? Defaults[(int)type] : Defaults[0];
    }
    
    public static DamageType None { get { return Defaults[0]; } }
    public static DamageType Fire { get { return Defaults[1]; } }
    public static DamageType Ice { get { return Defaults[2]; } }
    public static DamageType Earth { get { return Defaults[3]; } }
    public static DamageType Lightning { get { return Defaults[4]; } }
    public static DamageType Physical { get { return Defaults[5]; } }
    public static DamageType Psychic { get { return Defaults[6]; } }
    public static DamageType Light { get { return Defaults[7]; } }
    public static DamageType Dark { get { return Defaults[8]; } }

    public static DamageType Modifier_Any { get { return new DamageType(int.MaxValue, "Universal", null); } }
}
