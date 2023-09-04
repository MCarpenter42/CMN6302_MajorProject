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

#region [ ENUM TYPES ]

public enum ControlState { None, Menu, World, Combat }

#endregion

public class Core : MonoBehaviourExt
{
    #region [ OBJECTS / COMPONENTS ]

    public static EventSystem EventSystem = null;
    public static LevelManager LevelManager = null;
    public static UIManager UIManager = null;
    public static RandTuning RandTuning = null;

    #endregion

    #region [ PROPERTIES ]

    public const char markdownDelimiter = '%';

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static void SetRef_LevelManager(LevelManager levelManager)
    {
        //if (LevelManager != null)
            //Debug.LogWarning("Static LevelManager setter accessed with a value already present! The previous value has been overridden.\n" + FindObjectsOfType<LevelManager>().Length + " object(s) of this type found in the current scene.");
        LevelManager = levelManager;
    }
    public static void SetRef_UIManager(UIManager uiManager)
    {
        //if (UIManager != null)
            //Debug.LogError("Static UIManager setter accessed with a value already present! The previous value has been overridden.\n" + FindObjectsOfType<UIManager>().Length + " object(s) of this type found in the current scene.");
        UIManager = uiManager;
    }
    public static void SetRef_RandTuning(RandTuning randTuning)
    {
        //if (RandTuning == null)
            //Debug.LogError("Static RandTuning setter accessed with a value already present! The previous value has been overridden.\n" + FindObjectsOfType<RandTuning>().Length + " object(s) of this type found in the current scene.");
        RandTuning = randTuning;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static void Pause()
    {
        GameManager.Instance.OnPause();
    }

    public static void Resume()
    {
        GameManager.Instance.OnResume();
    }

    public static void TogglePause()
    {
        if (GameManager.allowPauseToggle)
        {
            if (GameManager.gamePaused)
                GameManager.Instance.OnResume();
            else
                GameManager.Instance.OnPause();
        }
    }

    public static void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static string DynamicDescription(CombatantCore combatant, StatusEffect effect, string descriptionRaw, int roundTo = 2)
    {
        string str = descriptionRaw, description = "", check;
        float val;
        if (descriptionRaw != null)
        {
            int[] inds = new int[3] { str.IndexOf(markdownDelimiter), -1, -1 };
            while (inds[0] > -1 && str.Length > inds[0] + 1)
            {
                if (inds[0] > 0)
                {
                    description += str.Substring(0, inds[0]);
                }
                inds[1] = str.IndexOf(' ', inds[0] + 1);
                inds[2] = str.IndexOf(markdownDelimiter, inds[0] + 1);
                if (inds[2] < 0)
                {
                    description += str.Substring(inds[1]);
                    inds[0] = -1;
                }
                else if (inds[1] > -1 && inds[1] < inds[2])
                {
                    description += str.Substring(0, inds[2]);
                    str = str.Substring(inds[2]);
                    inds[0] = 0;
                    inds[1] = -1;
                    inds[2] = -1;
                }
                else
                {
                    check = str.Substring(inds[0] + 1, inds[2] - inds[0] - 1);
                    bool firstUpper = check.LatinBasicUppercase(0);
                    switch (check.ToLower())
                    {
                        default:
                            description += "<VALUE>";
                            break;

                        case "turnstarttick":
                            description += (firstUpper ? 'A' : 'a') + "t the " + (effect.tickOnTurnStart ? "start" : "end") + " of each turn";
                            break;

                        case "removable":
                            description += (firstUpper ? 'C' : 'c') + "an" + (effect.removable ? " " : "'t ") + "be removed";
                            break;

                        case "dot_type":
                            description += effect.healthOverTime == null ? "TYPE" : effect.healthOverTime.type.displayName;
                            break;

                        case "dot_val":
                            description += effect.healthOverTime.value;
                            break;

                        case "dmgout_type":
                            description += effect.dmgOutModifier == null ? "TYPE" : DamageType.Defaults[effect.dmgOutModifier.typeID].displayName;
                            break;

                        case "dmgout_perc":
                            if (effect.dmgOutModifier == null)
                                description += "PERCENT%";
                            else
                            {
                                val = (float)System.Math.Round((effect.dmgOutModifier.value - 1.0f) * 100.0f, roundTo);
                                description += val + "%";
                            }
                            break;

                        case "dmgout_dirc":
                            if (effect.dmgOutModifier == null)
                                description += "CHANGES";
                            else
                            {
                                if (effect.dmgOutModifier.value >= 1.0f)
                                    description += (firstUpper ? 'I' : 'i') + "ncreases";
                                else
                                    description += (firstUpper ? 'D' : 'd') + "ecreases";
                            }
                            break;

                        case "dmgin_type":
                            description += effect.dmgInModifier == null ? "TYPE" : DamageType.Defaults[effect.dmgInModifier.typeID].displayName;
                            break;

                        case "dmgin_perc":
                            if (effect.dmgInModifier == null)
                                description += "PERCENT%";
                            else
                            {
                                val = (float)System.Math.Round((effect.dmgInModifier.floatValue - 1.0f) * 100.0f, roundTo);
                                description += val + "%";
                            }
                            break;

                        case "dmgin_dirc":
                            if (effect.dmgInModifier == null)
                                description += "CHANGES";
                            else
                            {
                                if (effect.dmgInModifier.floatValue >= 1.0f)
                                    description += (firstUpper ? 'I' : 'i') + "ncreases";
                                else
                                    description += (firstUpper ? 'D' : 'd') + "ecreases";
                            }
                            break;
                    }
                    if (str.Length > inds[2] + 1)
                    {
                        str = str.Substring(inds[2] + 1);
                        inds[0] = str.IndexOf(markdownDelimiter);
                    }
                    else
                    {
                        str = "";
                        inds[0] = -1;
                    }
                }
            }
            description += str;
        }
        return description;
    }
}
