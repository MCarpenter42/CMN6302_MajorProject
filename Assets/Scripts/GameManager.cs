using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;

using NeoCambion;
using NeoCambion.IO;
using NeoCambion.IO.Unity;
using NeoCambion.Maths;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;

[RequireComponent(typeof(ControlsHandler))]
[RequireComponent(typeof(RandTuning))]
[RequireComponent(typeof(ProgressData))]
public class GameManager : Core
{
    public static string prefabPath { get { return AssetDatabase.GetAssetPath(Instance.gameObject); } }

    #region [ OBJECTS / COMPONENTS ]

    private static GameManager instance = null;

    public SceneAttributes SceneAttributes = null;

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
    public GameDataStorage GameDataStorage = new GameDataStorage();

    public UIManager UI { get { return UIManager; } }
    public LevelManager Level { get { return LevelManager; } }

    public WorldPlayer playerW = null;
    public PlayerCam cameraW = null;

    public List<WorldEnemy> enemyListW = new List<WorldEnemy>();

    #endregion

    public TextAsset dataJSON_enemies;
    public TextAsset dataJSON_items;

    #region [ PROPERTIES ]

    #region [ EDITOR DEBUG TOGGLES ]
#if UNITY_EDITOR
    public bool debugOnAwake;
    public bool debugOnStart;
    public bool debugOnUpdate;
    public bool debugOnFixedUpdate;
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

    #region [ STAGE INFORMATION ]



    #endregion

    #endregion

    #region [ CALLBACKS ]

