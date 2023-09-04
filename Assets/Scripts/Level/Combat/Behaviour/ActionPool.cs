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
using static CombatAction;
using Unity.VisualScripting;

public enum ActionPoolCategory { None = -1, Standard, Advanced, Special }
public static class ActionPool
{
    public static float[] SINGLE = new float[] { 0.6f, 1.1f, 1.7f, 2.5f };
    public static float[] BLAST = new float[] { 0.5f, 0.9f, 1.4f, 2.0f };
    public static float[] AOE = new float[] { 0.3f, 0.6f, 1.0f, 1.5f };
    public static float ATTACK = 1.0f;
    public static float HEAL = 0.3f;
    public static float SHIELD = 1.6f;

    public static CombatAction[] Standard = new CombatAction[]
    {
        // 00 ATTACK - SINGLE TARGET
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[0] * ATTACK,
        },
        // 01 ATTACK - SINGLE TARGET
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1] * ATTACK,
        },
        // 02 ATTACK - SINGLE TARGET
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[3] * ATTACK,
        },

        // 03 ATTACK - BLAST
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[0] * ATTACK,
        },
        // 04 ATTACK - BLAST
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[1] * ATTACK,
        },

        // 05 ATTACK - AOE
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.OpposedAll,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[0] * ATTACK,
        },
        // 06 ATTACK - AOE
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.OpposedAll,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[1] * ATTACK,
        },

        // 07 HEAL - SELF
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Self,
                condition = new TargetCondition(TargConType.Health_Percent, 60, TargConStrength.Full, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[0] * HEAL,
        },
        // HEAL - SELF
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Self,
                condition = new TargetCondition(TargConType.Health_Percent, 60, TargConStrength.Full, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1] * HEAL,
        },
        // HEAL - SELF
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Self,
                condition = new TargetCondition(TargConType.Health_Percent, 60, TargConStrength.Full, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * HEAL,
        },
        
        // 08 HEAL - SINGLE TARGET
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[0] * HEAL,
        },
        // 09 HEAL - SINGLE TARGET
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1] * HEAL,
        },
        // 10 HEAL - SINGLE TARGET
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * HEAL,
        },
        
        // 11 HEAL - AOE
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.AlliedAll,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[0] * HEAL,
        },
        // 12 HEAL - AOE
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.AlliedAll,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[1] * HEAL,
        },

        // 13 SHIELD - SELF
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Self,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[0] * SHIELD,
        },
        // 14 SHIELD - SELF
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Self,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1] * SHIELD,
        },
        // 15 SHIELD - SELF
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Self,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * SHIELD,
        },
        
        // 16 SHIELD - SINGLE TARGET
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[0] * SHIELD,
        },
        // 17 SHIELD - SINGLE TARGET
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1] * SHIELD,
        },
        // 18 SHIELD - SINGLE TARGET
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * SHIELD,
        },

        // 19 SHIELD - BLAST
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[0] * SHIELD,
        },
        // 20 SHIELD - BLAST
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent, true),
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[1] * SHIELD,
        },
        
        // 21 SHIELD - AOE
        // LOW POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.AlliedAll,
                condition = TargetCondition.None,
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[0] * SHIELD,
        },
        // 22 SHIELD - AOE
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Shield,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.AlliedAll,
                condition = TargetCondition.None,
                tauntable = false,
                ignoreMarked = true,
            },
            baseAttribute = CombatantAttribute.Defence,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[1] * SHIELD,
        },
    };
    public static CombatAction[] Advanced = new CombatAction[]
    {
        // 00 ATTACK - SINGLE TARGET
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * ATTACK,
        },
        // 01 ATTACK - SINGLE TARGET
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[3] * ATTACK,
        },
        
        // 02 ATTACK - BLAST
        // 03 MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[1] * ATTACK,
        },
        // 04 ATTACK - BLAST
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[2] * ATTACK,
        },
        // 05 ATTACK - BLAST
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[3] * ATTACK,
        },
        
        // 06 ATTACK - AOE
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.OpposedAll,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[1] * ATTACK,
        },
        // 07 ATTACK - AOE
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.OpposedAll,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[2] * ATTACK,
        },

        // 08 HEAL - SELF
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Self,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * HEAL,
        },
        // 09 HEAL - SELF
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[3] * HEAL,
        },
        
        // 10 HEAL - SINGLE TARGET
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * HEAL,
        },
        // 11 HEAL - SINGLE TARGET
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Allied,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[3] * HEAL,
        },
        
        // 12 HEAL - AOE
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.AlliedAll,
                condition = TargetCondition.None,
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[2] * HEAL,
        },
        // 13 HEAL - AOE
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Heal,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.AlliedAll,
                condition = TargetCondition.None,
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[3] * HEAL,
        },

        // 14 SHIELD - SELF
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * SHIELD,
        },
        // 15 SHIELD - SELF
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[3] * SHIELD,
        },
        
        // 16 SHIELD - SINGLE TARGET
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[2] * SHIELD,
        },
        // 17 SHIELD - SINGLE TARGET
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[3] * SHIELD,
        },
        
        // 18 SHIELD - BLAST
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = BLAST[2] * SHIELD,
        },
        // 19 SHIELD - BLAST
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = BLAST[3] * SHIELD,
        },
        
        // 20 SHIELD - AOE
        // MEDIUM POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[1] * SHIELD,
        },
        // 21 SHIELD - AOE
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[2] * SHIELD,
        },

        // INFO
        // INFO
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
    };
    public static CombatAction[] Special = new CombatAction[]
    {
        // ATTACK - SINGLE TARGET
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
        
        // ATTACK - BLAST
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },

        // ATTACK - AOE
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },

        // ATTACK - AOE
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
        
        // HEAL - AOE
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
        // HEAL - AOE
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },

        // SHIELD - SELF
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
        
        // SHIELD - SINGLE TARGET
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
        
        // SHIELD - BLAST
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
        // SHIELD - BLAST
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },

        // SHIELD - AOE
        // HIGH POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
        // SHIELD - AOE
        // EXTREME POWER
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },

        // INFO
        // INFO
        new CombatAction()
        {
            type = CombatActionType.Attack,
            
            targeting = new ActionTarget()
            {

            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = 1.0f,
        },
    };

    public static CombatAction[] PlayerBasic = new CombatAction[]
    {
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Attack,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = SINGLE[1],
        },
    };
    public static CombatAction[] PlayerSkill = new CombatAction[]
    {
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[1],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[1],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[1],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.Basic,
            actionMultiplier = BLAST[1],
        },
    };
    public static CombatAction[] PlayerUltimate = new CombatAction[]
    {
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[2],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[2],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[2],
        },
        new CombatAction()
        {
            type = CombatActionType.Attack,
            targeting = new ActionTarget()
            {
                selection = TargetSelection.Opposed,
                condition = new TargetCondition(TargConType.Health_Percent),
                tauntable = true,
                ignoreMarked = false,
            },
            baseAttribute = CombatantAttribute.Health,
            multiTarget = MultiTargetAttributes.None,
            actionMultiplier = AOE[2],
        },
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

