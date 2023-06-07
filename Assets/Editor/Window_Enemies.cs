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

    public GameManager gameManager;
    public string gmPrefabPath { get { return AssetDatabase.GetAssetPath(gameManager.gameObject); } }

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

    void OnGUI()
    {
        GUI.enabled = true;
        bool dataModified = false;

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

                label.text = "GameManager Prefab";
                label.tooltip = null;

                gameManager = (GameManager)EditorGUILayout.ObjectField(label, gameManager, typeof(GameManager), false);

                if (gameManager != null)
                {
                    LoadCache();
                    if (enemyList.Count == 0)
                        enemyList = gameManager.ElementDataStorage.Enemies;

                    //EditorGUILayout.Space(8.0f);

                    EditorElements.SeparatorBar();

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

                    bool showPickListTemp = enemyList.Count > 0 ? showPickList : true;

                    label.text = showPickListTemp ? "Select Enemy to Edit" : "Editing: \"" + enemyList[selectedEnemy].displayName + "\"";
                    label.tooltip = null;

                    EditorElements.Header(elementRect, label, 13, TextAnchor.MiddleLeft);

                    label.text = showPickListTemp ? "-" : "+";
                    label.tooltip = (showPickListTemp ? "Hide" : "Show") + " list of\ncreated enemies";

                    btnRect = new Rect(elementRect);
                    btnRect.size = bSize;
                    btnRect.x += elementRect.width + 4;
                    if (GUI.Button(btnRect, label, boldButton))
                    {
                        showPickList = enemyList.Count > 0 ? !showPickList : true;
                    }

                    EditorGUILayout.Space(4.0f);

                    if (showPickListTemp)
                    {
                        EditorGUILayout.BeginHorizontal(enemyListBg);
                    }
                    Vector2 scrollPos = showPickList ? scrollPosList : scrollPosMain;
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
                    {
                        if (showPickListTemp)
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
                                    if (GUI.Button(elementRect, enemyList[i].displayName))
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
                                        dataModified = true;
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
                                    dataModified = true;
                                }
                            }
                            else
                            {
                                EditorGUILayout.Space(4.0f);
                                EditorGUILayout.LabelField("No enemies created yet! Click below to add one", centreWrapLabel);
                                EditorGUILayout.Space(2.0f);
                                elementRect = EditorGUILayout.GetControlRect(true, slHeight);
                                elementRect.x += 40;
                                elementRect.width -= 80;
                                elementRect.height += 10;
                                if (GUI.Button(elementRect, "Create New Enemy"))
                                {
                                    enemyList.Add(new EnemyData("New Enemy"));
                                    selectedEnemy = 0;
                                    showPickList = false;
                                    dataModified = true;
                                }
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
                                if (dispName != enemyList[selectedEnemy].displayName)
                                {
                                    dataModified = true;
                                }
                                enemyList[selectedEnemy].displayName = dispName;
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    if (showPickListTemp)
                    {
                        EditorGUILayout.EndHorizontal();
                        scrollPosList = scrollPos;

                    }
                    else
                    {
                        scrollPosMain = scrollPos;
                    }
                }
                EditorGUILayout.GetControlRect(false, 2.0f);
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        SaveCache(dataModified);
    }

    void OnValidate()
    {
        SaveCache();
    }

    [MenuItem("Window/Game Elements/Enemy Maker")]
    public static void ShowWindow()
    {
        _Window = GetWindow(typeof(Window_Enemies));
        Window.titleContent = new GUIContent("Enemy Maker");
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    string filepath { get { return Application.dataPath + "/Editor/Data/CACHE_EnemyData.json"; } }

    private void SaveCache(bool overwritePrefab = false)
    {
        string jsonString = JsonUtility.ToJson(new EnemyList(enemyList));
        File.WriteAllText(filepath, jsonString);
        if (overwritePrefab)
        {
            GameManager.Instance.ElementDataStorage.Enemies = enemyList;
            PrefabUtility.ApplyObjectOverride(GameManager.Instance.gameObject, gmPrefabPath, InteractionMode.AutomatedAction);
        }
    }

    private void LoadCache()
    {
        if (File.Exists(filepath))
        {
            string jsonString = File.ReadAllText(filepath);
            enemyList = jsonString.Length > 2 ? JsonUtility.FromJson<EnemyList>(jsonString).Enemies : new List<EnemyData>();
        }
        else
        {
            enemyList = new List<EnemyData>();
            File.Create(filepath);
            string jsonString = JsonUtility.ToJson(new EnemyList(enemyList));
            File.WriteAllText(filepath, jsonString);
        }
    }
}
