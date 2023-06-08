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
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;

[RequireComponent(typeof(ControlsHandler))]
[RequireComponent(typeof(ElementDataStorage))]
public class GameManager : Core
{
    public static string prefabPath { get { return AssetDatabase.GetAssetPath(Instance.gameObject); } }

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
    private ElementDataStorage _ElementDataStorage = null;
    public ElementDataStorage ElementDataStorage
    {
        get
        {
            if (_ElementDataStorage == null)
            {
                _ElementDataStorage = Instance.gameObject.GetComponent<ElementDataStorage>();
            }
            return _ElementDataStorage;
        }
    }

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

    public static bool gamePaused = false;
    public static bool allowPauseToggle = true;
    public static ControlState controlState = ControlState.World;

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
        /*float xPos = Camera.main.pixelWidth / 2.0f;
        float yPos = Camera.main.pixelHeight / 2.0f;
        windowCentre = new Vector3(xPos, yPos, 0.0f);*/

        UI = FindObjectOfType<UIManager>();
        Level = FindObjectOfType<LevelManager>();

        if (controlState == ControlState.Menu)
        {

        }
        else if (controlState == ControlState.World)
        {
            Cursor.lockState = CursorLockMode.Locked;
            playerW = FindObjectOfType<WorldPlayer>();
            cameraW = FindObjectOfType<PlayerCam>();

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
    
    public void UpdateElementData()
    {
        Instance.ElementDataStorage.Enemies = ElementDataStorage.LoadEnemyCache();
        PrefabUtility.ApplyObjectOverride(Instance, prefabPath, InteractionMode.AutomatedAction);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void OnCombatStart(WorldEnemy triggerEnemy)
    {
        foreach(WorldEnemy enemy in enemyListW)
        {
            enemy.PauseBehaviour(true);
        }
        Level.CombatTransition(true, 1.0f);
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