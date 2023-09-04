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

[System.Serializable]
public class RandTuning : Core
{
    public enum Var2Bit { Min, Low, High, Max }

    #region [ LEVEL GENERATION ] - 15

    private bool varMap_iterations = false;
    private Var2Bit varMap_roomStructure = Var2Bit.Min;
    private Var2Bit varMap_corridorStructure = Var2Bit.Min;
    private bool varMap_connectToExisting = true;

    private static RoomStructureVariance[] optMap_roomStructure = new RoomStructureVariance[]
    {
        new RoomStructureVariance(3, 3, 6, 6),
        new RoomStructureVariance(2, 3, 6, 7),
        new RoomStructureVariance(2, 4, 5, 7),
        new RoomStructureVariance(1, 4, 5, 8)
    };
    private static CorridorStructureVariance[] optMap_corridorStructure = new CorridorStructureVariance[]
    {
        new CorridorStructureVariance(4, 6, false),
        new CorridorStructureVariance(4, 6, true),
        new CorridorStructureVariance(3, 8, false),
        new CorridorStructureVariance(3, 8, true)
    };

    public int valMap_iterations { get { return varMap_iterations ? 3 : 2; } }
    public RoomStructureVariance valMap_roomStructure { get { return optMap_roomStructure[(int)varMap_roomStructure]; } }
    public CorridorStructureVariance valMap_corridorStructure { get { return optMap_corridorStructure[(int)varMap_corridorStructure]; } }
    public bool valMap_connectToExisting { get { return varMap_connectToExisting; } }

    private Var2Bit varLvl_enemyDensity = Var2Bit.Min;
    private Var2Bit varLvl_itemDensity = Var2Bit.Min;

    private static FloatRange[] optLvl_enemyDensity = new FloatRange[]
    {
        new FloatRange(0.76f, 0.76f),
        new FloatRange(0.68f, 0.84f),
        new FloatRange(0.60f, 0.92f),
        new FloatRange(0.52f, 1.00f)
    };
    private static FloatRange[] optLvl_itemDensity = new FloatRange[]
    {
        new FloatRange(0.55f, 0.55f),
        new FloatRange(0.50f, 0.60f),
        new FloatRange(0.45f, 0.65f),
        new FloatRange(0.40f, 0.70f),
    };
    
    public FloatRange valLvl_enemyDensity { get { return optLvl_enemyDensity[(int)varLvl_enemyDensity]; } }
    public FloatRange valLvl_itemDensity { get { return optLvl_itemDensity[(int)varLvl_itemDensity]; } }

    public GenerationSettings GenSettings
    {
        get
        {
            return new GenerationSettings()
            {
                iterations = valMap_iterations,
                roomStructure = valMap_roomStructure,
                corridorStructure = valMap_corridorStructure,
                connectToExisting = valMap_connectToExisting,
                enemyDensity = valLvl_enemyDensity,
                itemDensity = valLvl_itemDensity,
            };
        }
    }

    #endregion

    #region [ AI BEHAVIOUR ] - 5

    private Var2Bit varBhv_selection = Var2Bit.Min;
    private Var2Bit varBhv_targeting = Var2Bit.Min;
    
    public int valBhv_selection { get { return (int)varBhv_selection; } }
    public int valBhv_targeting { get { return (int)varBhv_targeting; } }

    #endregion

    #region [ STAT VARIANCE ] - 6

    private Var2Bit varSts_health = Var2Bit.Min;
    private Var2Bit varSts_defence = Var2Bit.Min;
    private bool varSts_speed = false;
    //private bool varSts_types = false;

    private static float[] optSts_floats = new float[] { 0.00f, 0.00f, 0.00f, 0.10f };

    public float valSts_health { get { return optSts_floats[(int)varSts_health]; } }
    public float valSts_defence { get { return optSts_floats[(int)varSts_defence]; } }
    public bool valSts_speed { get { return varSts_speed; } }
    //public bool valSts_types { get { return varSts_types; } }

    #endregion

    #region [ DAMAGE VARIANCE ] - 6

    private Var2Bit varDmg_base = Var2Bit.Min;
    private Var2Bit varDmg_critRate = Var2Bit.Min;
    private Var2Bit varDmg_critScale = Var2Bit.Min;

