using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections.Unity;
using NeoCambion.Unity.Editor;
using static CombatManager;
using static UnityEditor.Progress;

public class HUDManager : UIObject
{
    public CombatManager CombatManager { get { return LevelManager.Combat; } }

    #region [ OBJECTS / COMPONENTS ]

    // WORLD
    public UIObject hudWorld;
    public UIObject interactHighlight;
    public Minimap minimap;
    public UIObject minimapFrameFullview;
    public UIObject minimapFrameCorner;

    // COMBAT (GENERAL)
    public UIObject hudCombat;
    public UIObject turnOrderAnchor;
    public TurnOrderItem turnOrderItem;
    private List<GameObject> turnOrderObjects = new List<GameObject>();
    public ActionNameDisplay actionNameDisplay;

    // COMBAT (PLAYER)
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

    #endregion

    #region [ PROPERTIES ]

    public bool interactHLVisible = false;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        AbilityButtonSetup();
        minimap.SetState(MinimapState.Corner);

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
        Vector3 objPos = GameManager.Instance.playerW.targetInteract.transform.position;
        interactHighlight.transform.position = GameManager.Instance.cameraW.cam.WorldToScreenPoint(objPos);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void ShowMinimap(bool show) => SetMinimapState(show ? MinimapState.Corner : MinimapState.Hidden);

    public void SetMinimapState(MinimapState state)
    {
        switch(state)
        {
            default:
            case MinimapState.Corner:
                break;

            case MinimapState.Fullview:
                break;

            case MinimapState.Hidden:
                break;
        }
    }

    public void ToggleMinimap() => minimap.Toggle();

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void AbilityButtonSetup()
    {
        pAbilBasic.ind = 0;
        pAbilBasic.selRing.Show(false);
        pAbilSkill.ind = 1;
        pAbilSkill.selRing.Show(false);
        pAbilUltimate.ind = 2;
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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void DrawActionOrder(TurnOrderRef[] actionOrder)
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

    public void ActionNameDisplay(string actionName, float duration, float hangRatio = 0.8f)
    {
        actionNameDisplay.Display(actionName, duration, hangRatio);
    }
}

[CustomEditor(typeof(HUDManager))]
[CanEditMultipleObjects]
public class HUDManagerEditor : Editor
{
    HUDManager targ { get { return target as HUDManager; } }
    GUIContent label = new GUIContent();

    public override void OnInspectorGUI()
    {
        EditorElements.SectionHeader("World");
        EditorGUILayout.Space(0);

        label.text = "World Space HUD";
        label.tooltip = null;
        targ.hudWorld = EditorGUILayout.ObjectField(label, targ.hudWorld, typeof(UIObject), true) as UIObject;
        EditorGUILayout.Space(0);
        
        label.text = "Interaction Highlight";
        label.tooltip = null;
        targ.interactHighlight = EditorGUILayout.ObjectField(label, targ.interactHighlight, typeof(UIObject), true) as UIObject;
        EditorGUILayout.Space(0);
        
        label.text = "Minimap Handler";
        label.tooltip = null;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minimap"), label);
        //targ.minimap = EditorGUILayout.ObjectField(label, targ.minimap, typeof(Minimap), true) as Minimap;

        label.text = "Minimap Frame";
        label.tooltip = null;
        targ.interactHighlight = EditorGUILayout.ObjectField(label, targ.interactHighlight, typeof(UIObject), true) as UIObject;
        EditorGUILayout.Space(8);


        EditorElements.SectionHeader("Combat (General)");
        EditorGUILayout.Space(0);

        label.text = "Combat HUD";
        label.tooltip = null;
        targ.hudCombat = EditorGUILayout.ObjectField(label, targ.hudCombat, typeof(UIObject), true) as UIObject;
        EditorGUILayout.Space(0);

        label.text = "Turn Order Anchor";
        label.tooltip = null;
        targ.turnOrderAnchor = EditorGUILayout.ObjectField(label, targ.turnOrderAnchor, typeof(UIObject), true) as UIObject;
        EditorGUILayout.Space(0);

        label.text = "Turn Order Item";
        label.tooltip = null;
        targ.turnOrderItem = EditorGUI.ObjectField(EditorElements.PrefabLabel(label), targ.turnOrderItem, typeof(TurnOrderItem), false) as TurnOrderItem;
        EditorGUILayout.Space(0);

        label.text = "Action Name Display";
        label.tooltip = null;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("actionNameDisplay"), label);
        //targ.actionNameDisplay = EditorGUILayout.ObjectField(label, targ.actionNameDisplay, typeof(ActionNameDisplay), true) as ActionNameDisplay;
        EditorGUILayout.Space(8);


        EditorElements.SectionHeader("Combat (Player)");
        EditorGUILayout.Space(0);

        label.text = "Rotator";
        label.tooltip = null;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotator"), label);
        //targ.rotator = EditorGUILayout.ObjectField(label, targ.rotator, typeof(ConstRotate), true) as ConstRotate;
        EditorGUILayout.Space(0);
        
        label.text = "Basic Ability Icon";
        label.tooltip = null;
        targ.pAbilBasic = EditorGUILayout.ObjectField(label, targ.pAbilBasic, typeof(AbilityButton), true) as AbilityButton;
        EditorGUILayout.Space(0);

        label.text = "Skill Icon";
        label.tooltip = null;
        targ.pAbilSkill = EditorGUILayout.ObjectField(label, targ.pAbilSkill, typeof(AbilityButton), true) as AbilityButton;
        EditorGUILayout.Space(0);

        label.text = "Ultimate Icon";
        label.tooltip = null;
        targ.pAbilUltimate = EditorGUILayout.ObjectField(label, targ.pAbilUltimate, typeof(AbilityButton), true) as AbilityButton;
        EditorGUILayout.Space(8);

        /*label.text = "";
        label.tooltip = null;
        targ.interactHighlight = EditorGUILayout.ObjectField(label, targ.interactHighlight, typeof(UIObject), true) as UIObject;
        EditorGUILayout.Space(2);*/

        serializedObject.ApplyModifiedProperties();
    }
}
