using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using NeoCambion;

public enum IconState { Active, Focused, Hover, Normal };

[System.Serializable]
public struct GUIIcon
{
    public string activeName;
    //public Texture2D activeTexture;
    public string focusedName;
    //public Texture2D focusedTexture;
    public string hoverName;
    //public Texture2D hoverTexture;
    public string normalName;
    //public Texture2D normalTexture;

    public readonly bool empty { get { return activeName == null && focusedName == null && hoverName == null && normalName == null; } }

    public GUIIcon(string active, string focused, string hover, string normal)
    {
        activeName = active;
        //activeTexture = null;
        focusedName = focused;
        //focusedTexture = null;
        hoverName = hover;
        //hoverTexture = null;
        normalName = normal;
        //normalTexture = null;
    }

    /*public GUIIcon(Texture2D active, Texture2D focused, Texture2D hover, Texture2D normal)
    {
        activeName = active.name;
        activeTexture = active;
        focusedName = focused.name;
        focusedTexture = focused;
        hoverName = hover.name;
        hoverTexture = hover;
        normalName = normal.name;
        normalTexture = normal;
    }*/

    public string Name(IconState state = IconState.Normal)
    {
        switch (state)
        {
            case IconState.Active:
                return activeName;
            case IconState.Focused:
                return focusedName;
            case IconState.Hover:
                return hoverName;
            default:
            case IconState.Normal:
                return normalName;
        }
    }
}

public class _IconHandler
{
    private static string _IconFolder = null;
    public static string IconFolder
    {
        get
        {
            if (_IconFolder == null || !AssetDatabase.IsValidFolder("Assets" + _IconFolder))
            {
                _IconFolder = GetIconFolder();
            }
            return _IconFolder;
        }
    }

    public static Dictionary<string, GUIIcon> Icons = new Dictionary<string, GUIIcon>()
    {
        {
            "Dropdown Button - Down", new GUIIcon("DropdownButtonDownHover_48x.png", "DropdownButtonDownHover_48x.png", "DropdownButtonDownHover_48x.png", "DropdownButtonDownNormal_48x.png")
        },
        {
            "Dropdown Button - Down (Large)", new GUIIcon("DropdownButtonDownHover_120x.png", "DropdownButtonDownHover_120x.png", "DropdownButtonDownHover_120x.png", "DropdownButtonDownNormal_120x.png")
        },
        {
            "Dropdown Button - Up", new GUIIcon("DropdownButtonUpHover_48x.png", "DropdownButtonUpHover_48x.png", "DropdownButtonUpHover_48x.png", "DropdownButtonUpNormal_48x.png")
        },
        {
            "Dropdown Button - Up (Large)", new GUIIcon("DropdownButtonUpHover_120x.png", "DropdownButtonUpHover_120x.png", "DropdownButtonUpHover_120x.png", "DropdownButtonUpNormal_120x.png")
        }
    };

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static string GetIconFolder()
    {
        string[] guid = AssetDatabase.FindAssets($"t:Script {nameof(_IconHandler)}");
        string path = AssetDatabase.GUIDToAssetPath(guid[0]);
        path = path.Substring(0, path.LastIndexOf("/"));
        if (path.IsNullOrEmpty())
            return "";
        else
            return path.Substring(path.IndexOf("/")) + "/";
    }

    public static Texture2D GetIcon(string iconName, IconState state = IconState.Normal)
    {
        if (iconName.IsNullOrEmpty())
            return null;
        GUIIcon icon;
        Icons.TryGetValue(iconName, out icon);
        if (icon.empty)
            return null;
        return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets" + IconFolder + icon.Name(state));
    }
}