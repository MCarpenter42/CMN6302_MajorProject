using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;
#endif

public class HUDManager : UIObject
{
    public CombatManager CombatManager { get { return LevelManager.Combat; } }

    #region [ OBJECTS / COMPONENTS ]

    public UIObject hudWorld;
    public UIObject interactHighlight;
    public Minimap minimap;
    public UIObject minimapFrameFullview;
    public UIObject minimapFrameCorner;
    public HealthBarCanvas[] playerHealthBars_World;

    public PopupPrompt confirmPrompt;

    public UIObject hudCombat;
    public HealthBarCanvas[] playerHealthBars_Combat;
    public UIObject turnOrderAnchor;
    public TurnOrderItem turnOrderItem;
    private List<GameObject> turnOrderObjects = new List<GameObject>();
    public ActionNameDisplay actionNameDisplay;
    public EndScreen endWin;
    public EndScreen endLose;

    public UIConstRotate rotator;
    public AbilityButton pAbilBasic;
    public AbilityButton pAbilSkill;
    public AbilityButton pAbilUltimate;
    [HideInInspector] public ActionPoolCategory selectedAbilityType = ActionPoolCategory.None;
    public AbilityButton selectedAbilityButton
    {
        get
        {
            switch (selectedAbilityType)
            {
                default:
                    return null;

                case ActionPoolCategory.Standard:
                    return pAbilBasic;

                case ActionPoolCategory.Advanced:
                    return pAbilSkill;

                case ActionPoolCategory.Special:
                    return pAbilUltimate;
            }
        }
    }
    public ControlIcon useSelectedIcon;

    public Image skillPowerDisplay;
    public CounterRotateChildren skillPowerMask;
    public List<PowerIndictorSetting> skillPowerDisplaySettings = new List<PowerIndictorSetting>();
    public Image ultPowerDisplay;
    public CounterRotateChildren ultPowerMask;
    public List<PowerIndictorSetting> ultPowerDisplaySettings = new List<PowerIndictorSetting>();

    #endregion

    #region [ PROPERTIES ]

    public bool interactHLVisible = false;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Initialise()
    {
        base.Initialise();
    }

