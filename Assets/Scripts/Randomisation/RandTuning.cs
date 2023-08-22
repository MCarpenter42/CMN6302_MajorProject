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

public class RandTuning : Core
{
    private enum Var2Bit { Off, Low, High, Max }

    #region [ LEVEL GENERATION ] - 15

    private Var2Bit variance_map_roomCount = Var2Bit.Off;
    private Var2Bit variance_map_roomSize = Var2Bit.Off;
    private bool variance_map_roomConnections = false;
    private Var2Bit variance_map_corridorLength_min = Var2Bit.Off;
    private Var2Bit variance_map_corridorLength_max = Var2Bit.Off;

    private int[] options_map_roomCount = new int[] { 0, 0, 0, 0 };
    private int[] options_map_roomSize = new int[] { 0, 0, 0, 0 };
    private int[] options_map_corridorLength_min = new int[] { 0, 0, 0, 0 };
    private int[] options_map_corridorLength_max = new int[] { 0, 0, 0, 0 };

    public int value_map_roomCount { get { return options_map_roomCount[(int)variance_map_roomCount]; } }
    public int value_map_roomSize { get { return options_map_roomSize[(int)variance_map_roomSize]; } }
    public bool value_map_roomConnections { get { return variance_map_roomConnections; } }
    public int value_map_corridorLength_min { get { return options_map_corridorLength_min[(int)variance_map_corridorLength_min]; } }
    public int value_map_corridorLength_max { get { return options_map_corridorLength_max[(int)variance_map_corridorLength_max]; } }

    private Var2Bit variance_level_enemyCount = Var2Bit.Off;
    private Var2Bit variance_level_chestCount = Var2Bit.Off;
    private Var2Bit variance_level_spawnSpread = Var2Bit.Off;

    private int[] options_level_enemyCount = new int[] { 0, 0, 0, 0 };
    private int[] options_level_chestCount = new int[] { 0, 0, 0, 0 };
    private int[] options_level_spawnSpread = new int[] { 0, 0, 0, 0 };
    
    public int value_level_enemyCount { get { return options_level_enemyCount[(int)variance_level_enemyCount]; } }
    public int value_level_chestCount { get { return options_level_chestCount[(int)variance_level_chestCount]; } }
    public int value_level_spawnSpread { get { return options_level_spawnSpread[(int)variance_level_spawnSpread]; } }

    #endregion

    #region [ AI BEHAVIOUR ] - 5

    private bool variance_behaviour_available = false;
    private Var2Bit variance_behaviour_selection = Var2Bit.Off;
    private Var2Bit variance_behaviour_targeting = Var2Bit.Off;
    
    public bool value_behaviour_available { get { return variance_behaviour_available; } }
    public int value_behaviour_selection { get { return (int)variance_behaviour_selection; } }
    public int value_behaviour_targeting { get { return (int)variance_behaviour_targeting; } }

    #endregion

    #region [ STAT VARIANCE ] - 6

    private Var2Bit variance_stats_health = Var2Bit.Off;
    private Var2Bit variance_stats_defence = Var2Bit.Off;
    private bool variance_stats_speed = false;
    private bool variance_stats_types = false;
    
    private float[] options_stats_floats = new float[] { 0.00f, 0.00f, 0.00f, 0.10f };

    public float value_stats_health { get { return options_stats_floats[(int)variance_stats_health]; } }
    public float value_stats_defence { get { return options_stats_floats[(int)variance_stats_defence]; } }
    public bool value_stats_speed { get { return variance_stats_speed; } }
    public bool value_stats_types { get { return variance_stats_types; } }

    #endregion

    #region [ DAMAGE VARIANCE ] - 6

    private Var2Bit variance_damage_base = Var2Bit.Off;
    private Var2Bit variance_damage_critRate = Var2Bit.Off;
    private Var2Bit variance_damage_critScale = Var2Bit.Off;
    
    private float[] options_damage_base = new float[] { 0.00f, 0.05f, 0.10f, 0.15f };
    private float[] options_damage_critRate = new float[] { 0.00f, 0.02f, 0.05f, 0.10f };
    private float[] options_damage_critScale = new float[] { 1.20f, 1.50f, 1.70f, 2.00f };

