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
using NeoCambion.Unity.PersistentUID;
using System.Diagnostics.SymbolStore;

[RequireComponent(typeof(ControlsHandler))]
public class GameManager : Core
{
    public static string prefabPath { get { return AssetDatabase.GetAssetPath(Instance.gameObject); } }
    public TextAsset dataJSON;

    #region [ OBJECTS / COMPONENTS ]

    private static GameManager instance = null;

    private ControlsHandler _ControlsHandler = null;
    public ControlsHandler ControlsHandler
    {
        get
        {
            if (_ControlsHandler == null)
            {
                _ControlsHandler = Instance.gameObject.GetComponent<ControlsHandler>();
            }
            return _ControlsHandler;
        }
    }
    public ElementDataStorage ElementDataStorage = new ElementDataStorage();

    public UIManager UI = null;
    public LevelManager Level = null;

    public WorldPlayer playerW = null;
    public PlayerCam cameraW = null;

    public List<WorldEnemy> enemyListW = new List<WorldEnemy>();

    #endregion

    #region [ PROPERTIES ]

    #region [ EDITOR DEBUG TOGGLES ]
#if UNITY_EDITOR
    [SerializeField] bool debugOnAwake = false;
    [SerializeField] bool debugOnStart = false;
    [SerializeField] bool debugOnUpdate = false;
    [SerializeField] bool debugOnFixedUpdate = false;
#endif
    #endregion

    public static bool onGameLoad = true;

    public static bool applicationPlaying
    {
        get
        {
            bool playing = Application.isPlaying;
#if UNITY_EDITOR
            playing = playing || EditorApplication.isPlaying;
#endif
            return playing;
        }
    }

    public static bool gamePaused = false;
    public static bool allowPauseToggle = true;
    private static ControlState _controlState = ControlState.World;
    public static ControlState controlState
    {
        get
        {
            return _controlState;
        }
        set
        {
            switch (value)
            {
                default:
                case ControlState.None:
                    Cursor.lockState = CursorLockMode.None;
                    break;
                case ControlState.Menu:
                    Cursor.lockState = CursorLockMode.None;
                    break;
                case ControlState.World:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case ControlState.Combat:
                    Cursor.lockState = CursorLockMode.None;
                    break;
            }
            _controlState = value;
        }
    }