    public UnityEvent callback_debugOnAwake;
    public UnityEvent callback_debugOnStart;
    public UnityEvent callback_debugOnUpdate;
    public UnityEvent callback_debugOnFixedUpdate;

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
                Destroy(gameObject);
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
            OnStartDebug();
#endif
    }

    void Update()
    {


        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#if UNITY_EDITOR
        if (debugOnUpdate)
            OnUpdateDebug();
#endif
    }

    void FixedUpdate()
    {


        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#if UNITY_EDITOR
        if (debugOnFixedUpdate)
            OnFixedUpdateDebug();
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
        GameDataStorage.LoadData();
        // MAKE SURE THINGS PULL FROM THIS AT RUNTIME

        /*float xPos = Camera.main.pixelWidth / 2.0f;
        float yPos = Camera.main.pixelHeight / 2.0f;
        windowCentre = new Vector3(xPos, yPos, 0.0f);*/

        if (EventSystem == null)
            EventSystem = FindObjectOfType<EventSystem>();
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

    public static void SetControlState(ControlState state)
    {
        controlState = state;
        Instance.ControlsHandler.SetControlState(state);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void GetSceneState()
    {
        SceneAttributes = FindObjectOfType<SceneAttributes>();

        if (SceneAttributes == null)
        {
            new GameObject("Scene Attributes", typeof(SceneAttributes));
        }

        if (!SceneAttributes.levelScene)
        {
            controlState = ControlState.Menu;
            cameraW = null;
            playerW = null;
        }
        else if (Level == null)
        {
            controlState = ControlState.None;
            playerW = null;
            cameraW = null;
            Debug.LogError("A LevelManager object must be prewent in any level scene!");
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
        Instance.GameDataStorage.LoadData();
        PrefabUtility.ApplyObjectOverride(Instance, prefabPath, InteractionMode.AutomatedAction);
        AssetDatabase.SaveAssets();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public void StartNewGame()
    {
        GameDataStorage.GetStartingPlayerData();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void OnCombatStart(WorldEnemySet triggerGroup)
    {
        if (GameDataStorage.playerData == null)
            GameDataStorage.GetStartingPlayerData();
        Level.SetAIPause(true);
        Level.CombatTransition(true, 1.0f);
        Level.Combat.StartCombatDelayed(GameDataStorage.playerData, triggerGroup.enemyData, 0.5f);
    }

    public void OnCombatEnd()
    {
        Level.CombatTransition(false, 1.0f);
        c_OnCombatEnd = StartCoroutine(IOnCombatEnd(1.0f));
    }

    private IEnumerator IOnCombatEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        Level.SetAIPause(false);
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

        callback_debugOnAwake.Invoke();
    }

    public void OnStartDebug()
    {
        

        callback_debugOnStart.Invoke();
    }

    public void OnUpdateDebug()
    {
        //ControlsHandler.LogLastDevice();

        callback_debugOnUpdate.Invoke();
    }

    public void OnFixedUpdateDebug()
    {


        callback_debugOnFixedUpdate.Invoke();
    }
#endif
    #endregion
}

[CustomEditor(typeof(GameManager))]
[CanEditMultipleObjects]
public class GameManagerEditor : Editor
{
    #region [ DEBUGGING & TESTING ]

    private TestClass testObj = new TestClass();

    int fontSize = 10;

    // GUIStyle(GUI.skin.label) { fontSize = fontSize }
    // GUIStyle(OSFonts.SystemDefaultMonospaceStyle) { fontSize = fontSize }
    private GUIStyle test_font { get { return new GUIStyle(GUI.skin.label); } }

    private bool useTestMethod = false;
    private List<string> TestPrint()
    {
        int b = 34, c, d;
        List<string> lStr = new List<string>();
        string str;
        for (c = 5; c >= 0; c--)
        {
            d = 2.Pow((ushort)(c));
            str = c + ", " + d + " | " + b.ToString();
            if (b >= d)
            {
                b -= d;
                str += " - " + d + " --> " + b;
            }
            else
            {
                str += " > " + d;
            }
            lStr.Add(str);
        }
        return lStr;
    }

    private List<string> testPrint
    {
        get
        {
            return useTestMethod ? TestPrint() :
            new List<string>() {
                (2.Pow(5)).ToString()
            };
        }
    }

    #endregion

    private GameManager targ { get { return target as GameManager; } }
    private Rect elementRect;
    private GUIContent label = new GUIContent();

    private bool showElementData;
    private bool showRequiredObjects;
    private bool[] objectsFound;
    private bool[] objectsNecessary;
    private bool showDebug;

    public override void OnInspectorGUI()
    {
        GUIStyle[] objectCheckStyles = new GUIStyle[]
        {
            /* Found     | Essential     */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.green),
            /* Not Found | Essential     */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.red),
            /* Found     | Non-Essential */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.lightBlue),
            /* Not Found | Non-Essential */ EditorStylesExtras.LabelStyle(TextAnchor.MiddleLeft, new FontSettings(GUI.skin.label.font, 14, FontStyle.Bold), DynamicTextColour.orange)
        };

        bool[] bools = new bool[5] { false, false, false, false, false };

        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding, GUIStyle.none);
        {
            label.text = "Enemy JSON Data";
            label.tooltip = null;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("dataJSON_enemies"), label);
            
            label.text = "Item JSON Data";
            label.tooltip = null;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("dataJSON_items"), label);

            EditorGUILayout.Space(4.0f);

            label.text = "Game Element Data";
            label.tooltip = null;

            if (showElementData = EditorGUILayout.Foldout(showElementData, label, true, EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10.0f, 0);
                {
                    label.text = "Enemy Entries";
                    label.tooltip = null;

                    elementRect = EditorElements.PrefixLabel(label);
                    EditorGUI.LabelField(elementRect, targ.GameDataStorage.EnemyData.Count.ToString());

                    label.text = "Item Entries";
                    label.tooltip = null;

                    elementRect = EditorElements.PrefixLabel(label);
                    EditorGUI.LabelField(elementRect, targ.GameDataStorage.ItemData.Count.ToString());
                }
                EditorElements.EndSubSection();
            }

            EditorGUILayout.Space(8.0f);

            label.text = "Scene Objects";
            label.tooltip = null;
                 
            EditorElements.SectionHeader(label);
            EditorElements.BeginSubSection(10.0f, 0);
            {
                elementRect = EditorElements.ControlRect();
                elementRect.width = 140;
                elementRect.height += 6;
                if (GUI.Button(elementRect, "Search For Objects"))
                {
                    targ.GetSceneState();
                }

                objectsFound = new bool[] { targ.UI != null, targ.Level != null, targ.cameraW != null, targ.playerW != null };
                objectsNecessary = new bool[] { true, true, (targ.Level == null ? false : targ.SceneAttributes.levelScene), (targ.Level == null ? false : targ.SceneAttributes.levelScene) };

                EditorGUILayout.Space(6.0f);

                string tt, lbl, check = "< " + '\u2713' + " >", cross = "< " + '\u00d7' + " >";
                int styleInd;

                label.text = "UI Manager";
                label.tooltip = null;

                lbl = objectsFound[0] ? check : cross;
                tt = objectsFound[0] ? "Object found!" : "Object not found!\nPlease add an object with a \"UIManager\"\ncomponent to the scene.";
                styleInd = objectsFound[0] ? (objectsNecessary[0] ? 0 : 2) : (objectsNecessary[0] ? 1 : 3);
                elementRect = EditorElements.PrefixLabel(label);
                elementRect.x -= 30;
                elementRect.width = 60;
                EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);

                EditorGUILayout.Space(2.0f);

                label.text = "Level Manager";
                label.tooltip = null;

                lbl = objectsFound[1] ? check : cross;
                tt = objectsFound[1] ? "Object found!" : "Object not found!\nPlease add an object with a \"LevelManager\"\ncomponent to the scene.";
                styleInd = objectsFound[1] ? (objectsNecessary[1] ? 0 : 2) : (objectsNecessary[1] ? 1 : 3);
                elementRect = EditorElements.PrefixLabel(label);
                elementRect.x -= 30;
                elementRect.width = 60;
                EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);

                EditorGUILayout.Space(2.0f);

                label.text = "World Camera";
                label.tooltip = null;

                lbl = objectsFound[2] ? check : cross;
                tt = objectsFound[2] ? "Object found!" : "Object not found!\nPlease add an object with a \"PlayerCam\"\ncomponent to the scene.";
                styleInd = objectsFound[2] ? (objectsNecessary[2] ? 0 : 2) : (objectsNecessary[2] ? 1 : 3);
                elementRect = EditorElements.PrefixLabel(label);
                elementRect.x -= 30;
                elementRect.width = 60;
                EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);

                EditorGUILayout.Space(2.0f);

                label.text = "Player Entity";
                label.tooltip = null;

                lbl = objectsFound[3] ? check : cross;
                tt = objectsFound[3] ? "Object found!" : "Object not found!\nPlease add an object with a \"WorldPlayer\"\ncomponent to the scene.";
                styleInd = objectsFound[3] ? (objectsNecessary[3] ? 0 : 2) : (objectsNecessary[3] ? 1 : 3);
                elementRect = EditorElements.PrefixLabel(label);
                elementRect.x -= 30;
                elementRect.width = 60;
                EditorGUI.LabelField(elementRect, new GUIContent(lbl, tt), objectCheckStyles[styleInd]);
            }
            EditorElements.EndSubSection();

            EditorGUILayout.Space(8.0f);

            label.text = "Debugging";
            label.tooltip = null;
                
            if (showDebug = EditorGUILayout.Foldout(showDebug, label, true, EditorStylesExtras.foldoutLabel))
            {
                EditorElements.BeginSubSection(10.0f, 0);
                {
                    EditorGUILayout.Space(2);

                    float slHeight = EditorGUIUtility.singleLineHeight;
                    Rect rectA = new Rect() { height = slHeight + 2, width = slHeight + 4 }, rectB = new Rect(rectA), lastRect;
                    GUIContent toggleLabel = new GUIContent("Enable Callbacks");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("callback_debugOnAwake"), new GUIContent("Debug Callbacks - Awake"));
                    lastRect = GUILayoutUtility.GetLastRect();
                    rectB.x = (rectA.x = lastRect.x + 4) + rectA.width;
                    rectB.width = lastRect.width - rectA.width;
                    rectB.y = rectA.y = lastRect.y + lastRect.height - slHeight - 2;
                    bools[1] = EditorGUI.Toggle(rectA, targ.debugOnAwake);
                    EditorGUI.LabelField(rectB, toggleLabel);
                    if (bools[1] != targ.debugOnAwake)
                    {
                        targ.debugOnAwake = bools[1];
                        bools[0] = true;
                    }
                    EditorGUILayout.Space(4);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("callback_debugOnStart"), new GUIContent("Debug Callbacks - Start"));
                    lastRect = GUILayoutUtility.GetLastRect();
                    rectB.y = rectA.y = lastRect.y + lastRect.height - slHeight - 2;
                    bools[2] = EditorGUI.Toggle(rectA, targ.debugOnStart);
                    EditorGUI.LabelField(rectB, toggleLabel);
                    if (bools[2] != targ.debugOnStart)
                    {
                        targ.debugOnStart = bools[2];
                        bools[0] = true;
                    }
                    EditorGUILayout.Space(4);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("callback_debugOnUpdate"), new GUIContent("Debug Callbacks - Update"));
                    lastRect = GUILayoutUtility.GetLastRect();
                    rectB.y = rectA.y = lastRect.y + lastRect.height - slHeight - 2;
                    bools[3] = EditorGUI.Toggle(rectA, targ.debugOnUpdate);
                    EditorGUI.LabelField(rectB, toggleLabel);
                    if (bools[3] != targ.debugOnUpdate)
                    {
                        targ.debugOnUpdate = bools[3];
                        bools[0] = true;
                    }
                    EditorGUILayout.Space(4);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("callback_debugOnFixedUpdate"), new GUIContent("Debug Callbacks - FixedUpdate"));
                    lastRect = GUILayoutUtility.GetLastRect();
                    rectB.y = rectA.y = lastRect.y + lastRect.height - slHeight - 2;
                    bools[4] = EditorGUI.Toggle(rectA, targ.debugOnFixedUpdate);
                    EditorGUI.LabelField(rectB, toggleLabel);
                    if (bools[4] != targ.debugOnFixedUpdate)
                    {
                        targ.debugOnFixedUpdate = bools[4];
                        bools[0] = true;
                    }
                }
                EditorElements.EndSubSection();
            }

            if (testPrint.Count > 0)
            {
                EditorGUILayout.Space(12);
                EditorElements.SectionHeader("In-Editor Testing Output", FontStyle.BoldAndItalic);
                EditorGUILayout.Space(4);

                foreach (string str in testPrint)
                    EditorGUI.LabelField(EditorElements.ControlRect(fontSize + 2), str, test_font);
            }
        }
        EditorElements.EndHorizVert();

        bools[0] |= serializedObject.hasModifiedProperties;
        serializedObject.ApplyModifiedProperties();
        if (bools[0])
            PrefabUtility.ApplyPrefabInstance(targ.gameObject, InteractionMode.AutomatedAction);
    }

}

