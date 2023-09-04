using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

using NeoCambion;

public enum IconState { Normal, Hovered, Selected }
public enum IconResolution { Unknown, x32, x64, x128, x256, x512 };

[System.Serializable]
public struct ControlIconRef
{
    string fileName;
    IconResolution resolution;

    public readonly bool empty { get { return fileName == null && resolution == IconResolution.Unknown; } }

    public ControlIconRef(string fileName = null, IconResolution resolution = IconResolution.Unknown)
    {
        this.fileName = fileName;
        this.resolution = resolution;
    }

    public static ControlIconRef Empty { get { return new ControlIconRef(null, IconResolution.Unknown); } }

    public static ControlIconRef[] Add(ControlIconRef[] array, ControlIconRef newRef)
    {
        ControlIconRef[] arrayOut = new ControlIconRef[array.Length + 1];
        for (int i = 0; i < array.Length; i++)
        {
            arrayOut[i] = array[i];
        }
        arrayOut[arrayOut.Length - 1] = newRef;
        return arrayOut;
    }
}

public class _ControlIconHandler
{
    private static string _IconFolder = null;
    public static string IconFolder
    {
        get
        {
            if (_IconFolder == null || !AssetDatabase.IsValidFolder(_IconFolder))
            {
                _IconFolder = GetIconFolder();
            }
            return _IconFolder;
        }
    }

    public static string IconFolder_KeyboardAndMouse { get { return IconFolder + "/KeyboardAndMouse"; } }
    public static string IconFolder_Gamepad_Generic { get { return IconFolder + "/Gamepad_Generic"; } }
    public static string IconFolder_Gamepad_Xbox { get { return IconFolder + "/Gamepad_Xbox"; } }
    public static string IconFolder_Gamepad_PlayStation { get { return IconFolder + "/Gamepad_PlayStation"; } }
    public static string IconFolder_Gamepad_Nintendo { get { return IconFolder + "/Gamepad_Nintendo"; } }

    public static Dictionary<string, ControlIconRef[]> Icons_KeyboardAndMouse = new Dictionary<string, ControlIconRef[]>()
    {
        
    };
    
    public static Dictionary<string, ControlIconRef[]> Icons_Gamepad_Generic = new Dictionary<string, ControlIconRef[]>()
    {
        
    };
    
    public static Dictionary<string, ControlIconRef[]> Icons_Gamepad_Xbox = new Dictionary<string, ControlIconRef[]>()
    {
        
    };
    
    public static Dictionary<string, ControlIconRef[]> Icons_Gamepad_PlayStation = new Dictionary<string, ControlIconRef[]>()
    {
        
    };
    
    public static Dictionary<string, ControlIconRef[]> Icons_Gamepad_Nintendo = new Dictionary<string, ControlIconRef[]>()
    {
        
    };

    public static Dictionary<string, ControlIconRef[]> GetDict(int ind)
    {
        switch (ind)
        {
            default:
            case 0:
                if (Icons_KeyboardAndMouse.Count == 0)
                    GetControlSchemeIcons(ind);
                return Icons_KeyboardAndMouse;

            case 1:
                if (Icons_Gamepad_Generic.Count == 0)
                    GetControlSchemeIcons(ind);
                return Icons_Gamepad_Generic;

            case 2:
                if (Icons_Gamepad_Xbox.Count == 0)
                    GetControlSchemeIcons(ind);
                return Icons_Gamepad_Xbox;

            case 3:
                if (Icons_Gamepad_PlayStation.Count == 0)
                    GetControlSchemeIcons(ind);
                return Icons_Gamepad_PlayStation;

            case 4:
                if (Icons_Gamepad_Nintendo.Count == 0)
                    GetControlSchemeIcons(ind);
                return Icons_Gamepad_Nintendo;
        }
    }
    
