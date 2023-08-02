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
    public string displayName = "DamageType_DefaultName";
    public string iconPath = null;

    public StatusEffect defaultEffect;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    enum Type { None, Fire, Ice, Earth, Lightning, Physical, Psychic, Light, Dark }
    public static DamageType Fire = new DamageType()
    {
        displayName = "Fire",
        iconPath = null
    };
    public static DamageType Ice = new DamageType()
    {
        displayName = "Ice",
        iconPath = null
    };
    public static DamageType Earth = new DamageType()
    {
        displayName = "Earth",
        iconPath = null
    };
    public static DamageType Lightning = new DamageType()
    {
        displayName = "Lightning",
        iconPath = null
    };
    public static DamageType Physical = new DamageType()
    {
        displayName = "Physical",
        iconPath = null
    };
    public static DamageType Psychic = new DamageType()
    {
        displayName = "Psychic",
        iconPath = null
    };
    public static DamageType Light = new DamageType()
    {
        displayName = "Light",
        iconPath = null
    };
    public static DamageType Dark = new DamageType()
    {
        displayName = "Dark",
        iconPath = null
    };
}