    public static Vector2 windowCentre { get { return new Vector2(Screen.width, Screen.height) / 2.0f; } }

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_OnCombatEnd = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ SINGLETON CONTROL ]

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameManager inst = FindObjectOfType<GameManager>();
                if (inst == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();

                    instance.Init();

                    // Prevents game manager from being destroyed on loading of a new scene
                    DontDestroyOnLoad(obj);

                    Debug.Log(obj.name);
                }
                instance = inst;
            }
            return instance;
        }
    }

    // Initialiser function, serves a similar purpose to a constructor
    private void Init()
    {
        //Setup();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        if (Application.isPlaying)
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            if (onGameLoad)
            {
                Setup();
            }
            OnAwake();
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#if UNITY_EDITOR
        if (debugOnAwake)
            OnAwakeDebug();
#endif
    }

    void Start()
    {


        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#if UNITY_EDITOR
        if (debugOnStart)
            OnAwakeDebug();
#endif
    }

    void Update()
    {


        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#if UNITY_EDITOR
        if (debugOnUpdate)
            OnAwakeDebug();
#endif
    }

    void FixedUpdate()
    {


        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#if UNITY_EDITOR
        if (debugOnFixedUpdate)
            OnAwakeDebug();
#endif
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Setup()
    {
        onGameLoad = false;
    }

    public void OnAwake()
    {
        ElementDataStorage.LoadData();
        // MAKE SURE THINGS PULL FROM THIS AT RUNTIME

        /*float xPos = Camera.main.pixelWidth / 2.0f;
        float yPos = Camera.main.pixelHeight / 2.0f;
        windowCentre = new Vector3(xPos, yPos, 0.0f);*/

        GetSceneState();

        if (controlState == ControlState.Menu)
        {

        }
        else if (controlState == ControlState.World)
        {
            enemyListW.Clear();
            UI.HUD.ShowHUD(ControlState.World);
        }
        else
        {

        }
    }

    public void OnPause()
    {
        gamePaused = true;
        Time.timeScale = 0.0f;
        if (controlState == ControlState.World)
            Cursor.lockState = CursorLockMode.None;
        Debug.Log("Paused");
    }

    public void OnResume()
    {
        gamePaused = false;
        Time.timeScale = 1.0f;
        if (controlState == ControlState.World)
            Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Resumed");
    }

    public void OnLog()
    {

    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void GetSceneState()
    {
        UI = FindObjectOfType<UIManager>();
        Level = FindObjectOfType<LevelManager>();
        if (Level == null)
        {
            controlState = ControlState.None;
            playerW = null;
            cameraW = null;
        }
        else if (!Level.levelScene)
        {
            controlState = ControlState.Menu;
            cameraW = FindObjectOfType<PlayerCam>();
            playerW = null;
        }
        else
        {
            controlState = ControlState.World;
            cameraW = FindObjectOfType<PlayerCam>();
            playerW = FindObjectOfType<WorldPlayer>();
        }
    }

    public void UpdateElementData()
    {
        //EditorUtility.SetDirty(Instance.gameObject);
        Instance.ElementDataStorage.Enemies = ElementDataStorage.LoadCache<CombatantData>();
        PrefabUtility.ApplyObjectOverride(Instance, prefabPath, InteractionMode.AutomatedAction);
        //PrefabUtility.SavePrefabAsset((GameObject)Resources.Load("Prefabs/GameManager"));
        AssetDatabase.SaveAssets();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void OnCombatStart(WorldEnemy triggerEnemy)
    {
        foreach(WorldEnemy enemy in enemyListW)
        {
            enemy.PauseBehaviour(true);
        }
        Level.CombatTransition(true, 1.0f);
        Level.Combat.StartCombatDelayed(null, triggerEnemy.enemyTypes.AsData(), 0.5f);
    }

    public void OnCombatEnd()
    {
        Level.CombatTransition(false, 1.0f);
        c_OnCombatEnd = StartCoroutine(IOnCombatEnd(1.0f));
    }

    private IEnumerator IOnCombatEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        foreach (WorldEnemy enemy in enemyListW)
        {
            enemy.PauseBehaviour(false);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ EDITOR DEBUG ]
#if UNITY_EDITOR
    public void OnAwakeDebug()
    {
        /*SquareMatrix mat5 = new SquareMatrix(5, new float[] { 1, 3, 4, 1, 2, 5, 4, 5, 3, 1, 4, 1, 2, 3, 4, 3, 2, 5, 4, 2, 5, 3, 2, 1, 5 });
        float[,] mat5inv = mat5.inverse;
        for (int j = 0; j < mat5inv.GetLength(1); j++)
        {
            string output = "";
            for (int i = 0; i < mat5inv.GetLength(0); i++)
            {
                output += mat5inv[i,j] + " ";
            }
            Debug.Log(output);
        }*/
    }

    public void OnStartDebug()
    {

    }

    public void OnUpdateDebug()
    {

    }

    public void OnFixedUpdateDebug()
    {

    }
#endif
    #endregion
}

[CustomEditor(typeof(GameManager))]
[CanEditMultipleObjects]
public class GameManagerEditor : Editor
{
    GameManager targ { get { return target as GameManager; } }
    Rect elementRect;
    GUIContent label = new GUIContent();

    bool showElementData;
    bool showRequiredObjects;
    bool[] objectsFound;
    bool[] objectsNecessary;

    public override void OnInspectorGUI()
    {
        GUIStyle[] objectCheckStyles = new GUIStyle[]
        {
            /* Found     | Essential     */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.green),
            /* Not Found | Essential     */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.red),
            /* Found     | Non-Essential */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.lightBlue),
            /* Not Found | Non-Essential */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.orange)
        };

        EditorGUILayout.BeginHorizontal(EditorStylesExtras.noMarginsNoPadding);
        {
            EditorGUILayout.BeginVertical();
            {
                label.text = "Data JSON File";
                label.tooltip = null;

                elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                targ.dataJSON = EditorGUI.ObjectField(elementRect, targ.dataJSON, typeof(TextAsset), false) as TextAsset;

                EditorGUILayout.Space(4.0f);

                label.text = "Game Element Data";
                label.tooltip = null;

                if (showElementData = EditorGUILayout.Foldout(showElementData, label, true, EditorStylesExtras.foldoutLabel))
                {
                    EditorElements.BeginSubSection(10.0f, 0);
                    {
                        label.text = "Enemy Entries";
                        label.tooltip = null;

                        elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                        EditorGUI.LabelField(elementRect, targ.ElementDataStorage.Enemies.Count.ToString());

                        label.text = "Item Entries";
                        label.tooltip = null;

                        elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                        EditorGUI.LabelField(elementRect, targ.ElementDataStorage.Items.Count.ToString());
                    }
                    EditorElements.EndSubSection();
                }

                EditorGUILayout.Space(8.0f);

                label.text = "Scene Objects";
                label.tooltip = null;

                EditorElements.Header(EditorGUILayout.GetControlRect(), label, 12, TextAnchor.MiddleLeft);
                {
                    EditorElements.BeginSubSection(10.0f, 0);
                    {
                        elementRect = EditorGUILayout.GetControlRect();
                        elementRect.width = 140;
                        elementRect.height += 6;
                        if (GUI.Button(elementRect, "Search For Objects"))
                        {
                            targ.GetSceneState();
                        }

                        objectsFound = new bool[] { targ.UI != null, targ.Level != null, targ.cameraW != null, targ.playerW != null };
                        objectsNecessary = new bool[] { true, true, (targ.Level == null ? false : targ.Level.levelScene), (targ.Level == null ? false : targ.Level.levelScene) };

                        EditorGUILayout.Space(6.0f);

                        string tt, lbl, check = "< " + '\u2713' + " >", cross = "< " + '\u00d7' + " >";
                        int styleInd;

                        label.text = "UI Manager";
                        label.tooltip = null;

                        lbl = objectsFound[0] ? check : cross;
                        tt = objectsFound[0] ? "Object found!" : "Object not found!\nPlease add an object with a \"UIManager\"\ncomponent to the scene.";
                        styleInd = objectsFound[0] ? (objectsNecessary[0] ? 0 : 2) : (objectsNecessary[0] ? 1 : 3);
                        elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                        elementRect.x -= 30;
                        elementRect.width = 60;
                        EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);

                        EditorGUILayout.Space(2.0f);

                        label.text = "Level Manager";
                        label.tooltip = null;

                        lbl = objectsFound[1] ? check : cross;
                        tt = objectsFound[1] ? "Object found!" : "Object not found!\nPlease add an object with a \"LevelManager\"\ncomponent to the scene.";
                        styleInd = objectsFound[1] ? (objectsNecessary[1] ? 0 : 2) : (objectsNecessary[1] ? 1 : 3);
                        elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                        elementRect.x -= 30;
                        elementRect.width = 60;
                        EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);

                        EditorGUILayout.Space(2.0f);

                        label.text = "World Camera";
                        label.tooltip = null;

                        lbl = objectsFound[2] ? check : cross;
                        tt = objectsFound[2] ? "Object found!" : "Object not found!\nPlease add an object with a \"PlayerCam\"\ncomponent to the scene.";
                        styleInd = objectsFound[2] ? (objectsNecessary[2] ? 0 : 2) : (objectsNecessary[2] ? 1 : 3);
                        elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                        elementRect.x -= 30;
                        elementRect.width = 60;
                        EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);

                        EditorGUILayout.Space(2.0f);

                        label.text = "Player Entity";
                        label.tooltip = null;

                        lbl = objectsFound[3] ? check : cross;
                        tt = objectsFound[3] ? "Object found!" : "Object not found!\nPlease add an object with a \"WorldPlayer\"\ncomponent to the scene.";
                        styleInd = objectsFound[3] ? (objectsNecessary[3] ? 0 : 2) : (objectsNecessary[3] ? 1 : 3);
                        elementRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
                        elementRect.x -= 30;
                        elementRect.width = 60;
                        EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);
                    }
                    EditorElements.EndSubSection();
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
}
