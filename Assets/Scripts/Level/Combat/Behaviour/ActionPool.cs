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

public enum ActionPoolCategory { Standard, Advanced, Special }
public static class ActionPool
{
    public static CombatAction[] Standard = new CombatAction[]
    {

    };

    public static CombatAction[] Advanced = new CombatAction[]
    {

    };

    public static CombatAction[] Special = new CombatAction[]
    {

    };

    public static CombatAction[] PlayerBasic = new CombatAction[]
    {

    };

    public static CombatAction[] PlayerSkill = new CombatAction[]
    {

    };

    public static CombatAction[] PlayerUltimate = new CombatAction[]
    {

    };
}

public struct ActionSetRef
{
    public ActionPoolCategory category;
    public int index;

    public ActionSetRef(ActionPoolCategory category = ActionPoolCategory.Standard, int index = -1)
    {
        this.category = category;
        this.index = index;
    }
}

[System.Serializable]
public enum ActionSetName { None, BasicAttackSingle, BasicAttackBlast, BasicAttackAoE,  }
public class ActionSet
{
    public int[] standardInds = new int[0];
    public int[] advancedInds = new int[0];
    public int[] specialInds = new int[0];

    public CombatAction[] standard;
    public CombatAction[] advanced;
    public CombatAction[] special;

    public ActionSetRef[] Sequence;

    public ActionSet()
    {

    }
    
    public static ActionSet GetSet(ActionSetName setName)
    {
        ActionSet set;
        switch (setName)
        {
            default:
                return null;

            case ActionSetName.BasicAttackSingle:
                set = new ActionSet()
                {
                    standardInds = new int[]
                    {

                    },
                    advancedInds = new int[]
                    {

                    },
                    specialInds = new int[]
                    {

                    },
                    Sequence = new ActionSetRef[]
                    {

                    }
                };
                break;

            case ActionSetName.BasicAttackBlast:
                set = new ActionSet()
                {
                    standardInds = new int[]
                    {

                    },
                    advancedInds = new int[]
                    {

                    },
                    specialInds = new int[]
                    {

                    },
                    Sequence = new ActionSetRef[]
                    {

                    }
                };
                break;

            case ActionSetName.BasicAttackAoE:
                set = new ActionSet()
                {
                    standardInds = new int[]
                    {

                    },
                    advancedInds = new int[]
                    {

                    },
                    specialInds = new int[]
                    {

                    },
                    Sequence = new ActionSetRef[]
                    {

                    }
                };
                break;
        }
        return set;
    }

    public void GetActions(DamageType.Type damageType, bool randomise = false)
    {
        int i;
        standard = new CombatAction[standardInds.Length];
        for (i = 0; i < standardInds.Length; i++)
        {
            if (ActionPool.Standard.InBounds(standardInds[i]))
            {
                standard[i] = ActionPool.Standard[i];
                if ((int)damageType > 0)
                    standard[i].damageType = (int)damageType;
            }
        }
        advanced = new CombatAction[advancedInds.Length];
        for (i = 0; i < advancedInds.Length; i++)
        {
            if (ActionPool.Standard.InBounds(advancedInds[i]))
            {
                advanced[i] = ActionPool.Advanced[i];
                if ((int)damageType > 0)
                    advanced[i].damageType = (int)damageType;

            }
        }
        special = new CombatAction[specialInds.Length];
        for (i = 0; i < specialInds.Length; i++)
        {
            if (ActionPool.Standard.InBounds(specialInds[i]))
            {
                special[i] = ActionPool.Special[i];
                if ((int)damageType > 0)
                    special[i].damageType = (int)damageType;
            }
        }
    }

    public float ExecuteAction(CombatantCore combatant, int index)
    {
        if (Sequence.InBounds(index))
        {
            int variance = GameManager.Instance.RandTuning.value_behaviour_selection;
            bool doRand = variance > 0, randCat = variance > 2;
            variance = Mathf.Clamp(variance, 0, 2);
            ActionPoolCategory cat = randCat ? (ActionPoolCategory)Random.Range(0, 3) : Sequence[index].category;
            int ind = Sequence[index].index;
            float f1, f2, r = doRand ? Random.Range(0.0f, 1.0f) : 0.0f;
            switch (cat)
            {
                default:
                case ActionPoolCategory.Standard:
                    if (standard.InBounds(ind))
                    {
                        if (doRand)
                        {
                            switch (variance)
                            {
                                default:
                                    break;

                                case 1:
                                    f1 = 1.0f / standard.Length + 1;
                                    f2 = -f1;
                                    for (int i = 0; i < standard.Length; i++)
                                    {
                                        f2 += f1 * (i == ind ? 2.0f : 1.0f);
                                        if (r <= f2)
                                        {
                                            ind = i;
                                            break;
                                        }
                                    }
                                    break;

                                case 2:
                                    f1 = 1.0f / standard.Length;
                                    f2 = -f1;
                                    for (int i = 0; i < standard.Length; i++)
                                    {
                                        f2 += f1;
                                        if (r <= f2)
                                        {
                                            ind = i;
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }
                        standard[ind].Execute(combatant);
                    }
                    break;

                case ActionPoolCategory.Advanced:
                    if (advanced.InBounds(ind))
                    {
                        if (doRand)
                        {
                            switch (variance)
                            {
                                default:
                                    break;

                                case 1:
                                    f1 = 1.0f / advanced.Length + 1;
                                    f2 = -f1;
                                    for (int i = 0; i < advanced.Length; i++)
                                    {
                                        f2 += f1 * (i == ind ? 2.0f : 1.0f);
                                        if (r <= f2)
                                        {
                                            ind = i;
                                            break;
                                        }
                                    }
                                    break;

                                case 2:
                                    f1 = 1.0f / advanced.Length;
                                    f2 = -f1;
                                    for (int i = 0; i < advanced.Length; i++)
                                    {
                                        f2 += f1;
                                        if (r <= f2)
                                        {
                                            ind = i;
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }
                        advanced[ind].Execute(combatant);
                    }
                    break;

                case ActionPoolCategory.Special:
                    if (special.InBounds(ind))
                    {
                        if (doRand)
                        {
                            switch (variance)
                            {
                                default:
                                    break;

                                case 1:
                                    f1 = 1.0f / special.Length + 1;
                                    f2 = -f1;
                                    for (int i = 0; i < special.Length; i++)
                                    {
                                        f2 += f1 * (i == ind ? 2.0f : 1.0f);
                                        if (r <= f2)
                                        {
                                            ind = i;
                                            break;
                                        }
                                    }
                                    break;

                                case 2:
                                    f1 = 1.0f / special.Length;
                                    f2 = -f1;
                                    for (int i = 0; i < special.Length; i++)
                                    {
                                        f2 += f1;
                                        if (r <= f2)
                                        {
                                            ind = i;
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }
                        special[ind].Execute(combatant);
                    }
                    break;
            }
        }
        return 0.0f;
    }
}
