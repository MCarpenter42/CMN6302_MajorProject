using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.IO;
using NeoCambion.Unity.PersistentUID;
using NeoCambion.Interpolation.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;

[CustomEditor(typeof(EntityModel), true)]
[CanEditMultipleObjects]
public class EntityModelEditor : Editor
{
    EntityModel targ { get { return target as EntityModel; } }
    Rect rect;
    int rangeMin = 1;
    int rangeMax = 4;

    public override void OnInspectorGUI()
    {
        EditorElements.BeginHorizVert(EditorStylesExtras.noMarginsNoPadding, EditorStylesExtras.noMarginsNoPadding);
        {
            targ.size = EditorGUILayout.IntSlider("Size", (targ.size <= rangeMax ? (targ.size >= rangeMin ? targ.size : rangeMin) : rangeMax), rangeMin, rangeMax);
            rect = EditorElements.ControlRect(22);
            rect.x += 50;
            rect.width -= 100;
            if (GUI.Button(rect, new GUIContent("Force Update"), EditorStylesExtras.textButtonRed))
            {
                PersistentUID_Utility.AssignPrefabUIDs(EntityModel.modelFolder);
                EntityModel.CompileModelList();
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("oscillators"), new GUIContent("Oscillators"));
        }
        EditorElements.EndHorizVert();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

[RequireComponent(typeof(PrefabUID))]
public class EntityModel : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] OscillateTransform[] oscillators;

    #endregion

    #region [ PROPERTIES ]

    public static string modelFolder = "Entity Models";
    public static string modelFolderPath { get { return Application.dataPath + "/Resources/" + modelFolder; } }
    public int size = 1;

    #endregion

    #region [ COROUTINES ]



    #endregion

    protected override void Initialise()
    {
        base.Initialise();
        foreach (OscillateTransform osc in oscillators)
        {
            osc.Setup();
        }
    }

    void Update()
    {
        float dT = Time.deltaTime;
        foreach (OscillateTransform osc in oscillators)
        {
            osc.Oscillate(dT);
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

#if UNITY_EDITOR
    public static void CompileModelList()
    {
        string[] modelPaths = PersistentUID_Utility.GetPrefabPathsWithUID(modelFolder);
        List<ModelRef> data = new List<ModelRef>();
        string hexUID;
        for (int i = 0; i < modelPaths.Length; i++)
        {
            //Debug.Log(Resources.Load(modelPaths[i].TrimFileExtension()) == null);
            hexUID = ((GameObject)Resources.Load(modelPaths[i].TrimFileExtension())).GetComponent<PrefabUID>().hexUID;
            data.Add(new ModelRef(hexUID, modelPaths[i].TrimFileExtension()));
        }
        ModelList modelList = new ModelList(data);
        FileHandler.Write(JsonUtility.ToJson(modelList), modelFolderPath + "/_modelList.json");
    }
#endif

    public static List<string[]> GetModelList()
    {
#if UNITY_EDITOR
        CompileModelList();
#endif
        string modelListJson = Resources.Load<TextAsset>(modelFolder + "/_modelList").text;
        ModelList modelList = JsonUtility.FromJson<ModelList>(modelListJson);
        return modelList.GetStrings();
    }

    public static string GetModelPathFromUID(string hexUID)
    {
        List<string[]> modelList = GetModelList();
        if (!(hexUID == null || hexUID == "00000000") && modelList != null && modelList.Count > 0)
        {
            for (int i = 0; i < modelList.Count; i++)
            {
                //Debug.Log(modelList[i][0] + " | " + hexUID);
                if (modelList[i][0] == hexUID)
                    return modelList[i][1];
            }
        }
        return null;
    }
}

[System.Serializable]
public class OscillateTransform
{
    private static float cycleFactor = Mathf.PI * 2f;

    public Transform target;
    [Min(0.01f)]
    public float cyclePeriod = 1f;
    public Vector3 zeroPosition = Vector3.zero;
    public Vector3 maxDisplacement = Vector3.zero;
    [Range(0f, 1f)]
    public float cycleOffset = 0f;

    private Vector3 lerpMin => zeroPosition - maxDisplacement;
    private Vector3 lerpMax => zeroPosition + maxDisplacement;
    private float t, delta;

    public void Setup()
    {
        t = cyclePeriod * cycleOffset;
        target.localPosition = Vector3.Lerp(lerpMin, lerpMax, delta);
    }

    private float GetDelta(float input) => Mathf.Cos(input) / 2f + 0.5f;

    public void Oscillate(float deltaTime)
    {
        t += deltaTime;
        if (t > cyclePeriod)
            t -= cyclePeriod;
        delta = GetDelta(cycleFactor * (t / cyclePeriod));
        target.localPosition = Vector3.Lerp(lerpMin, lerpMax, delta);
    }
}

[System.Serializable]
public struct ModelRef
{
    public string hexUID;
    public string resourcesPath;

    public ModelRef(string hexUID, string resourcesPath)
    {
        this.hexUID = hexUID;
        this.resourcesPath = resourcesPath;
    }
}

[System.Serializable]
public class ModelList
{
    public List<ModelRef> data = new List<ModelRef>();

    public ModelList()
    {

    }

    public ModelList(List<ModelRef> data)
    {
        if (data != null)
            this.data.AddRange(data);
    }

    public List<string[]> GetStrings()
    {
        List<string[]> strings = new List<string[]>();
        foreach(ModelRef mdlRef in data)
        {
            strings.Add(new string[] { mdlRef.hexUID, mdlRef.resourcesPath });
        }
        return strings;
    }
}

/*[CustomPropertyDrawer(typeof(EntityModel))]
public class EntityModelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(position, label, property);
        {
            Rect elementRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (options.Length > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    options[i] = data[i].displayName;
                }
                selected = EditorGUI.IntPopup(elementRect, selected, options, optInds);
                property.FindPropertyRelative("typeInd").intValue = selected;
            }
            else
            {
                EditorGUI.LabelField(elementRect, "No enemy types available!");
                property.FindPropertyRelative("typeInd").intValue = -1;
            }
        }
        EditorGUI.EndProperty();
    }
}*/