    private static float[] optDmg_base = new float[] { 0.00f, 0.05f, 0.10f, 0.15f };
    private static float[] optDmg_critRate = new float[] { 0.00f, 0.02f, 0.05f, 0.10f };
    private static float[] optDmg_critScale = new float[] { 1.20f, 1.50f, 1.70f, 2.00f };

    public float valDmg_base { get { return optDmg_base[(int)varDmg_base]; } }
    public float valDmg_critRate { get { return optDmg_critRate[(int)varDmg_critRate]; } }
    public float valDmg_critScale { get { return optDmg_critScale[(int)varDmg_critScale]; } }

    #endregion

    // Total bits used by all settings = 32

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool randOnAwake = false;

    void Awake()
    {
        SetRef_RandTuning(this);
        if (randOnAwake)
        {
            NewRandomness();
            string str = "More map iterations: " + (varMap_iterations ? "Enabled (" : "Disabled (") + varMap_iterations + ")";
            Debug.Log(str);
        }
    }

    public void NewRandomness()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        byte bRand = (byte)Random.Range(0, 1 << 5);
        int bBit = 7;

        varMap_iterations = bRand.Bit(bBit--);
        varMap_roomStructure = (Var2Bit)Random.Range(0, 4);
        varMap_connectToExisting = bRand.Bit(bBit--);
        varMap_corridorStructure = (Var2Bit)Random.Range(0, 4);

        varLvl_enemyDensity = (Var2Bit)Random.Range(0, 4);
        varLvl_itemDensity = (Var2Bit)Random.Range(0, 4);

        varBhv_selection = (Var2Bit)Random.Range(0, 4);
        varBhv_targeting = (Var2Bit)Random.Range(0, 4);

        varSts_health = (Var2Bit)Random.Range(0, 4);
        varSts_defence = (Var2Bit)Random.Range(0, 4);
        varSts_speed = bRand.Bit(bBit--);
        //varSts_types = bRand.Bit(bBit--);

        varDmg_base = (Var2Bit)Random.Range(0, 4);
        varDmg_critRate = (Var2Bit)Random.Range(0, 4);
        varDmg_critScale = (Var2Bit)Random.Range(0, 4);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // 1 character (base 64) --> 6 bits
    //    --> Use a multiple of 6 total bit count
    //       --> Any bits not used for control can be used for scrambling

    private int lShift;

    private ulong Shift(bool var)
    {
        lShift -= 1;
        return (var ? 1ul : 0ul) << lShift;
    }

    private ulong Shift(Var2Bit var)
    {
        lShift -= 2;
        return (ulong)var << lShift;
    }

    private ulong BitShiftLoop(ulong val, int bitStart, int bitEnd, int shift)
    {
        int range = bitEnd - bitStart + 1;

        if (bitStart < 0)
            bitStart = 0;
        else if (bitStart > 62)
            bitStart = 62;
        if (bitEnd > 63)
            bitEnd = 63;
        else if (bitEnd <= bitStart)
            bitEnd = bitStart + 1;
        shift = shift.WrapClamp(1, range - 1);

        ulong a = (1ul << shift) - 1;
        ulong a2 = a << (64 - bitStart - shift);
        ulong a3 = a << (63 - bitEnd);

        ulong b = (1ul << (range - shift)) - 1;
        ulong b2 = b << (63 - bitEnd);
        ulong b3 = b << (64 - bitStart - range - shift);

        ulong c = ((1ul << range) - 1) << (64 - bitStart - range);

        ulong d = ((val & a2) >> (shift - range)) & a3;
        ulong e = ((val & b2) << shift) & b3;

        return (val ^ c) | d | e;
    }

    private char BitsToBase64(ulong val, int bitStart)
    {
        if (bitStart > 58)
            bitStart = 58;
        ulong ind = ((ulong)val >> (58 - bitStart)) & 31ul;
        return Ext_Char.Base64[(int)ind];
    }