// SET NAME FORMAT

// ???_..._..._... [Type]
//  > STD = Standard
//  > SMN = Summoned
//  > ELT = Elite
//  > BSS = Boss

// .._???_..._... [Role]
//  > DMG = Damage
//  > SPT = Support
//  > BFR = Buffer
//  > DBF = Debuffer

// ..._..._???_... [Targeting]
//  > SNG = Single-target
//  > BLS = Blast
//  > AOE = AoE

// ..._..._..._??? [Identifier]

[System.Serializable]
public enum ActionSetName
{
    None,

    STD_DMG_SNG_001, STD_DMG_SNG_002,
    STD_DMG_BLS_001,
    STD_DMG_AOE_001,

    STD_SPT_SNG_001,
    STD_SPT_AOE_001,

    ELT_DMG_SNG_001,
    ELT_DMG_BLS_001,
    ELT_DMG_AOE_001,

    ELT_BFR_AOE_001,

    ELT_DBF_SNG_OO1,

    BSS_DMG_BLS_001,

    PLAYER_A, PLAYER_B, PLAYER_C, PLAYER_D
}
public class ActionSet
{
    private static RandTuning RandTuning { get { return Core.RandTuning; }}

    public struct SetNameRange
    {
        public ushort indStart;
        public ushort indEnd;

