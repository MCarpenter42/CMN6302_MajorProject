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

    #region < REGION TOGGLES >

    private bool showPickList = false;
    private bool showHealth = false;

    #endregion

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

                bool _showPickList = (enemyList.Count > 0 && enemyList.InBounds(selectedEnemy)) ? showPickList : true;

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
                        showPickList = enemyList.Count > 0 ? !showPickList : true;
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
                                    showPickList = false;
                                }

                                label.text = "X";
                                label.tooltip = "Delete \"" + enemyList[i].displayName + "\"";

                                elementRect.x += elementRect.width + 4;
                                elementRect.size = bSize;
                                if (GUI.Button(elementRect, label, EditorStylesExtras.textButtonRed))
                                {
                                    enemyList.RemoveAt(i);
                                    selectedEnemy = 0;
                                    showPickList = true;
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
                                showPickList = false;
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
                                showPickList = false;
                                changesMade = true;
                            }

                            /*EditorGUILayout.Space(12.0f);
                            EditorGUILayout.LabelField("Alternatively, retrieve existing data:", centreWrapLabel);

                            elementRect = EditorGUILayout.GetControlRect(true, slHeight + 10);
                            elementRect.x += 40;
                            elementRect.width -= 80;
                            if (GUI.Button(elementRect, "Retrieve Enemy Data"))
                            {
                                enemyList = ElementDataStorage.LoadCache<EnemyData>();
                            }*/
                        }
                    }
                    else
                    {
                        if (selectedEnemy > -1 && enemyList.Count > 0)
                        {
                            EditorGUILayout.Space(4.0f);

                            string dispName = enemyList[selectedEnemy].displayName;

                            label.text = "Display Name";
                            label.tooltip = null;

                            elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                            dispName = EditorGUI.DelayedTextField(elementRect, label, dispName);
                            if (dispName != enemyList[selectedEnemy].displayName)
                            {
                                changesMade = true;
                                enemyList[selectedEnemy].displayName = dispName;
                            }

                            EntityModel mdl = enemyList[selectedEnemy].model;

                            label.text = "Model";
                            label.tooltip = null;

                            elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                            mdl = (EntityModel)EditorGUI.ObjectField(elementRect, label, mdl, typeof(EntityModel), false);
                            if (mdl != enemyList[selectedEnemy].model)
                            {
                                changesMade = true;
                                enemyList[selectedEnemy].model = mdl;
                            }

                            EditorGUILayout.Space(2.0f);

                            label.text = "Preview at Level: " + previewLevel;
                            label.tooltip = null;

                            elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                            previewLevel = (ushort)GUI.HorizontalSlider(elementRect, previewLevel, 1, 100);

                            EditorGUILayout.Space(2.0f);

                            label.text = "Health";
                            label.tooltip = null;

                            if (showHealth = EditorGUILayout.Foldout(showHealth, label, true, EditorStylesExtras.foldoutLabel))
                            {
                                EditorElements.BeginSubSection(10.0f, 0);
                                {
                                    label.text = "Base Value";
                                    label.tooltip = null;

                                    int baseHealth = enemyList[selectedEnemy].baseHealth;
                                    baseHealth = EditorGUILayout.DelayedIntField(label, baseHealth);
                                    if (baseHealth != enemyList[selectedEnemy].baseHealth)
                                    {
                                        changesMade = true;
                                        if (baseHealth < 1)
                                            baseHealth = 1;
                                        enemyList[selectedEnemy].baseHealth = baseHealth;
                                    }

                                    label.text = "Scaling Factor";
                                    label.tooltip = null;

                                    elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                                    elementRect.width -= 44;
                                    int healthScaling = enemyList[selectedEnemy].healthScaling;
                                    healthScaling = (int)GUI.HorizontalSlider(elementRect, healthScaling, 0.0f, 100.0f).Round(0);
                                    elementRect.x += elementRect.width + 4;
                                    elementRect.width = 40;
                                    healthScaling = (int)EditorElements.IntPercentField(elementRect, healthScaling, false);
                                    if (healthScaling != enemyList[selectedEnemy].healthScaling)
                                    {
                                        changesMade = true;
                                        enemyList[selectedEnemy].healthScaling = healthScaling;
                                    }

                                    EditorGUILayout.Space(2.0f);

                                    label.text = "Scaled to Level " + previewLevel;
                                    label.tooltip = null;

                                    elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                                    int scaledHealth = CombatValue.ScaledInt(baseHealth, previewLevel, healthScaling / 100.0f);
                                    EditorElements.ReadonlyField(elementRect, scaledHealth.ToString());
                                }
                                EditorElements.EndSubSection();
                            }
                        }
                        else
                            showPickList = true;
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

}