    private string TuningSettingsString()
    {
        int bitCount = 30;
        lShift = bitCount;

        ulong bits = Shift(varMap_iterations); // -1 --> 29
        bits |= Shift(varMap_roomStructure); // -2 --> 27
        bits |= Shift(varMap_corridorStructure); // -2 --> 25
        bits |= Shift(varMap_connectToExisting); // -1 --> 24

        bits |= Shift(varLvl_enemyDensity); // -2 --> 22
        bits |= Shift(varLvl_itemDensity); // -2 --> 20

        bits |= Shift(varBhv_selection); // -2 --> 18
        bits |= Shift(varBhv_targeting); // -2 --> 16

        bits |= Shift(varSts_health); // -2 --> 14
        bits |= Shift(varSts_defence); // -2 --> 12
        bits |= Shift(varSts_speed); // -1 --> 11
        //bits |= Shift(varSts_types); // -1 --> 10

        bits |= Shift(varDmg_base); // -2 --> 8
        bits |= Shift(varDmg_critRate); // -2 --> 6
        bits |= Shift(varDmg_critScale); // -2 --> 4

        int startBit = 64 - bitCount, r = Random.Range(0, 1 << lShift);
        bits = BitShiftLoop(bits, 64 - bitCount, 63 - lShift, r);
        bits |= (uint)r;

        List<char> chars = new List<char>();
        for (; startBit < 59; startBit += 6)
        {
            chars.Add(BitsToBase64(bits, startBit));
        }
        return new string(chars.ToArray());
    }
    public string SettingsString { get { return TuningSettingsString(); } }
}

public struct RoomStructureVariance
{
    public ushort standardMin;
    public ushort standardMax;
    public ushort largeMin;
    public ushort largeMax;

    public int Min(bool large) { return large ? largeMin : standardMin; }
    public int Max(bool large) { return large ? largeMax : standardMax; }

    public RoomStructureVariance(ushort standardMin, ushort standardMax, ushort largeMin, ushort largeMax)
    {
        if (standardMin > standardMax)
        {
            this.standardMin = standardMax;
            this.standardMax = standardMin;
        }
        else
        {
            this.standardMin = standardMin;
            this.standardMax = standardMax;
        }

        if (largeMin > largeMax)
        {
            this.largeMin = largeMax;
            this.largeMax = largeMin;
        }
        else
        {
            this.largeMin = largeMin;
            this.largeMax = largeMax;
        }

        Check();
    }

    private void Check()
    {
        if (largeMin < standardMin)
            (largeMin, standardMin) = (standardMin, largeMin);

        if (largeMax < standardMax)
            (largeMax, standardMax) = (standardMax, largeMax);
    }
}

public struct CorridorStructureVariance
{
    public uint lengthMin;
    public uint lengthMax;
    public bool allowSplit;

    public CorridorStructureVariance(uint lengthMin, uint lengthMax, bool allowSplit)
    {
        if (lengthMin > lengthMax)
        {
            this.lengthMin = lengthMax;
            this.lengthMax = lengthMin;
        }
        else
        {
            this.lengthMin = lengthMin;
            this.lengthMax = lengthMax;
        }
        this.allowSplit = allowSplit;
    }
}

public struct GenerationSettings
{
    public int iterations;

    public RoomStructureVariance roomStructure;
    public int RoomMin(bool largeRoom) => roomStructure.Min(largeRoom);
    public int RoomMax(bool largeRoom) => roomStructure.Max(largeRoom);

    public CorridorStructureVariance corridorStructure;
    public int corridorMin => (int)corridorStructure.lengthMin;
    public int corridorMax => (int)corridorStructure.lengthMax;
    public bool corridorSplit => corridorStructure.allowSplit;

    public bool connectToExisting;

    public FloatRange enemyDensity;
    public float enemyMin => enemyDensity.lower;
    public float enemyMax => enemyDensity.upper;

    public FloatRange itemDensity;
    public float itemMin => itemDensity.lower;
    public float itemMax => itemDensity.upper;
}

[CustomEditor(typeof(RandTuning))]
[CanEditMultipleObjects]
public class RandTuningEditor : Editor
{
    RandTuning targ { get { return target as RandTuning; } }
    SerializedProperty randOnAwake;

    public override void OnInspectorGUI()
    {
        EditorElements.RequiredComponent("Necessary for pre-run randomness tuning");
        EditorGUILayout.Space(4);
        EditorElements.SeparatorBar();
        EditorGUILayout.Space(4);
        randOnAwake = serializedObject.FindProperty("randOnAwake");
        randOnAwake.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Randomise vals when \"Awake()\" is executed"), randOnAwake.boolValue);
        if (serializedObject.hasModifiedProperties)
            serializedObject.ApplyModifiedProperties();
    }
}