public class TestClass
{
    private enum Var2Bit { Off, Low, High, Max }

    Var2Bit var00 = Var2Bit.Off;
    Var2Bit var01 = Var2Bit.Off;
    Var2Bit var02 = Var2Bit.Off;
    bool var03 = false;
    Var2Bit var04 = Var2Bit.Off;
    Var2Bit var05 = Var2Bit.Off;
    Var2Bit var06 = Var2Bit.Off;
    bool var07 = false;
    Var2Bit var08 = Var2Bit.Off;
    Var2Bit var09 = Var2Bit.Off;
    Var2Bit var10 = Var2Bit.Off;
    Var2Bit var11 = Var2Bit.Off;
    bool var12 = false;
    bool var13 = false;
    Var2Bit var14 = Var2Bit.Off;
    Var2Bit var15 = Var2Bit.Off;
    Var2Bit var16 = Var2Bit.Off;

    private int lShift;
    private ulong Shift(bool var)
    {
        lShift -= 1;
        return (var ? 1ul : 0ul) << lShift;
    }
    private ulong Shift(Var2Bit var)
    {
        lShift -= 2;
        return (ulong)var << lShift;
    }
    public ulong BitShiftLoop(ulong value, int bitStart, int bitEnd, int shift)
    {
        int range = bitEnd - bitStart + 1;

        if (bitStart < 0)
            bitStart = 0;
        else if (bitStart > 62)
            bitStart = 62;
        if (bitEnd > 63)
            bitEnd = 63;
        else if (bitEnd <= bitStart)
            bitEnd = bitStart + 1;
        shift = shift.WrapClamp(1, range - 1);

        ulong a = (1ul << shift) - 1;
        ulong a2 = a << (64 - bitStart - shift);
        ulong a3 = a << (63 - bitEnd);

        ulong b = (1ul << (range - shift)) - 1;
        ulong b2 = b << (63 - bitEnd);
        ulong b3 = b << (64 - bitStart - range - shift);

        ulong c = ((1ul << range) - 1) << (64 - bitStart - range);

        ulong d = ((value & a2) >> (shift - range)) & a3;
        ulong e = ((value & b2) << shift) & b3;

        return (value ^ c) | d | e;
    }
    private char BitsToBase64(ulong value, int bitStart)
    {
        if (bitStart > 58)
            bitStart = 58;
        ulong ind = ((ulong)value >> (58 - bitStart)) & 31ul;
        return Ext_Char.Base64[(int)ind];
    }
    public string[] GetTestStrings()
    {
        int bitCount = 36;
        lShift = bitCount;

        ulong bits = Shift(var00); // -2 --> 34
        bits |= Shift(var01); // -2 --> 32
        bits |= Shift(var02); // -2 --> 31
        bits |= Shift(var03); // -1 --> 29

        bits |= Shift(var04); // -2 --> 27
        bits |= Shift(var05); // -2 --> 25
        bits |= Shift(var06); // -2 --> 23

        bits |= Shift(var07); // -1 --> 22
        bits |= Shift(var08); // -2 --> 20
        bits |= Shift(var09); // -2 --> 18

        bits |= Shift(var10); // -2 --> 16
        bits |= Shift(var11); // -2 --> 14
        bits |= Shift(var12); // -1 --> 13
        bits |= Shift(var13); // -1 --> 12

        bits |= Shift(var14); // -2 --> 10
        bits |= Shift(var15); // -2 --> 8
        bits |= Shift(var16); // -2 --> 6

        string str_A = bits.BitString();
        string str_B = str_A.Substring(str_A.Length / 2);
        str_A = str_A.Substring(0, str_A.Length / 2);

        int startBit = 64 - bitCount;

        List<char> charsA = new List<char>();
        for (int i = startBit; i < 59; i += 6)
        {
            charsA.Add(BitsToBase64(bits, i));
        }
        string strA = new string(charsA.ToArray());

        //int r = Random.Range(0, 1 << lShift);
        int r = 0;
        bits = BitShiftLoop(bits, startBit, bitCount - lShift, r);
        bits |= (uint)r;

        string str_C = bits.BitString();
        string str_D = str_C.Substring(str_C.Length / 2);
        str_C = str_C.Substring(0, str_C.Length / 2);

        List<char> charsB = new List<char>();
        for (int i = startBit; i < 59; i += 6)
        {
            charsB.Add(BitsToBase64(bits, i));
        }
        string strB = new string(charsB.ToArray());

        return new string[]
        {
            str_A, str_B,
            " --- ",
            (strA + " --> " + r + " --> " + strB),
            " --- ",
            str_C, str_D,
        };
    }
}

