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

    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ OBJECT-COMPONENT VALIDATION ]



    #endregion

    #region [ PROPERTIES ]

    private bool focussed;

    private Vector2 scrollPosMain = new Vector2();
    private Vector2 scrollPosList = new Vector2();

    private enum RegionToggle { EnemyList, Health, Attack, Defence }
    private bool[] regionToggles = new bool[4];
    private int toggleCount { get { return Enum.GetNames(typeof(RegionToggle)).Length; } }

    private int selectedEnemy = -1;

    #region < UTILITY OBJECTS / DATA >

    private GUIContent label = new GUIContent();
    private Rect elementRect;

    List<CombatantData> enemyList;

    private ushort previewLevel = 1;

    #endregion

    private float lastAvWidth;
    private float lastAvHeight;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        enemyList = ElementDataStorage.LoadCache<CombatantData>();
        if (regionToggles.Length != toggleCount)
            regionToggles = new bool[toggleCount];
        regionToggles[0] = true;
        for (int i = 1; i < regionToggles.Length; i++)
        {
            regionToggles[i] = false;
        }
    }

    void OnGUI()
    {
        GUI.enabled = true;

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
            fontStyle = FontStyle.Bold
        };

        GUIStyle enemyListBg = new GUIStyle(EditorStyles.helpBox)
        {
            margin = rOffZero,
            padding = new RectOffset(4, 4, 4, 4),
        };

        bool changesMade = false;
        EditorGUILayout.BeginHorizontal(EditorStyles.inspectorFullWidthMargins);
        {
            float avWidth = EditorGUILayout.BeginVertical(EditorStylesExtras.noMarginsNoPadding).width;
            if (avWidth > 0.0f)
                lastAvWidth = avWidth;
            else
                avWidth = lastAvWidth;
            {
                EditorGUILayout.Space(8.0f);

                label.text = "ENEMIES";
                label.tooltip = null;

                elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                EditorElements.Header(elementRect, label, 18);

                Vector2 bSize;
                Rect btnRect;

                if (Window.docked)
                {
                    btnRect = new Rect(elementRect);
                    btnRect.size = new Vector2(56, 20);
                    btnRect.y += (elementRect.height - btnRect.height) / 2;
                    if (GUI.Button(btnRect, "Undock", GUI.skin.button))
                    {
                        Window.position = Window.position;
                    }
                }

                EditorElements.SeparatorBar();

                elementRect = EditorGUILayout.GetControlRect(true, slHeight + 2);
                bSize = Vector2.one * (elementRect.height + 4);
                elementRect.width -= bSize.x + 4;

                bool _showPickList = (enemyList.Count > 0 && enemyList.InBounds(selectedEnemy)) ? regionToggles[0] : true;

                label.text = _showPickList ? "Select Enemy to Edit" : "Editing: \"" + enemyList[selectedEnemy].displayName + "\"";
                label.tooltip = null;

                EditorElements.Header(elementRect, label, 13, TextAnchor.MiddleLeft);

                label.text = _showPickList ? "-" : "+";
                label.tooltip = (_showPickList ? "Hide" : "Show") + " list of\ncreated enemies";

                btnRect = new Rect(elementRect);
                btnRect.size = bSize;
                btnRect.x += elementRect.width + 4;
                if (selectedEnemy > -1)
                {
                    if (GUI.Button(btnRect, label, boldButton))
                    {
                        regionToggles[0] = enemyList.Count > 0 ? !regionToggles[0] : true;
                    }
                }

                EditorGUILayout.Space(4.0f);

                if (_showPickList)
                {
                    EditorGUILayout.BeginHorizontal(enemyListBg);
                }
                Vector2 scrollPos = _showPickList ? scrollPosList : scrollPosMain;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
                {
                    if (_showPickList)
                    {
                        if (enemyList.Count > 0)
                        {
                            //EnemyData enemy in enemyList
                            for (int i = 0; i < enemyList.Count; i++)
                            {
                                elementRect = EditorGUILayout.GetControlRect(true, slHeight + 4);
                                bSize.y = elementRect.height;
                                bSize.x = elementRect.height + 6;
                                elementRect.width -= bSize.x + 4;
                                if (GUI.Button(elementRect, enemyList[i].displayName, i == selectedEnemy ? EditorStylesExtras.ColouredTextButton(DynamicTextColour.lightBlue, FontStyle.Normal) : GUI.skin.button))
                                {
                                    selectedEnemy = i;
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

                            EditorGUILayout.Space(2.0f);

                            elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                            elementRect.x += 40;
                            elementRect.width -= 80;
                            elementRect.height += 10;
                            if (GUI.Button(elementRect, "Create New Enemy"))
                            {
                                enemyList.Add(new CombatantData("New Enemy"));
                                selectedEnemy = enemyList.Count - 1;
                                enemyList[selectedEnemy].isFriendly = false;
                                regionToggles[0] = false;
                                changesMade = true;
                            }
                        }
                        else
                        {
                            EditorGUILayout.Space(4.0f);
                            EditorGUILayout.LabelField("No enemy data found! Click below to add one:", centreWrapLabel);
                            elementRect = EditorGUILayout.GetControlRect(true, slHeight + 10);
                            elementRect.x += 40;
                            elementRect.width -= 80;
                            if (GUI.Button(elementRect, "Create New Enemy"))
                            {
                                enemyList.Add(new CombatantData("New Enemy"));
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
                            EditorGUILayout.Space(4.0f);

                            label.text = "Display Name";
                            label.tooltip = null;

                            string dispName = enemyList[selectedEnemy].displayName;

                            elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                            dispName = EditorGUI.DelayedTextField(elementRect, label, dispName);
                            if (dispName != enemyList[selectedEnemy].displayName)
                            {
                                changesMade = true;
                                enemyList[selectedEnemy].displayName = dispName;
                            }

                            label.text = "Model";
                            label.tooltip = null;

                            string modelPath = EntityModel.GetModelPathFromUID(enemyList[selectedEnemy].modelHexUID);
                            GameObject modelObj = Resources.Load<GameObject>(modelPath);
                            //Debug.Log(enemyList[selectedEnemy].modelHexUID + " | " + modelPath + " | " + (modelObj == null ? "null" : modelObj.name));
                            
                            elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                            EntityModel model = EditorGUI.ObjectField(elementRect, label, (modelObj == null ? null : modelObj.GetComponent<EntityModel>()), typeof(EntityModel), false) as EntityModel;
                            string[] hexUIDs = new string[] { (modelObj == null ? "00000000" : enemyList[selectedEnemy].modelHexUID), (model == null ? "00000000" : model.gameObject.GetComponent<PrefabUID>().hexUID) };
                            if (hexUIDs[1] == "00000000")
                            {
                                if (hexUIDs[0] != "00000000")
                                    changesMade = true;
                                enemyList[selectedEnemy].modelHexUID = hexUIDs[1];
                            }
                            else if (hexUIDs[0] == "00000000")
                            {
                                changesMade = true;
                                enemyList[selectedEnemy].modelHexUID = model.gameObject.GetComponent<PrefabUID>().hexUID;
                            }
                            else
                            {
                                string hexUID = model.gameObject.GetComponent<PrefabUID>().hexUID;
                                if (hexUID != enemyList[selectedEnemy].modelHexUID)
                                {
                                    changesMade = true;
                                    enemyList[selectedEnemy].modelHexUID = hexUID;
                                }
                            }
                            //Debug.Log("modelObj: " + (modelObj == null ? "null" : hexUIDs[0]) + " | model: " + (model == null ? "null" : hexUIDs[1]));

                            EditorGUILayout.Space(2.0f);

                            label.text = "Preview at Level: " + previewLevel;
                            label.tooltip = null;

                            elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                            previewLevel = (ushort)GUI.HorizontalSlider(elementRect, previewLevel, 1, 100);

                            EditorGUILayout.Space(2.0f);

                            ReturnScalingData healthData = ScalingStatFoldoutRegion(RegionToggle.Health, "Health", enemyList[selectedEnemy].baseHealth, enemyList[selectedEnemy].healthScaling);
                            if (healthData.value != enemyList[selectedEnemy].baseHealth || healthData.scaling != enemyList[selectedEnemy].healthScaling)
                            {
                                changesMade = true;
                                enemyList[selectedEnemy].baseHealth = healthData.value;
                                enemyList[selectedEnemy].healthScaling = healthData.scaling;
                            }

                            //EditorGUILayout.Space(2.0f);

                            ReturnScalingData attackData = ScalingStatFoldoutRegion(RegionToggle.Attack, "Attack", enemyList[selectedEnemy].baseAttack, enemyList[selectedEnemy].attackScaling);
                            if (attackData.value != enemyList[selectedEnemy].baseAttack || attackData.scaling != enemyList[selectedEnemy].attackScaling)
                            {
                                changesMade = true;
                                enemyList[selectedEnemy].baseAttack = attackData.value;
                                enemyList[selectedEnemy].attackScaling = attackData.scaling;
                            }

                            //EditorGUILayout.Space(2.0f);

                            ReturnScalingData defenceData = ScalingStatFoldoutRegion(RegionToggle.Defence, "Defence", enemyList[selectedEnemy].baseDefence, enemyList[selectedEnemy].defenceScaling);
                            if (defenceData.value != enemyList[selectedEnemy].baseDefence || defenceData.scaling != enemyList[selectedEnemy].defenceScaling)
                            {
                                changesMade = true;
                                enemyList[selectedEnemy].baseDefence = defenceData.value;
                                enemyList[selectedEnemy].defenceScaling = defenceData.scaling;
                            }

                            //EditorGUILayout.Space(2.0f);
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
                    scrollPosMain = scrollPos;
                }
                EditorGUILayout.GetControlRect(false, 2.0f);
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        if (changesMade)
            ElementDataStorage.SaveCache(enemyList);
    }

    void OnValidate()
    {
        if (focussed)
        {
            enemyList = ElementDataStorage.LoadCache<CombatantData>();
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

    [MenuItem("Window/Game Elements/Enemy Maker")]
    public static void ShowWindow()
    {
        _Window = GetWindow(typeof(Window_Enemies));
        Window.titleContent = new GUIContent("Enemy Maker");
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
                    elementRect.width -= 44;
                    returnData.scaling = (int)GUI.HorizontalSlider(elementRect, valueScaling, 0.0f, 100.0f).Round(0);
                    elementRect.x += elementRect.width + 4;
                    elementRect.width = 40;
                    returnData.scaling = (int)EditorElements.IntPercentField(elementRect, valueScaling, false);

                    EditorGUILayout.Space(2.0f);

                    label.text = "Scaled to Level " + previewLevel;
                    label.tooltip = null;

                    elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                    int scaledValue = CombatValue.ScaledInt(returnData.value, previewLevel, returnData.scaling / 100.0f);
                    EditorElements.ReadonlyField(elementRect, scaledValue.ToString());
                }
                EditorElements.EndSubSection();
            }
            EditorGUILayout.Space(8.0f);
        }

        return returnData;
    }
}