        public SetNameRange(ushort indStart, ushort indEnd)
        {
            if (indStart <= indEnd)
            {
                this.indStart = indStart;
                this.indEnd = indEnd;
            }
            else
            {
                this.indStart = indEnd;
                this.indEnd = indStart;
            }
            if (this.indStart > typeof(ActionSetName).GetCount() - 2)
                this.indStart = (ushort)(typeof(ActionSetName).GetCount() - 2);
            if (this.indEnd > typeof(ActionSetName).GetCount())
                this.indEnd = (ushort)(typeof(ActionSetName).GetCount() - 1);
            else if (this.indEnd <= this.indStart)
                this.indEnd = (ushort)(this.indStart + 1);
        }
    }
    public static SetNameRange Enemies_Standard = new SetNameRange(1, 4);
    public static SetNameRange Enemies_Elite = new SetNameRange();
    public static SetNameRange Enemies_Boss = new SetNameRange();

    public ActionCopyRef[] standardRefs = new ActionCopyRef[0];
    public ActionCopyRef[] advancedRefs = new ActionCopyRef[0];
    public ActionCopyRef[] specialRefs = new ActionCopyRef[0];

    public CombatAction[] standard;
    public CombatAction[] advanced;
    public CombatAction[] special;

    public ActionSetRef[] Sequence;

    public ActionSet()
    {

    }
    
