using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.IO;

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

    private Vector2 scrollPosMain = new Vector2();
    private Vector2 scrollPosList = new Vector2();

    #region < REGION TOGGLES >

    private bool showPickList = false;

    #endregion

    private int selectedEnemy = -1;

    #region < UTILITY OBJECTS / DATA >

    private GUIContent label = new GUIContent();
    private Rect elementRect;

    List<EnemyData> enemyList;

    #endregion

    private float lastAvWidth;
    private float lastAvHeight;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        enemyList = ElementDataStorage.LoadCache<EnemyData>();
    }

    void OnGUI()
    {
        bool updateGameManager = false;

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
                                    updateGameManager = true;
                                }
                            }

                            EditorGUILayout.Space(2.0f);

                            elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                            elementRect.x += 40;
                            elementRect.width -= 80;
                            elementRect.height += 10;
                            if (GUI.Button(elementRect, "Create New Enemy"))
                            {
                                enemyList.Add(new EnemyData("New Enemy"));
                                selectedEnemy = enemyList.Count - 1;
                                showPickList = false;
                                updateGameManager = true;
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
                                enemyList.Add(new EnemyData("New Enemy"));
                                selectedEnemy = 0;
                                showPickList = false;
                                updateGameManager = true;
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

                            dispName = EditorGUILayout.DelayedTextField(label, dispName);
                            /*if (dispName != enemyList[selectedEnemy].displayName)
                                dataModified = true;*/
                            enemyList[selectedEnemy].displayName = dispName;

                            EntityModel mdl = enemyList[selectedEnemy].model;

                            label.text = "Model";
                            label.tooltip = null;

                            mdl = (EntityModel)EditorGUILayout.ObjectField(label, mdl, typeof(EntityModel), false);
                            /*if (mdl != enemyList[selectedEnemy].model)
                                dataModified = true;*/
                            enemyList[selectedEnemy].model = mdl;
                        }
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

        ElementDataStorage.SaveCache(enemyList);

        if (updateGameManager)
            GameManager.Instance.UpdateElementData();
    }

    void OnValidate()
    {
        enemyList = ElementDataStorage.LoadCache<EnemyData>();
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
