using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.IO.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using System.Globalization;
using System.Text.RegularExpressions;
using NeoCambion.Unity.PersistentUID;
using Unity.VisualScripting;
using System;
using JetBrains.Annotations;
using UnityEngine.TextCore.Text;

public class Window_Enemies : EditorWindow
{
    protected static EditorWindow _Window = null;
    public static EditorWindow Window
    {
        get
        {
            if (_Window == null)
                _Window = GetWindow(typeof(Window_Enemies));
            return _Window;
        }
    }

    #region [ PROPERTIES ]

    private bool loaded = false;

    private bool focussed;

    private Vector2 scrollPosMain = new Vector2();
    private Vector2 scrollPosList = new Vector2();
    private Vector2 scrollPosSpeeds = new Vector2();

    private enum RegionToggle { EnemyList, Health, Attack, Defence, Speed, WeakAgainst, StrongAgainst, SetNameExplainer }
    private List<bool> regionToggles = new List<bool>();
    private int toggleCount { get { return Enum.GetNames(typeof(RegionToggle)).Length; } }

    private int selectedEnemy = -1;

    #region < UTILITY OBJECTS / DATA >

    private GUIContent label = new GUIContent();
    private Rect elementRect;

    private List<EnemyData> enemyList;

    private ushort previewLevel = 1;

    private CombatSpeed speedData = new CombatSpeed(0);
    private int editingSpeedEntry = -1;
    private int pendingSpeedValue = -1;
    private ushort newSpeedLevel = 1;
    private int newSpeedValue = 1;

    #endregion