    public float value_damage_base { get { return options_damage_base[(int)variance_damage_base]; } }
    public float value_damage_critRate { get { return options_damage_critRate[(int)variance_damage_critRate]; } }
    public float value_damage_critScale { get { return options_damage_critScale[(int)variance_damage_critScale]; } }

    #endregion

    // Total bits used by all settings = 32

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void NewRandomness()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        byte bRand = (byte)Random.Range(0, 1 << 4);

        variance_map_roomCount = (Var2Bit)Random.Range(0, 4);
        variance_map_roomSize = (Var2Bit)Random.Range(0, 4);
        variance_map_roomConnections = bRand.Bit(7);
        variance_map_corridorLength_min = (Var2Bit)Random.Range(0, 4);
        variance_map_corridorLength_max = (Var2Bit)Random.Range(0, 4);

        variance_level_enemyCount = (Var2Bit)Random.Range(0, 4);
        variance_level_chestCount = (Var2Bit)Random.Range(0, 4);
        variance_level_spawnSpread = (Var2Bit)Random.Range(0, 4);

        variance_behaviour_available = bRand.Bit(6);
        variance_behaviour_selection = (Var2Bit)Random.Range(0, 4);
        variance_behaviour_targeting = (Var2Bit)Random.Range(0, 4);

        variance_stats_health = (Var2Bit)Random.Range(0, 4);
        variance_stats_defence = (Var2Bit)Random.Range(0, 4);
        variance_stats_speed = bRand.Bit(5);
        variance_stats_types = bRand.Bit(4);

        variance_damage_base = (Var2Bit)Random.Range(0, 4);
        variance_damage_critRate = (Var2Bit)Random.Range(0, 4);
        variance_damage_critScale = (Var2Bit)Random.Range(0, 4);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static char[] base64 = new char[]
    {
        '0', '1', '2', '3', '4', '5', '6', '7',
        '8', '9', 'a', 'b', 'c', 'd', 'e', 'f',
        'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
        'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
        'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D',
        'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L',
        'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
        'U', 'V', 'W', 'X', 'Y', 'Z', '_', '-',
    };

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

    private ulong BitShiftLoop(ulong value, int bitStart, int bitEnd, int shift)
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

        ulong d = ((value & a2) >> (shift - range)) & a3;
        ulong e = ((value & b2) << shift) & b3;

        return (value ^ c) | d | e;
    }

    private char BitsToBase64(ulong value, int bitStart)
    {
        if (bitStart > 58)
            bitStart = 58;
        ulong ind = ((ulong)value >> (58 - bitStart)) & 31ul;
        return base64[(int)ind];
    }

    private string TuningSettingsString()
    {
        int bitCount = 36;
        lShift = bitCount;

        ulong bits = Shift(variance_map_roomCount); // -2 --> 34
        bits |= Shift(variance_map_roomSize); // -2 --> 32
        bits |= Shift(variance_map_roomConnections); // -1 --> 31
        bits |= Shift(variance_map_corridorLength_min); // -2 --> 29
        bits |= Shift(variance_map_corridorLength_max); // -2 --> 27

        bits |= Shift(variance_level_enemyCount); // -2 --> 25
        bits |= Shift(variance_level_chestCount); // -2 --> 23
        bits |= Shift(variance_level_spawnSpread); // -2 --> 21

        bits |= Shift(variance_behaviour_available); // -1 --> 20
        bits |= Shift(variance_behaviour_selection); // -2 --> 18
        bits |= Shift(variance_behaviour_targeting); // -2 --> 16

        bits |= Shift(variance_stats_health); // -2 --> 14
        bits |= Shift(variance_stats_defence); // -2 --> 12
        bits |= Shift(variance_stats_speed); // -1 --> 11
        bits |= Shift(variance_stats_types); // -1 --> 10

        bits |= Shift(variance_damage_base); // -2 --> 8
        bits |= Shift(variance_damage_critRate); // -2 --> 6
        bits |= Shift(variance_damage_critScale); // -2 --> 4

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

[CustomEditor(typeof(RandTuning))]
[CanEditMultipleObjects]
public class RandTuningEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorElements.RequiredComponent("Necessary for pre-run randomness tuning");
    }
}