    public static Dictionary<string, ControlIconRef[]> GetDict(string name)
    {
        switch (name.ToLower())
        {
            default:
            case "keyboardandmouse":
                if (Icons_KeyboardAndMouse.Count == 0)
                    GetControlSchemeIcons(0);
                return Icons_KeyboardAndMouse;

            case "gamepad_generic":
                if (Icons_Gamepad_Generic.Count == 0)
                    GetControlSchemeIcons(1);
                return Icons_Gamepad_Generic;

            case "gamepad_xbox":
                if (Icons_Gamepad_Xbox.Count == 0)
                    GetControlSchemeIcons(2);
                return Icons_Gamepad_Xbox;

            case "gamepad_playstation":
                if (Icons_Gamepad_PlayStation.Count == 0)
                    GetControlSchemeIcons(3);
                return Icons_Gamepad_PlayStation;

            case "gamepad_nintendo":
                if (Icons_Gamepad_Nintendo.Count == 0)
                    GetControlSchemeIcons(4);
                return Icons_Gamepad_Nintendo;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static string GetIconFolder()
    {
        string[] guid = AssetDatabase.FindAssets($"t:Script {nameof(_ControlIconHandler)}");
        string path = AssetDatabase.GUIDToAssetPath(guid[0]), check;
        bool halt = false;
        int ind;
        while (!halt)
        {
            ind = path.IndexOf('/');
            if (ind == 0)
            {
                path = path.Substring(1);
                ind = path.IndexOf('/');
            }
            if (ind >= 0)
            {
                check = path.Substring(0, ind);
                if (check == "Assets")
                {
                    halt = true;
                    continue;
                }
                path = path.Substring(ind + 1);
            }
            else
                halt = true;
        }
        if (path[path.Length - 1] == '/')
            path = path.Substring(0, path.Length - 1);
        if (path.IsNullOrEmpty())
            return "";
        else
            return path;
    }

    private static IconResolution GetResolution(string fileName)
    {
        if (fileName.LastIndexOf('.') > 0)
            fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
        string res = null;
        if (fileName.LastIndexOf('_') > 0)
            res = fileName.Substring(fileName.LastIndexOf('_') + 1);
        if (res == null || res.Length == 0)
        {
            return IconResolution.Unknown;
        }
        else
        {
            switch (res.ToLower())
            {
                default:
                case "x32, x64, x128, x256, x512":
                    return IconResolution.Unknown;

                case "32":
                case "x32":
                case "32x":
                case "32x32":
                    return IconResolution.x32;

                case "64":
                case "x64":
                case "64x":
                case "64x64":
                    return IconResolution.x64;

                case "128":
                case "x128":
                case "128x":
                case "128x128":
                    return IconResolution.x128;

                case "256":
                case "x256":
                case "256x":
                case "256x256":
                    return IconResolution.x256;

                case "512":
                case "x512":
                case "512x":
                case "512x512":
                    return IconResolution.x512;
            }
        }
    }

    public static void GetControlSchemeIcons(int ind)
    {
        string[] iconGUIDs;
        string path, fileName, name;
        Dictionary<string, ControlIconRef[]> dict;
        switch (ind)
        {
            default:
                iconGUIDs = new string[0];
                dict = null;
                break;

            case 0:
                iconGUIDs = AssetDatabase.FindAssets("t:Sprite", new string[] { IconFolder_KeyboardAndMouse });
                dict = Icons_KeyboardAndMouse;
                break;

            case 1:
                iconGUIDs = AssetDatabase.FindAssets("t:Sprite", new string[] { IconFolder_Gamepad_Generic });
                dict = Icons_Gamepad_Generic;
                break;

            case 2:
                iconGUIDs = AssetDatabase.FindAssets("t:Sprite", new string[] { IconFolder_Gamepad_Xbox });
                dict = Icons_Gamepad_Xbox;
                break;

            case 3:
                iconGUIDs = AssetDatabase.FindAssets("t:Sprite", new string[] { IconFolder_Gamepad_PlayStation });
                dict = Icons_Gamepad_PlayStation;
                break;

            case 4:
                iconGUIDs = AssetDatabase.FindAssets("t:Sprite", new string[] { IconFolder_Gamepad_Nintendo });
                dict = Icons_Gamepad_Nintendo;
                break;
        }

        if (iconGUIDs.Length > 0 || dict == null)
        {
            foreach (string GUID in iconGUIDs)
            {
                path = AssetDatabase.GUIDToAssetPath(GUID);
                fileName = path.Substring(path.LastIndexOf('/') + 1);
                if (fileName.LastIndexOf('_') > 0)
                    name = fileName.Substring(0, fileName.LastIndexOf('_'));
                else if (fileName.LastIndexOf('.') > 0)
                    name = fileName.Substring(0, fileName.LastIndexOf('.'));
                else
                    name = fileName;
                if (dict.ContainsKey(name))
                {
                    dict[name] = ControlIconRef.Add(dict[name], new ControlIconRef(fileName, GetResolution(fileName)));
                }
                else
                {
                    dict.TryAdd(name, new ControlIconRef[] { new ControlIconRef(fileName, GetResolution(fileName)) });
                }
            }
        }
    }

    public static Sprite GetIcon(string controlName, ControlScheme controlScheme, GamepadType gamepadType = GamepadType.None, int resInd = 0)
    {
        int ctrlSchemeInd;
        if (controlScheme == ControlScheme.Gamepad)
        {
            switch (gamepadType)
            {
                default:
                    ctrlSchemeInd = 1;
                    break;

                case GamepadType.Xbox:
                    ctrlSchemeInd = 2;
                    break;

                case GamepadType.PlayStation:
                    ctrlSchemeInd = 3;
                    break;

                case GamepadType.Nintendo:
                    ctrlSchemeInd = 4;
                    break;
            }
        }
        else
        {
            ctrlSchemeInd = 0;
        }
        Dictionary<string, ControlIconRef[]> dict = GetDict(ctrlSchemeInd);
        if (dict.ContainsKey(controlName))
        {

            return AssetDatabase.LoadAssetAtPath<Sprite>("Assets" + IconFolder);
        }
        else
        {
            return null;
        }
    }
}