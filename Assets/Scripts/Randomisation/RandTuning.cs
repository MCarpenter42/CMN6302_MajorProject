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
    public enum Var2Bit { Off, Low, High, Max }

    #region [ LEVEL GENERATION ] (13)

    public Var2Bit variance_map_maxSize;
    public bool variance_map_roomConnections;
    public Var2Bit variance_map_corridorLength_min;
    public Var2Bit variance_map_corridorLength_max;

    public Var2Bit variance_level_enemyCount;
    public Var2Bit variance_level_chestCount;
    public Var2Bit variance_level_spawnSpread;

    #endregion

    #region [ AI BEHAVIOUR ] (5)

    public bool variance_behaviour_available;
    public Var2Bit variance_behaviour_selection;
    public Var2Bit variance_behaviour_targeting;

    #endregion

    #region [ STAT VARIANCE ] (7)

    public Var2Bit variance_stats_health;
    public Var2Bit variance_stats_defence;
    public Var2Bit variance_stats_speed;
    public bool variance_stats_types;

    #endregion

    #region [ DAMAGE VARIANCE ] (6)

    public Var2Bit variance_damage_base;
    public Var2Bit variance_damage_critRate;
    public Var2Bit variance_damage_critScale;

    #endregion

    #region [  ]

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */



    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // 1 character (base 64) --> 6 characters (base 2)
    // 6 bits can hold:
    //  - 1x [00-63]
    //  - 1x [00-31] + 1x [00-01]
    //  - 1x [00-15] + 2x [00-01]
    //  - 1x [00-15] + 1x [00-03]
    //  - 1x [00-07] + 3x [00-01]
    //  - 1x [00-07] + 1x [00-03] + 1x [00-01]
    //  - 2x [00-07]
    //  - 1x [00-03] + 4x [00-01]
    //  - 2x [00-03] + 2x [00-01]
    //  - 3x [00-03]
    //  - 6x [00-01]

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

    // Use a multiple of 6 total bit count
    //  --> Any bits not used for control can be used for scrambling

    private int lShift;
    private ulong Shift(bool var)
    {
        int shift = lShift;
        lShift -= 1;
        return (var ? 1ul : 0ul) << shift;
    }
    private ulong Shift(Var2Bit var)
    {
        int shift = lShift;
        lShift -= 2;
        return (ulong)var << shift;
    }
    private ulong BitLoop(ulong value, int bitStart, int bitEnd, int shift)
    {
        uint[] filters = new uint[]
        {
            
        };
    }
    private string TuningSettingsString()
    {
        lShift = 34;
        ulong bits = 0u & Shift(variance_map_maxSize);
        bits &= Shift(variance_map_roomConnections);
        bits &= Shift(variance_map_corridorLength_min);
        bits &= Shift(variance_map_corridorLength_max);
        bits &= Shift(variance_level_enemyCount);
        bits &= Shift(variance_level_chestCount);
        bits &= Shift(variance_level_spawnSpread);
        bits &= Shift(variance_behaviour_available);
        bits &= Shift(variance_behaviour_selection);
        bits &= Shift(variance_behaviour_targeting);
        bits &= Shift(variance_stats_health);
        bits &= Shift(variance_stats_defence);
        bits &= Shift(variance_stats_speed);
        bits &= Shift(variance_stats_types);
        bits &= Shift(variance_damage_base);
        bits &= Shift(variance_damage_critRate);
        bits &= Shift(variance_damage_critScale);

        List<char> chars = new List<char>();

        return new string(chars.ToArray());
    }
    public string SettingsString { get { return TuningSettingsString(); } }
}

[CustomEditor(typeof(RandTuning))]
[CanEditMultipleObjects]
public class RandTuningEditor : Editor
{
    RandTuning targ { get { return target as RandTuning; } }
    Rect elementRect;
    GUIContent label = new GUIContent();

    public override void OnInspectorGUI()
    {
        EditorElements.RequiredComponent("Necessary for pre-run randomness tuning");
    }
}
