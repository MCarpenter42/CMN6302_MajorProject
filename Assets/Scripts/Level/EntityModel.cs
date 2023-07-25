using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.IO;
using NeoCambion.Unity.PersistentUID;
using JetBrains.Annotations;

[RequireComponent(typeof(PrefabUID))]
public class EntityModel : Core
{
    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ PROPERTIES ]

    public static string modelFolder = "Entity Models";
    public static string modelFolderPath { get { return Application.dataPath + "/Resources/" + modelFolder; } }

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {

    }

    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

#if UNITY_EDITOR
    private static void CompileModelList()
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

[CustomEditor(typeof(EntityModel))]
public class EntityModelEditor : Editor
{
    EntityModel targ { get { return target as EntityModel; } }
    Rect rect;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PersistentUID_Utility.AssignPrefabUIDs(EntityModel.modelFolder);
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