    private float lastAvWidth;
    private float lastAvHeight;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        enemyList = GameDataStorage.LoadEnemyCache();
        if (regionToggles.Count < toggleCount)
        {
            regionToggles.PadList(toggleCount);
            int c = toggleCount - regionToggles.Count;
            for (int i = 0; i < c; i++)
            {
                regionToggles.Add(false);
            }
        }
        regionToggles[0] = true;
    }

    void OnGUI()
    {
        if (!loaded)
        {
            List<EnemyData> newData = GameDataStorage.LoadEnemyCache();
            if (newData != null)
                enemyList = newData;
            //enemyList = GameDataStorage.LoadCache<EnemyData>();
            loaded = true;
        }

        GUI.enabled = true;
        if (regionToggles.Count < toggleCount)
        {
            regionToggles.PadList(toggleCount);
            int c = toggleCount - regionToggles.Count;
            for (int i = 0; i < c; i++)
            {
                regionToggles.Add(false);
            }
        }

        float slHeight = EditorGUIUtility.singleLineHeight;
        bool darkTheme = GUI.skin.label.normal.textColor.ApproximatelyEquals(new Color(0.824f, 0.824f, 0.824f, 1.000f), 0.005f);
        RectOffset rOffZero = new RectOffset(0, 0, 0, 0);

        GUIStyle centreWrapLabel = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };

        GUIStyle boldButton = new GUIStyle(EditorStyles.miniButton)
        {
            fixedHeight = 0,
            fixedWidth = 0,
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };

        GUIStyle enemyListBg = new GUIStyle(EditorStyles.helpBox)
        {
            margin = rOffZero,
            padding = new RectOffset(4, 4, 4, 4),
        };

        bool changesMade = false;
        (_, Rect vRect) = EditorElements.BeginHorizVert(EditorStyles.inspectorFullWidthMargins, GUIStyle.none);
        {
            int i;
            float avWidth = vRect.width;
            if (avWidth > 0.0f)
                lastAvWidth = avWidth;
            else
                avWidth = lastAvWidth;
            
            EditorGUILayout.Space(8);

            label.text = "ENEMIES";
            label.tooltip = null;

            elementRect = EditorGUILayout.GetControlRect(true, slHeight);
            EditorElements.Header(elementRect, label, 18);

            Vector2 bSize = new Vector2(24, 24);
            Rect btnRect = new Rect(elementRect) { size = bSize };
            btnRect.y += (elementRect.height - btnRect.height) / 2;
            EditorElements.UndockButton(btnRect, Window);

            btnRect.x = elementRect.x + elementRect.width - btnRect.width;
            if (EditorElements.IconButton(btnRect, "SaveAs", "Force save data"))
            {
                GameDataStorage.SaveEnemyCache(enemyList);
            }
            
            btnRect.x -= btnRect.width + 10;
            if (EditorElements.IconButton(btnRect, "Collab.FileAdded", "Force load data"))
            {
                enemyList = GameDataStorage.LoadEnemyCache();
                Debug.Log(enemyList.Count + " enemies found!");
            }

            EditorElements.SeparatorBar();

            bool _showPickList = (enemyList.Count > 0 && enemyList.InBounds(selectedEnemy)) ? regionToggles[(int)RegionToggle.EnemyList] : true;

            label.text = _showPickList ? "Select Enemy to Edit" : "Editing: \"" + enemyList[selectedEnemy].displayName + "\"";
            label.tooltip = null;

            bSize = Vector2.one * (24);
            elementRect = EditorElements.ControlRect(bSize.x);
            elementRect.x += 1;
            elementRect.width -= bSize.x + 5;
            EditorElements.SectionHeader(elementRect, label, 13);

            label.text = _showPickList ? "-" : "+";
            label.tooltip = (_showPickList ? "Hide" : "Show") + " list of\ncreated enemies";

            btnRect = new Rect(elementRect);
            btnRect.size = bSize;
            btnRect.x += elementRect.width + 4;
            string btnIcon = _showPickList ? "CustomTool" : "UnityEditor.SceneHierarchyWindow";
            string btnTooltip = _showPickList ? "Edit selected\nenemy" : "Show list of\ncreated enemies";
            if (selectedEnemy > -1)
            {
                if (EditorElements.IconButton(btnRect, btnIcon, btnTooltip))
                {
                    regionToggles[0] = enemyList.Count > 0 ? !regionToggles[0] : true;
                }
            }

            EditorGUILayout.Space(4);

            if (_showPickList)
            {
                EditorGUILayout.BeginHorizontal(enemyListBg);
            }
            else
            {
                EditorGUILayout.BeginHorizontal(new GUIStyle(EditorStyles.inspectorFullWidthMargins) { padding = new RectOffset(4, 4, 0, 0) });
            }
            Vector2 scrollPos = _showPickList ? scrollPosList : scrollPosMain;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            {
                if (_showPickList)
                {
                    if (enemyList.Count > 0)
                    {
                        //EnemyData enemy in enemyList
                        for (i = 0; i < enemyList.Count; i++)
                        {
                            elementRect = EditorGUILayout.GetControlRect(true, slHeight + 4);
                            bSize.y = elementRect.height;
                            bSize.x = elementRect.height + 6;
                            elementRect.width -= bSize.x + 4;
                            if (GUI.Button(elementRect, enemyList[i].displayName, i == selectedEnemy ? EditorStylesExtras.ColouredTextButton(DynamicTextColour.green, FontStyle.Normal) : GUI.skin.button))
                            {
                                selectedEnemy = i;
                                speedData.Overwrite(enemyList[selectedEnemy].speeds);
                                regionToggles[0] = false;
                            }

                            label.text = "X";
                            label.tooltip = "Delete \"" + enemyList[i].displayName + "\"";

                            elementRect.x += elementRect.width + 4;
                            elementRect.size = bSize;
                            if (GUI.Button(elementRect, label, EditorStylesExtras.textButtonRed))
                            {
                                enemyList.RemoveAt(i);
                                selectedEnemy = 0;
                                regionToggles[0] = true;
                                changesMade = true;
                            }
                        }

                        EditorGUILayout.Space(2);

                        elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                        elementRect.x += 40;
                        elementRect.width -= 80;
                        elementRect.height += 10;
                        if (GUI.Button(elementRect, "Create New Enemy"))
                        {
                            enemyList.Add(new EnemyData() { displayName = "New Enemy" });
                            selectedEnemy = enemyList.Count - 1;
                            enemyList[selectedEnemy].friendly = false;
                            enemyList[selectedEnemy].playerControlled = false;
                            regionToggles[0] = false;
                            changesMade = true;
                        }
                    }
                    else
                    {
                        EditorGUILayout.Space(4);
                        EditorGUILayout.LabelField("No enemy data found! Click below to add one:", centreWrapLabel);
                        elementRect = EditorGUILayout.GetControlRect(true, slHeight + 10);
                        elementRect.x += 40;
                        elementRect.width -= 80;
                        if (GUI.Button(elementRect, "Create New Enemy"))
                        {
                            enemyList.Add(new EnemyData() { displayName = "New Enemy" });
                            selectedEnemy = 0;
                            regionToggles[0] = false;
                            changesMade = true;
                        }
                    }
                }
                else
                {
                    if (selectedEnemy > -1 && enemyList.Count > 0)
                    {
                        EditorGUILayout.Space(8);

                        label.text = "Display Name";
                        label.tooltip = null;
                        changesMade |= UpdateDispName(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(2);

                        label.text = "Model";
                        label.tooltip = null;
                        changesMade |= UpdateModel(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(8);

                        label.text = "Class";
                        label.tooltip = null;
                        changesMade |= UpdateClass(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(8);

                        label.text = "Wander In World";
                        label.tooltip = null;
                        changesMade |= UpdateWandering(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(4);
                        EditorElements.SeparatorBar();
                        EditorGUILayout.Space(4);

                        label.text = "Preview at Level: " + previewLevel;
                        label.tooltip = null;
                        previewLevel = (ushort)GUI.HorizontalSlider(EditorElements.PrefixLabel(label), previewLevel, 1, 100);
                        EditorGUILayout.Space(8);

                        changesMade |= UpdateAttributeData(enemyList[selectedEnemy], CombatantAttribute.Health);
                        EditorGUILayout.Space(2);
                        changesMade |= UpdateAttributeData(enemyList[selectedEnemy], CombatantAttribute.Attack);
                        EditorGUILayout.Space(2);
                        changesMade |= UpdateAttributeData(enemyList[selectedEnemy], CombatantAttribute.Defence);
                        EditorGUILayout.Space(2);
                        changesMade |= UpdateSpdData();
                        EditorGUILayout.Space(4);
                        EditorElements.SeparatorBar();
                        EditorGUILayout.Space(4);

                        label.text = "Status Inflict Chance";
                        label.tooltip = null;
                        changesMade |= UpdateInflictChance(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(2);

                        label.text = "Status Inflict Resistance";
                        label.tooltip = null;
                        changesMade |= UpdateInflictResist(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(8);

                        label.text = "Attack Damage Type";
                        label.tooltip = null;
                        changesMade |= UpdateAttackType(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(2);

                        label.text = "Damage Type Weaknesses (" + enemyList[selectedEnemy].weakAgainst.CountIf(true) + ")";
                        label.tooltip = null;
                        changesMade |= UpdateWeakAgainst(label, enemyList[selectedEnemy]);
                        EditorGUILayout.Space(10);

                        label.text = "Action Set";
                        label.tooltip = null;
                        changesMade |= UpdateActionSetName(label, enemyList[selectedEnemy]);
                    }
                    else
                        regionToggles[0] = true;
                }
            }
            EditorGUILayout.EndScrollView();
            if (_showPickList)
            {
                EditorGUILayout.EndHorizontal();
                scrollPosList = scrollPos;
            }
            else
            {
                EditorGUILayout.EndHorizontal();
                scrollPosMain = scrollPos;
            }
            EditorGUILayout.GetControlRect(false, 2.0f);
        }
        EditorElements.EndHorizVert();

        if (changesMade)
        {
            GameDataStorage.SaveEnemyCache(enemyList);
        }
    }

    void OnValidate()
    {
        if (focussed)
        {
            enemyList = GameDataStorage.LoadEnemyCache();
            GameManager.Instance.UpdateElementData();
        }
    }
    
    void OnFocus()
    {
        //Debug.Log("OnFocus executed at " + System.DateTime.Now.ToString("T", CultureInfo.GetCultureInfo("en-GB")));
        focussed = true;
    }

    void OnLostFocus()
    {
        focussed = false;
    }

    [MenuItem("Tools/Enemy Maker")]
    public static void ShowWindow()
    {
        _Window = GetWindow(typeof(Window_Enemies));
        Window.titleContent = new GUIContent("Enemy Maker");
    }

    #endregion

    #region [ VALUE UPDATE FUNCTIONS ]

    private bool UpdateDispName(GUIContent label, EnemyData combatant)
    {
        string dispName = EditorGUI.DelayedTextField(EditorElements.PrefixLabel(label), combatant.displayName);
        if (dispName != combatant.displayName)
        {
            combatant.displayName = dispName;
            return true;
        }
        return false;
    }
    
    private bool UpdateModel(GUIContent label, EnemyData combatant)
    {
        bool changesMade = false;
        string modelPath = EntityModel.GetModelPathFromUID(combatant.modelHexUID);
        GameObject modelObj = Resources.Load<GameObject>(modelPath);
        EntityModel model = EditorGUI.ObjectField(EditorElements.PrefixLabel(label), (modelObj == null ? null : modelObj.GetComponent<EntityModel>()), typeof(EntityModel), false) as EntityModel;
        string[] hexUIDs = new string[] { (modelObj == null ? "00000000" : combatant.modelHexUID), (model == null ? "00000000" : model.gameObject.GetComponent<PrefabUID>().hexUID) };
        if (hexUIDs[1] == "00000000")
        {
            if (hexUIDs[0] != "00000000")
                changesMade = true;
            combatant.modelHexUID = hexUIDs[1];
        }
        else if (hexUIDs[0] == "00000000")
        {
            changesMade = true;
            combatant.modelHexUID = model.gameObject.GetComponent<PrefabUID>().hexUID;
        }
        else
        {
            string hexUID = model.gameObject.GetComponent<PrefabUID>().hexUID;
            if (hexUID != combatant.modelHexUID)
            {
                changesMade = true;
                combatant.modelHexUID = hexUID;
            }
        }
        return changesMade;
    }

    private bool UpdateClass(GUIContent label, EnemyData combatant)
    {
        EnemyClass Class = (EnemyClass)EditorGUI.Popup(EditorElements.PrefixLabel(label), (int)combatant.Class, typeof(EnemyClass).GetNames());
        if (Class != combatant.Class)
        {
            enemyList[selectedEnemy].Class = Class;
            return true;
        }
        return false;
    }

    private bool UpdateAttributeData(EnemyData combatant, CombatantAttribute attribute)
    {
        int currentValue;
        int currentScaling;
        ReturnScalingData data;
        switch (attribute)
        {
            default:
                return false;

            case CombatantAttribute.Health:
                currentValue = combatant.baseHealth;
                currentScaling = combatant.healthScaling;
                data = ScalingStatFoldoutRegion(RegionToggle.Health, "Health", currentValue, currentScaling);
                break;

            case CombatantAttribute.Attack:
                currentValue = combatant.baseAttack;
                currentScaling = combatant.attackScaling;
                data = ScalingStatFoldoutRegion(RegionToggle.Attack, "Attack", currentValue, currentScaling);
                break;

            case CombatantAttribute.Defence:
                currentValue = combatant.baseDefence;
                currentScaling = combatant.defenceScaling;
                data = ScalingStatFoldoutRegion(RegionToggle.Defence, "Defence", currentValue, currentScaling);
                break;
        }

        bool changed = currentValue != data.value || currentScaling != data.scaling;
        if (changed)
        {
            switch (attribute)
            {
                default:
                    break;

                case CombatantAttribute.Health:
                    combatant.baseHealth = data.value;
                    combatant.healthScaling = data.scaling;
                    break;

                case CombatantAttribute.Attack:
                    combatant.baseAttack = data.value;
                    combatant.attackScaling = data.scaling;
                    break;

                case CombatantAttribute.Defence:
                    combatant.baseDefence = data.value;
                    combatant.defenceScaling = data.scaling;
                    break;
            }
        }
        return changed;
    }
    
    private bool UpdateSpdData()
    {
        if (SpeedListFoldoutRegion(enemyList[selectedEnemy].speeds))
        {
            enemyList[selectedEnemy].speeds = speedData.GetList();
            return true;
        }
        return false;
    }
    
    private bool UpdateInflictChance(GUIContent label, EnemyData combatant)
    {
        int inflictChance = EditorElements.IntPercentSlider(EditorElements.PrefixLabel(label), combatant.inflictChance);
        if (inflictChance != combatant.inflictChance)
        {
            combatant.inflictChance = inflictChance;
            return true;
        }
        return false;
    }

    private bool UpdateInflictResist(GUIContent label, EnemyData combatant)
    {
        int inflictResist = EditorElements.IntPercentSlider(EditorElements.PrefixLabel(label), combatant.inflictResist);
        if (inflictResist != combatant.inflictResist)
        {
            combatant.inflictResist = inflictResist;
            return true;
        }
        return false;
    }

    private bool UpdateAttackType(GUIContent label, EnemyData combatant)
    {
        DamageType.Type attackType = (DamageType.Type)EditorGUI.Popup(EditorElements.PrefixLabel(label), (int)combatant.attackType, DamageType.TypeNames);
        if (attackType != enemyList[selectedEnemy].attackType)
        {
            enemyList[selectedEnemy].attackType = attackType;
            return true;
        }
        return false;
    }
    
    private bool UpdateWeakAgainst(GUIContent label, EnemyData combatant)
    {
        int i, nWeak = DamageType.TypeCount, toggleWidth = 20;
        string[] damageTypeOptions = DamageType.TypeNames;
        bool changesMade = false;
        if (regionToggles[(int)RegionToggle.WeakAgainst] = EditorGUILayout.Foldout(regionToggles[(int)RegionToggle.WeakAgainst], label, true, EditorStylesExtras.foldoutLabel))
        {
            nWeak = combatant.weakAgainst.Length;
            if (nWeak < DamageType.TypeCount)
            {
                bool[] bools = new bool[DamageType.TypeCount];
                for (i = 0; i < nWeak; i++)
                {
                    bools[i] = combatant.weakAgainst[i];
                }
                for (i = nWeak; i < DamageType.TypeCount; i++)
                {
                    bools[i] = false;
                }
                combatant.weakAgainst = bools;
                changesMade = true;
            }
            else if (nWeak > DamageType.TypeCount)
            {
                bool[] bools = new bool[DamageType.TypeCount];
                for (i = 0; i < DamageType.TypeCount; i++)
                {
                    bools[i] = combatant.weakAgainst[i];
                }
                combatant.weakAgainst = bools;
                changesMade = true;
            }

            EditorElements.BeginSubSection(10.0f, 0);
            {
                bool weakAgainst;
                for (i = 1; i < combatant.weakAgainst.Length; i++)
                {
                    if (i != (int)combatant.attackType)
                    {
                        elementRect = EditorElements.ControlRect();
                        elementRect.x += 2;
                        weakAgainst = EditorGUI.Toggle(elementRect, combatant.weakAgainst[i]);
                        if (weakAgainst != combatant.weakAgainst[i])
                        {
                            combatant.weakAgainst[i] = weakAgainst;
                            changesMade = true;
                        }
                        elementRect.x += toggleWidth - 2;
                        elementRect.width -= toggleWidth;
                        label.text = damageTypeOptions[i];
                        EditorGUI.LabelField(elementRect, label);
                    }
                    else
                    {
                        elementRect = EditorElements.ControlRect();
                        EditorGUI.LabelField(elementRect, EditorElements.IconContentBuiltin("scenepicking_notpickable-mixed_hover", "Combatants cannot have an innate\nweankess to their attack type!"));
                        combatant.weakAgainst[i] = false;
                        elementRect.x += toggleWidth;
                        elementRect.width -= toggleWidth;
                        label.text = damageTypeOptions[i];
                        EditorGUI.LabelField(elementRect, label);
                    }
                }
            }
            EditorElements.EndSubSection();
        }
        return changesMade;
    }

    private bool UpdateActionSetName(GUIContent label, EnemyData combatant)
    {
        int btnSize = 20;
        Rect totalRect = EditorElements.ControlRect(btnSize), btnRect = new Rect(totalRect);

        btnRect.width = btnSize;
        ActionSetNameExplanation(btnRect);

        totalRect.width -= btnSize + 4;
        totalRect.x += btnSize + 4;
        ActionSetName actionSet = (ActionSetName)EditorGUI.Popup(EditorElements.PrefixLabel(totalRect, label, EditorGUIUtility.labelWidth - btnSize - 4), (int)combatant.actionSet, typeof(ActionSetName).GetNames());
        if (actionSet != combatant.actionSet)
        {
            enemyList[selectedEnemy].actionSet = actionSet;
            return true;
        }
        return false;
    }

    private void ActionSetNameExplanation(Rect btnRect)
    {
        bool region = regionToggles[(int)RegionToggle.SetNameExplainer], toggle = EditorElements.IconButton(btnRect, "_Help", "Set name formatting" + (region ? "\n[HIDE]" : "\n[SHOW]"));
        if (regionToggles[(int)RegionToggle.SetNameExplainer] = toggle ? !region : region)
        {
            GUIStyle monospaceListItem = EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label) { font = OSFonts.SystemDefaultMonospace });
            float wLabel = 42.0f;

            EditorGUILayout.Space(2);
            EditorElements.BeginSubSection(10.0f, 0);
            {
                EditorGUILayout.Space(2);
                EditorElements.SectionHeader("AAA_???_???_???");
                // CHANGE TO NOT USE PrefixLabel
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • STD", wLabel, monospaceListItem), " - Standard");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • ELT", wLabel, monospaceListItem), " - Elite");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • BSS", wLabel, monospaceListItem), " - Boss");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • SMN", wLabel, monospaceListItem), " - Summoned");

                EditorGUILayout.Space(10);
                EditorElements.SectionHeader("???_AAA_???_???");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • DMG", wLabel, monospaceListItem), " - Attacker");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • SPT", wLabel, monospaceListItem), " - Support");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • BFR", wLabel, monospaceListItem), " - Buffer");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • DBF", wLabel, monospaceListItem), " - Debuffer");

                EditorGUILayout.Space(10);
                EditorElements.SectionHeader("???_???_AAA_???");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • SNG", wLabel, monospaceListItem), " - Single-target");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • BLS", wLabel, monospaceListItem), " - Blast");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • AOE", wLabel, monospaceListItem), " - AoE");

                EditorGUILayout.Space(10);
                EditorElements.SectionHeader("???_???_???_AAA");
                EditorGUI.LabelField(EditorElements.PrefixLabel(" • ###", wLabel, monospaceListItem), " - Numerical Identifier");
            }
            EditorElements.EndSubSection();
        }
    }

    private bool UpdateWandering(GUIContent label, EnemyData combatant)
    {
        elementRect = EditorElements.ControlRect();
        Rect toggleRect = new Rect(elementRect);
        toggleRect.x += 1;
        toggleRect.width = elementRect.height;
        elementRect.x += toggleRect.width + 1;
        elementRect.width -= toggleRect.width + 1;
        bool wanderInWorld = EditorGUI.Toggle(toggleRect, combatant.wanderInWorld);
        EditorGUI.LabelField(elementRect, label);
        if (wanderInWorld != combatant.wanderInWorld)
        {
            combatant.wanderInWorld = wanderInWorld;
            return true;
        }
        return false;
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private struct ReturnScalingData
    {
        public int value;
        public int scaling;

        public ReturnScalingData(int value, int scaling)
        {
            this.value = value;
            this.scaling = scaling;
        }
    }

    private ReturnScalingData ScalingStatFoldoutRegion(RegionToggle region, string headerText, int baseValue, int valueScaling)
    {
        label.text = headerText;
        label.tooltip = null;

        ReturnScalingData returnData = new ReturnScalingData(baseValue, valueScaling);
        int r = (int)region;
        if (regionToggles.InBounds(r))
        {
            if (regionToggles[r] = EditorGUILayout.Foldout(regionToggles[r], label, true, EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10.0f, 0);
                {
                    label.text = "Base Value";
                    label.tooltip = null;

                    returnData.value = EditorGUILayout.DelayedIntField(label, baseValue);
                    if (returnData.value < 1)
                        returnData.value = 1;

                    label.text = "Scaling Factor";
                    label.tooltip = null;

                    elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                    Rect rSlider = new Rect(elementRect);
                    rSlider.width -= 44;
                    Rect rBox = new Rect(rSlider);
                    rBox.x += rSlider.width + 4;
                    rBox.width = 40;
                    int[] returnScaling = new int[2];
                    returnScaling[0] = (int)GUI.HorizontalSlider(rSlider, valueScaling, 0.0f, 100.0f).Round(0);
                    returnScaling[1] = (int)EditorElements.IntPercentField(rBox, valueScaling, false);
                    if (returnScaling[0] == valueScaling)
                        returnData.scaling = returnScaling[1];
                    else
                        returnData.scaling = returnScaling[0];

                    EditorGUILayout.Space(2.0f);

                    label.text = "Scaled to Level " + previewLevel;
                    label.tooltip = null;

                    elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                    int scaledValue = CombatValue.ScaledInt(returnData.value, previewLevel, returnData.scaling / 100.0f);
                    EditorElements.ReadonlyField(elementRect, scaledValue.ToString());
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(4.0f);
        }

        return returnData;
    }

    private struct ReturnSpeedData
    {
        public SpeedAtLevel[] data;
        public bool changed;

        public ReturnSpeedData(SpeedAtLevel[] data, bool changed)
        {
            this.data = data;
            this.changed = changed;
        }
    }

    private struct ReturnSpeedAtLevel
    {
        public Rect[] rects;
        public bool changed;

        public ReturnSpeedAtLevel(Rect[] rects, bool changed)
        {
            this.rects = rects;
            this.changed = changed;
        }
    }

    private bool SpeedListFoldoutRegion(SpeedAtLevel[] speeds)
    {
        speedData.Overwrite(speeds);

        label.text = "Speed";
        label.tooltip = null;

        bool speedDataChanged = false;

        if (regionToggles[(int)RegionToggle.Speed] = EditorGUILayout.Foldout(regionToggles[(int)RegionToggle.Speed], label, true, EditorStylesExtras.foldoutLabel))
        {
            EditorElements.BeginSubSection(10.0f, 0);
            {
                // Column headers: "Level Threshold" / "Speed"
                // Scroll rect: Speed data list
                    // See "DrawSpeedAtLevel" function
                // Input fields: [Level Threshold] / [Speed Value]
                // Function buttons: <Add new data to list from input fields> / <Clear input fields>
                    // Add: Creates new "SpeedAtLevel" object from input field values & adds to list
                    // Clear: Removes any values from input fields

                elementRect = EditorGUILayout.GetControlRect();
                elementRect.x += 23;
                elementRect.width -= 56;
                Rect[] columHeaderRects = SpeedAtLevelRects(elementRect, 104);

                label.text = "Level Threshold";
                EditorGUI.LabelField(columHeaderRects[0], label, EditorStylesExtras.LabelStyle(TextAnchor.MiddleCenter, FontStyle.Bold));
                EditorElements.GreyRect(columHeaderRects[1]);
                label.text = "Speed Value";
                EditorGUI.LabelField(columHeaderRects[2], label, EditorStylesExtras.LabelStyle(TextAnchor.MiddleCenter, FontStyle.Bold));

                float itemHeight = EditorGUIUtility.singleLineHeight + 16, scrollHeight = itemHeight * (speedData.valueCount > 8 ? 8 : speedData.valueCount) + 10;
                //GUIStyle listBox = new GUIStyle(GUI.skin.box) { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0) };
                scrollPosSpeeds = EditorGUILayout.BeginScrollView(scrollPosSpeeds, false, true, GUILayout.Height(scrollHeight), GUILayout.MaxHeight(scrollHeight));
                {
                    for (int i = 0; i < speedData.valueCount; i++)
                    {
                        elementRect = EditorGUILayout.GetControlRect(false, itemHeight);
                        elementRect.x += 20;
                        elementRect.width -= 40;
                        speedDataChanged |= DrawSpeedAtLevel(elementRect, i).changed;
                    }
                    EditorGUILayout.Space(7);
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();

                elementRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 8);
                elementRect.x += 32;
                elementRect.width = (elementRect.width - 100) / 2;
                elementRect.y += 4;
                elementRect.height = EditorGUIUtility.singleLineHeight;
                int nsl = EditorGUI.DelayedIntField(elementRect, newSpeedLevel);
                if (nsl != newSpeedLevel && nsl > 0)
                    newSpeedLevel = (ushort)nsl;

                elementRect.x += elementRect.width + 4;
                int nsv = EditorGUI.DelayedIntField(elementRect, newSpeedValue);
                if (nsv != newSpeedValue && nsv > 0)
                    newSpeedValue = nsv;

                elementRect.x += elementRect.width + 4;
                elementRect.width = 24;
                elementRect.y += (elementRect.height - 24) / 2;
                elementRect.height = 24;
                GUIStyle btnStyle = GUI.skin.button;
                btnStyle.padding = new RectOffset(2, 2, 2, 2);
                if (GUI.Button(elementRect, EditorElements.IconContentBuiltin("Toolbar Plus", "Add Entry"), btnStyle))
                {
                    speedData.Add(new SpeedAtLevel(newSpeedLevel, newSpeedValue));
                    newSpeedLevel = 1;
                    newSpeedValue = 1;
                    speedDataChanged = true;
                }
            }
            EditorElements.EndSubSection();
        }
        else
        {
            editingSpeedEntry = -1;
            pendingSpeedValue = -1;
            newSpeedLevel = 1;
            newSpeedValue = 1;
        }

        return speedDataChanged;
    }

    private ReturnSpeedAtLevel DrawSpeedAtLevel(Rect position, int ind)
    {
        // Get data from "speedData" at index
        // Display fields: [Level Threshold] / [Speed Value]
        // Function buttons: <Edit> / <Delete>
            // Edit: Writes entry values into input fields
            // Delete: Removes entry
        if (speedData.IndexInBounds(ind))
        {
            bool speedDataChanged = false;

            position.height -= 4;
            position.y += 2;
            EditorGUI.LabelField(position, "", GUI.skin.box);
            position.size -= Vector2.one * 8;
            position.position += Vector2.one * 4;

            SpeedAtLevel spd = speedData[ind];
            Rect[] rects = SpeedAtLevelRects(position);
            GUIStyle infoLabel = EditorStylesExtras.LabelStyle(TextAnchor.MiddleCenter, FontStyle.Normal);
            GUIContent label = new GUIContent() { tooltip = null };

            GUIStyle btnStyle = GUI.skin.button;
            btnStyle.padding = new RectOffset(2, 2, 2, 2);

            label.text = spd.levelThreshold.ToString() + " +";
            EditorGUI.LabelField(rects[0], label, infoLabel);

            EditorElements.GreyRect(rects[1]);

            if (editingSpeedEntry != ind)
            {
                label.text = spd.value.ToString();
                EditorGUI.LabelField(rects[2], label, infoLabel);

                if (EditorElements.IconButton(rects[3], "CustomTool", "Edit Entry"))
                {
                    editingSpeedEntry = ind;
                    pendingSpeedValue = spd.value;
                }

                if (ind > 0 && spd.levelThreshold > 0)
                {
                    if (EditorElements.IconButton(rects[4], "TreeEditor.Trash", "Delete Entry"))
                    {
                        speedData.RemoveAt(ind);
                        speedDataChanged = true;
                    }
                }
            }
            else
            {
                if (pendingSpeedValue == -1)
                    pendingSpeedValue = spd.value;

                Rect rEditBox = rects[2];
                rEditBox.x += 4;
                rEditBox.width -= 8;
                pendingSpeedValue = EditorGUI.IntField(rEditBox, pendingSpeedValue);

                if (EditorElements.IconButton(rects[3], "SaveAs", "Save Changes"))
                {
                    if (pendingSpeedValue > 0)
                    {
                        speedData[ind].value = pendingSpeedValue;
                        speedDataChanged = true;
                    }
                    editingSpeedEntry = -1;
                }

                if (EditorElements.IconButton(rects[4], "winbtn_win_close", "Discard Changes"))
                {
                    editingSpeedEntry = -1;
                }
            }

            return new ReturnSpeedAtLevel(rects, speedDataChanged);
        }
        return new ReturnSpeedAtLevel(null, false);
    }

    private Rect[] SpeedAtLevelRects(Rect position, float minDisplayWidth = 100)
    {
        Rect[] rects = new Rect[5];

        float w1 = position.width > (2 * minDisplayWidth + 76) ? ((position.width - 76) / 2.0f) : minDisplayWidth, w2 = 4, w3 = 24;
        float x = position.x, y = position.y, h = position.height;
        rects[0] = new Rect() { x = x, y = y, width = w1, height = h };
        x += w1;
        rects[1] = new Rect() { x = x + 1, y = y, width = 2, height = h };
        x += w2;
        rects[2] = new Rect() { x = x, y = y, width = w1, height = h };
        x += w1 + w2;
        y += (h - w3) / 2;
        h = w3;
        rects[3] = new Rect() { x = x, y = y, width = w3, height = h };
        x += w3 + w2;
        rects[4] = new Rect() { x = x, y = y, width = w3, height = h };

        return rects;
    }
}
