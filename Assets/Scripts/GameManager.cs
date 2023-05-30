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
using UnityEditor.Experimental.Rendering;

[RequireComponent(typeof(ControlsHandler))]
public class GameManager : Core
{
    #region [ OBJECTS / COMPONENTS ]

    private static GameManager instance = null;

    private UIManager _UIManager = null;
    public UIManager UIManager
    {
        get
        {
            if (_UIManager == null)
            {
                _UIManager = FindObjectOfType<UIManager>();
                if (_UIManager == null)
                    throw new System.Exception("ERROR: No UI Manager present in scene!");
            }
            return _UIManager;
        }
    }
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

    public WorldPlayer playerW = null;
    public PlayerCam cameraW = null;

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

    public static Vector2 windowCentre;

    #endregion

    #region [ COROUTINES ]



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
        float xPos = Camera.main.pixelWidth / 2.0f;
        float yPos = Camera.main.pixelHeight / 2.0f;
        windowCentre = new Vector3(xPos, yPos, 0.0f);

        switch (controlState)
        {
            default:
            case ControlState.Menu:
                break;

            case ControlState.World:
                Cursor.lockState = CursorLockMode.Locked;
                playerW = FindObjectOfType<WorldPlayer>();
                cameraW = FindObjectOfType<PlayerCam>();
                break;

            case ControlState.Combat:
                break;
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