    public static ActionSet GetSet(ActionSetName setName)
    {
        switch (setName)
        {
            default:
                return new ActionSet();

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            // ENEMY SETS

            case ActionSetName.STD_DMG_SNG_001:
                return new ActionSet() {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(1, "Slice", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "Impale", null),
                    },
                    specialRefs = new ActionCopyRef[0],
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Advanced, 0),
                    }
                };
            case ActionSetName.STD_DMG_SNG_002:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(1, "Potshot", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "Steady Shot", null),
                    },
                    specialRefs = new ActionCopyRef[0],
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Advanced, 0),
                    }
                };
            case ActionSetName.STD_DMG_BLS_001:
                return  new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "Basic Attack ()", null),
                        new ActionCopyRef(3, "Basic Attack ()", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(2, "Skill Attack ()", null),
                    },
                    specialRefs = new ActionCopyRef[0],
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 1),
                        new ActionSetRef(ActionPoolCategory.Advanced, 0),
                    }
                };
            case ActionSetName.STD_DMG_AOE_001:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "Basic Attack ()", null),
                        new ActionCopyRef(16, "Basic Attack ()", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(5, "Skill Attack ()", null),
                    },
                    specialRefs = new ActionCopyRef[0],
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 1),
                        new ActionSetRef(ActionPoolCategory.Advanced, 0),
                    }
                };

            case ActionSetName.STD_SPT_SNG_001:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "Basic Attack (Single)", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(10, "Skill Heal (Single)", null),
                    },
                    specialRefs = new ActionCopyRef[0],
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Advanced, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                    }
                };
            case ActionSetName.STD_SPT_AOE_001:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "Basic Attack (Single)", null),
                        new ActionCopyRef(19, "Basic Shield (Blast)", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(12, "Skill Heal (AoE)", null),
                    },
                    specialRefs = new ActionCopyRef[0],
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Advanced, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 1),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Advanced, 0),
                    }
                };

            case ActionSetName.ELT_DMG_SNG_001:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                    }
                };
            case ActionSetName.ELT_DMG_BLS_001:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                    }
                };
            case ActionSetName.ELT_DMG_AOE_001:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                    }
                };

            case ActionSetName.ELT_BFR_AOE_001:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                    }
                };

            case ActionSetName.ELT_DBF_SNG_OO1:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "name", null),
                    },
                    Sequence = new ActionSetRef[]
                    {
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                        new ActionSetRef(ActionPoolCategory.Standard, 0),
                    }
                };

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            // PLAYER SETS

            case ActionSetName.PLAYER_A:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "A_BASIC", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "A_SKILL", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(0, "A_ULT", null),
                    },
                    Sequence = null
                };
            case ActionSetName.PLAYER_B:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(1, "B_BASIC", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(1, "B_SKILL", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(1, "B_ULT", null),
                    },
                    Sequence = null
                };
            case ActionSetName.PLAYER_C:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(2, "C_BASIC", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(2, "C_SKILL", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(2, "C_ULT", null),
                    },
                    Sequence = null
                };
            case ActionSetName.PLAYER_D:
                return new ActionSet()
                {
                    standardRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(3, "D_BASIC", null),
                    },
                    advancedRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(3, "D_SKILL", null),
                    },
                    specialRefs = new ActionCopyRef[]
                    {
                        new ActionCopyRef(3, "D_ULT", null),
                    },
                    Sequence = null
                };
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void GetActions(DamageType.Type damageType)
    {
        int i;
        standard = new CombatAction[standardRefs.Length];
        for (i = 0; i < standardRefs.Length; i++)
        {
            if (ActionPool.Standard.InBounds(standardRefs[i].index))
            {
                standard[i] = ActionPool.Standard[i].Copy(standardRefs[i]);
                if ((int)damageType > 0)
                    standard[i].damageType = (int)damageType;
            }
        }
        advanced = new CombatAction[advancedRefs.Length];
        for (i = 0; i < advancedRefs.Length; i++)
        {
            if (ActionPool.Standard.InBounds(advancedRefs[i].index))
            {
                advanced[i] = ActionPool.Advanced[i].Copy(advancedRefs[i]);
                if ((int)damageType > 0)
                    advanced[i].damageType = (int)damageType;
            }
        }
        special = new CombatAction[specialRefs.Length];
        for (i = 0; i < specialRefs.Length; i++)
        {
            if (ActionPool.Standard.InBounds(specialRefs[i].index))
            {
                special[i] = ActionPool.Special[i].Copy(specialRefs[i]);
                if ((int)damageType > 0)
                    special[i].damageType = (int)damageType;
            }
        }
    }
    
    public void GetPlayerActions(DamageType.Type damageType)
    {
        int i;
        standard = new CombatAction[standardRefs.Length];
        for (i = 0; i < standardRefs.Length; i++)
        {
            if (ActionPool.PlayerBasic.InBounds(standardRefs[i].index))
            {
                standard[i] = ActionPool.PlayerBasic[i].Copy(standardRefs[i]);
                if ((int)damageType > 0)
                    standard[i].damageType = (int)damageType;
            }
        }
        advanced = new CombatAction[advancedRefs.Length];
        for (i = 0; i < advancedRefs.Length; i++)
        {
            if (ActionPool.PlayerSkill.InBounds(advancedRefs[i].index))
            {
                advanced[i] = ActionPool.PlayerSkill[i].Copy(advancedRefs[i]);
                if ((int)damageType > 0)
                    advanced[i].damageType = (int)damageType;
            }
        }
        special = new CombatAction[specialRefs.Length];
        for (i = 0; i < specialRefs.Length; i++)
        {
            if (ActionPool.PlayerUltimate.InBounds(specialRefs[i].index))
            {
                special[i] = ActionPool.PlayerUltimate[i].Copy(specialRefs[i]);
                if ((int)damageType > 0)
                    special[i].damageType = (int)damageType;
            }
        }
    }

    public float ExecuteAction(CombatantCore combatant, ActionPoolCategory cat, int index, bool allowVariance = true)
    {
        int variance = RandTuning.valBhv_selection;
        float f1, f2, r = allowVariance ? Random.Range(0.0f, 1.0f) : 0.0f;
        ExecutionData executionData = ExecutionData.Empty;
        switch (cat)
        {
            default:
            case ActionPoolCategory.Standard:
                if (standard.InBounds(index))
                {
                    if (allowVariance)
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
                                    f2 += f1 * (i == index ? 2.0f : 1.0f);
                                    if (r <= f2)
                                    {
                                        index = i;
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
                                        index = i;
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    executionData = standard[index].Execute(combatant);
                }
                break;

            case ActionPoolCategory.Advanced:
                if (advanced.InBounds(index))
                {
                    if (allowVariance)
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
                                    f2 += f1 * (i == index ? 2.0f : 1.0f);
                                    if (r <= f2)
                                    {
                                        index = i;
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
                                        index = i;
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    executionData = advanced[index].Execute(combatant);
                }
                break;

            case ActionPoolCategory.Special:
                if (special.InBounds(index))
                {
                    if (allowVariance)
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
                                    f2 += f1 * (i == index ? 2.0f : 1.0f);
                                    if (r <= f2)
                                    {
                                        index = i;
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
                                        index = i;
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    executionData = special[index].Execute(combatant);
                }
                break;
        }
        return executionData.empty || !executionData.succeeded ? 1f : executionData.duration;
    }
    
    public float ExecuteAction(CombatantCore combatant, CombatantTeamIndex[] targets, ActionPoolCategory cat, int index, bool allowVariance = true)
    {
        int variance = RandTuning.valBhv_selection;
        float f1, f2, r = allowVariance ? Random.Range(0.0f, 1.0f) : 0.0f;
        ExecutionData executionData = ExecutionData.Empty;
        switch (cat)
        {
            default:
            case ActionPoolCategory.Standard:
                if (standard.InBounds(index))
                {
                    if (allowVariance)
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
                                    f2 += f1 * (i == index ? 2.0f : 1.0f);
                                    if (r <= f2)
                                    {
                                        index = i;
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
                                        index = i;
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    executionData = standard[index].Execute(combatant, targets);
                }
                break;

            case ActionPoolCategory.Advanced:
                if (advanced.InBounds(index))
                {
                    if (allowVariance)
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
                                    f2 += f1 * (i == index ? 2.0f : 1.0f);
                                    if (r <= f2)
                                    {
                                        index = i;
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
                                        index = i;
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    executionData = advanced[index].Execute(combatant, targets);
                }
                break;

            case ActionPoolCategory.Special:
                if (special.InBounds(index))
                {
                    if (allowVariance)
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
                                    f2 += f1 * (i == index ? 2.0f : 1.0f);
                                    if (r <= f2)
                                    {
                                        index = i;
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
                                        index = i;
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    executionData = special[index].Execute(combatant, targets);
                }
                break;
        }
        return executionData.empty || !executionData.succeeded ? 1f : executionData.duration;
    }

    public float ExecuteAction(CombatantCore combatant, int sequenceIndex, bool allowVariance = true)
    {
        if (Sequence.InBounds(sequenceIndex))
        {
            int variance = RandTuning.valBhv_selection;
            allowVariance &= variance > 0;
            bool randCat = variance > 2;
            variance = Mathf.Clamp(variance, 0, 2);
            ActionPoolCategory cat = randCat ? (ActionPoolCategory)Random.Range(0, 3) : Sequence[sequenceIndex].category;
            int index = Sequence[sequenceIndex].index;
            return ExecuteAction(combatant, cat, index, allowVariance);
        }
        return 0f;
    }
}