    protected override void Update()
    {
        base.Update();
        if (interactHighlight.visible)
            UpdateInteractHighlight();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void ShowHUD(ControlState state = ControlState.None)
    {
        if (state == ControlState.World)
        {
            hudWorld.Show(true);
            hudCombat.Show(false);
        }
        else if (state == ControlState.Combat)
        {
            hudWorld.Show(false);
            hudCombat.Show(true);
        }
        else
        {
            hudWorld.Show(false);
            hudCombat.Show(false);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SetInteractHightlightVis(bool visible) => interactHighlight.Show(visible);

    private void UpdateInteractHighlight()
    {
        InteractPoint interact = GameManager.Instance.Player == null ? null : GameManager.Instance.Player.targetInteract;
        if (interact != null)
        {
            Vector3 objPos = interact.transform.position;
            interactHighlight.transform.position = GameManager.Instance.WorldCam.WorldToScreenPoint(objPos);
        }
        else
        {
            SetInteractHightlightVis(false);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void ShowMinimap(bool show) => SetMinimapState(show ? MinimapState.Corner : MinimapState.Hidden);

    public void SetMinimapState(MinimapState state) => minimap.SetState(state);

    public void ToggleMinimap() => minimap.Toggle();

    private void SetPromptButtonStates(bool showMiddle)
    {
        Vector2 size = confirmPrompt.buttons[1].rTransform.sizeDelta;
        Vector3[] positions = new Vector3[]
        {
            confirmPrompt.buttons[0].rTransform.anchoredPosition,
            confirmPrompt.buttons[1].rTransform.anchoredPosition,
            confirmPrompt.buttons[2].rTransform.anchoredPosition,
        };

        size.x += showMiddle ? 0f : 60f;

        positions[0].x = showMiddle ? -(size.x + 20f) : -(size.x / 2f + 10f);
        positions[1].y = showMiddle ? -65f : 15f;
        positions[2].x = showMiddle ? size.x + 20f : size.x / 2f + 10f;

        confirmPrompt.buttons[0].rTransform.anchoredPosition = positions[0];
        confirmPrompt.buttons[1].rTransform.anchoredPosition = positions[1];
        confirmPrompt.buttons[2].rTransform.anchoredPosition = positions[2];

        confirmPrompt.buttons[1].Show(showMiddle);
    }
    public void UpdatePrompt(string title, string description, PromptButtonData buttonA, PromptButtonData buttonB, PromptButtonData buttonC)
    {
        confirmPrompt.title.text = title;
        confirmPrompt.description.text = description;

        confirmPrompt.buttons[0].Button.onClick.RemoveAllListeners();
        confirmPrompt.buttons[0].label.text = buttonA.label;
        foreach (UnityAction action in buttonA.actions)
        {
            confirmPrompt.buttons[0].Button.onClick.AddListener(action);
        }

        confirmPrompt.buttons[1].Button.onClick.RemoveAllListeners();
        confirmPrompt.buttons[1].label.text = buttonB.label;
        foreach (UnityAction action in buttonB.actions)
        {
            confirmPrompt.buttons[1].Button.onClick.AddListener(action);
        }

        confirmPrompt.buttons[2].Button.onClick.RemoveAllListeners();
        confirmPrompt.buttons[2].label.text = buttonC.label;
        foreach (UnityAction action in buttonC.actions)
        {
            confirmPrompt.buttons[2].Button.onClick.AddListener(action);
        }

        SetPromptButtonStates(buttonB.enable);
    }
    public void ShowPrompt(string title, string description, PromptButtonData buttonA, PromptButtonData buttonB, PromptButtonData buttonC)
    {
        UpdatePrompt(title, description, buttonA, buttonB, buttonC);
        confirmPrompt.Animate(true, 0.6f);
    }

    public void HidePrompt() => confirmPrompt.Animate(false, 0.6f);

    public void UpdateWorldHealthBars(bool healAnim = false)
    {
        int[] health;
        for (int i = 0; i < 4; i++)
        {
            health = GameManager.Instance.GameData.runData.p_healthValues[i];
            if (healAnim)
                playerHealthBars_World[i].SetValueWithFlash(health[0], health[1], false, 0.6f);
            else
                playerHealthBars_World[i].SetValue(health[0], health[1], 0f);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void AbilityButtonSetup()
    {
        pAbilBasic.selRing.Show(false);
        pAbilSkill.selRing.Show(false);
        pAbilUltimate.selRing.Show(false);

        selectedAbilityType = ActionPoolCategory.None;
    }

    public void OnAbilityButtonSelected(ActionPoolCategory selected)
    {
        if ((int)selected > -1 && (int)selected < 3)
        {
            selectedAbilityType = selected;
            rotator.SetTargetTransform(selectedAbilityButton.selRing.rTransform);
            selectedAbilityButton.selRing.Show(true);
            switch (selected)
            {
                default:
                    selectedAbilityButton.SelRingColour(true);
                    break;

                case ActionPoolCategory.Advanced:
                    selectedAbilityButton.SelRingColour(CombatManager.canUseSkill);

                    break;
                case ActionPoolCategory.Special:
                    selectedAbilityButton.SelRingColour(CombatManager.canUseUlt);

                    break;
            }
            useSelectedIcon.Show(true);
            useSelectedIcon.rTransform.position = new Vector3(32 + selectedAbilityButton.rTransform.position.x, useSelectedIcon.rTransform.position.y, 0f);
        }
        else
        {
            selectedAbilityButton.selRing.Show(false);
            rotator.SetTargetTransform(null);
            selectedAbilityType = ActionPoolCategory.None;
        }
    }

    public void OnAbilityButtonDeselected(ActionPoolCategory deselected)
    {
        if ((int)deselected > -1 && (int)deselected < 3)
        {
            if (selectedAbilityType == deselected)
            {
                rotator.SetTargetTransform(null);
                selectedAbilityType = (ActionPoolCategory)(-1);
                useSelectedIcon.Show(false);
            }
            switch (deselected)
            {
                default:
                    break;
                case ActionPoolCategory.Standard:
                    pAbilBasic.selRing.Show(false);
                    break;
                case ActionPoolCategory.Advanced:
                    pAbilSkill.selRing.Show(false);
                    break;
                case ActionPoolCategory.Special:
                    pAbilUltimate.selRing.Show(false);
                    break;
            }
        }
    }

    private bool abilityButtonsEnabled = true;
    public void SetAbilityButtonsEnabled(bool enable)
    {
        if (enable != abilityButtonsEnabled)
        {
            if (!enable)
            {
                if (selectedAbilityButton != null)
                    selectedAbilityButton.selRing.Show(false);
                selectedAbilityType = ActionPoolCategory.None;
                rotator.SetTargetTransform(null);
                useSelectedIcon.Show(false);
            }
            abilityButtonsEnabled = enable;
            pAbilBasic.enabled = enable;
            pAbilSkill.enabled = enable;
            pAbilUltimate.enabled = enable;
        }
    }

    public void AbilityButtonPressed(ActionPoolCategory select)
    {
        switch (select)
        {
            default: break;
            case ActionPoolCategory.Standard:
                pAbilBasic.OnPointerClick(null);
                break;
            case ActionPoolCategory.Advanced:
                pAbilSkill.OnPointerClick(null);
                break;
            case ActionPoolCategory.Special:
                pAbilUltimate.OnPointerClick(null);
                break;
        }
    }

    public void UpdateSkillPowerDisplay(int value)
    {
        if (skillPowerDisplaySettings.InBounds(value))
        {
            skillPowerDisplay.color = skillPowerDisplaySettings[value].color;
            skillPowerMask.SetRotation(skillPowerDisplaySettings[value].rotation);
        }
    }
    public void UpdateUltPowerDisplay(int value)
    {
        if (ultPowerDisplaySettings.InBounds(value))
        {
            ultPowerDisplay.color = ultPowerDisplaySettings[value].color;
            ultPowerMask.SetRotation(ultPowerDisplaySettings[value].rotation);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void DrawActionOrder(CombatManager.TurnOrderRef[] actionOrder)
    {
        int i;
        if (turnOrderObjects.Count > actionOrder.Length)
        {
            for (i = turnOrderObjects.Count - 1; i >= actionOrder.Length; i++)
            {
                Destroy(turnOrderObjects[i]);
                turnOrderObjects.RemoveAt(i);
            }
        }
        else if (turnOrderObjects.Count < actionOrder.Length)
        {
            for (i = turnOrderObjects.Count; i < actionOrder.Length; i++)
            {
                TurnOrderItem lItem = Instantiate(turnOrderItem, turnOrderAnchor.transform);
                lItem.rTransform.anchoredPosition = Vector3.zero + Vector3.up * -80 * i;
                turnOrderObjects.Add(lItem.gameObject);
            }
        }

        int index;
        string name;
        for (i = 0; i < actionOrder.Length; i++)
        {
            index = actionOrder[i].index;
            name = actionOrder[i].index + ": " + (actionOrder[i].allyTeam ? CombatManager.combatants.allyTeam[index].displayName : CombatManager.combatants.enemyTeam[index].displayName);
            turnOrderObjects[i].GetComponent<TurnOrderItem>().SetName(name);
        }
    }

    public void ClearActionOrder()
    {
        for (int i = turnOrderAnchor.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(turnOrderAnchor.transform.GetChild(i).gameObject);
        }
    }

    public void ActionNameDisplay(string actionName, float duration, float hangRatio = 0.8f)
    {
        actionNameDisplay.Display(actionName, duration, hangRatio);
    }

    public void CombatEndScreen(bool playerWon, float wait)
    {
        if (playerWon)
            endWin.Animation(wait);
        else
            endLose.Animation(wait);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HUDManager))]
[CanEditMultipleObjects]
public class HUDManagerEditor : Editor
{
    HUDManager targ { get { return target as HUDManager; } }
    GUIContent label = new GUIContent();

    public bool showWorld = true;
    public bool showCombatGeneral = true;
    public bool showCombatAbilityButtons = true;
    public bool showCombatAbilityPower = false;

    public override void OnInspectorGUI()
    {
        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding, GUIStyle.none);
        {
            label.text = "World";
            label.tooltip = null;
            if (showWorld = EditorElements.FoldoutHeader(showWorld, label))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    label.text = "World Space HUD";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hudWorld"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Interaction Highlight";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactHighlight"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Minimap Handler";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("minimap"), label);

                    label.text = "Minimap Frame";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactHighlight"), label);

                    label.text = "Health Bars";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("playerHealthBars_World"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Confirmation Prompt";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("confirmPrompt"), label);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(4);

            label.text = "Combat (General)";
            label.tooltip = null;
            if (showCombatGeneral = EditorElements.FoldoutHeader(showCombatGeneral, label))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    label.text = "Combat HUD";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hudCombat"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Player Health Bars";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("playerHealthBars_Combat"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Turn Order Anchor";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("turnOrderAnchor"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Turn Order Item";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("turnOrderItem"), label);
                    //targ.turnOrderItem = EditorGUI.ObjectField(EditorElements.PrefabLabel(label), targ.turnOrderItem, typeof(TurnOrderItem), false) as TurnOrderItem;
                    EditorGUILayout.Space(0);

                    label.text = "Action Name Display";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("actionNameDisplay"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Combat Won Screen";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("endWin"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Combat Lost Screen";
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("endLose"), label);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(4);

            label.text = "Combat (Ability Buttons)";
            label.tooltip = null;
            if (showCombatAbilityButtons = EditorElements.FoldoutHeader(showCombatAbilityButtons, label))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    label.text = "Rotator";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rotator"), label);
                    //targ.rotator = EditorGUILayout.ObjectField(label, targ.rotator, typeof(ConstRotate), true) as ConstRotate;
                    EditorGUILayout.Space(0);

                    label.text = "Basic Ability Icon";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pAbilBasic"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Skill Icon";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pAbilSkill"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Ultimate Icon";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pAbilUltimate"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Use Selected";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("useSelectedIcon"), label);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(4);

            label.text = "Combat (Ability Power)";
            label.tooltip = null;
            if (showCombatAbilityPower = EditorElements.FoldoutHeader(showCombatAbilityPower, label))
            {
                EditorElements.BeginSubSection(10, 0);
                {
                    label.text = "Skill Power Display";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("skillPowerDisplay"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Skill Power Mask";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("skillPowerMask"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Skill Power Display Settings";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("skillPowerDisplaySettings"), label);
                    EditorGUILayout.Space(4);

                    label.text = "Ult Power Display";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ultPowerDisplay"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Ult Power Mask";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ultPowerMask"), label);
                    EditorGUILayout.Space(0);

                    label.text = "Ult Power Display Settings";
                    label.tooltip = null;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ultPowerDisplaySettings"), label);
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(4);
        }
        EditorElements.EndHorizVert();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

[System.Serializable]
public struct PowerIndictorSetting
{
    public float rotation;
    public Color color;

    public PowerIndictorSetting(float rotation, Color color)
    {
        this.rotation = rotation;
        this.color = color;
    }
}

public struct PromptButtonData
{
    public readonly string label;
    public readonly Callback[] callbacks;
    public readonly UnityAction[] actions;
    public readonly bool enable;
    
    public PromptButtonData(string label, Callback[] callbacks, bool enable = true)
    {
        this.label = label;
        this.callbacks = callbacks;
        actions = new UnityAction[callbacks.Length];
        for (int i = 0; i < callbacks.Length; i++)
        {
            actions[i] = callbacks[i].UAction();
        }
        this.enable = enable;
    }
}