/*
    public ulong BitShiftLoop(ulong value, int bitStart, int length, int shift)
    {
        shift = shift.WrapClamp(0, length - 1);

        if (shift > 0)
        {
            if (length < 1)
                length = 1;
            else if (length > 64 - bitStart)
                length = 64 - bitStart;
            if (length > 1)
            {
                if (bitStart < 0)
                    bitStart = 0;
                else if (bitStart > 62)
                    bitStart = 62;

                int shiftL = shift, shiftR = length - shift;

                int bitAA1 = bitStart, bitAB1 = bitStart + shift - 1;
                ulong filterA1 = ((1ul << (63 - bitAA1)) - (1ul << (64 - bitAB1)));
                int bitBA1 = bitAB1 + 1, bitBB1 = bitStart + length - 1;
                ulong filterB1 = ((1ul << (63 - bitBA1)) - (1ul << (64 - bitBB1)));

                int bitAA2 = bitAA1 + shiftR, bitAB2 = bitAB1 + shiftR;
                ulong filterA2 = ((1ul << (63 - bitAA2)) - (1ul << (64 - bitAB2)));
                int bitBA2 = bitBA1 - shiftL, bitBB2 = bitBB1 - shiftL;
                ulong filterB2 = ((1ul << (63 - bitBA2)) - (1ul << (64 - bitBB2)));

                ulong filterC = ulong.MaxValue ^ ((1ul << (63 - bitAA1)) - (1ul << (64 - bitBB1)));

                ulong A = ((value & filterA1) >> shiftR);
                ulong B = ((value & filterB1) >> shiftL) & filterB2;
                ulong C = value & filterC;

                return A | B;
            }
        }
        return value;
    }
*/
