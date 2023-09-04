using UnityEditor;

namespace NeoCambion.Unity
{
    using UnityEngine;
    using UnityEditor;

    public static class EditorMenuItems
    {
        // Creates the submenu at the top of the GameObject menu
        [MenuItem("GameObject/NeoCambion/", false, -50)]

        [MenuItem("GameObject/NeoCambion/Pivot Arm Camera", false, 0)]
        private static void CreatePivotArmCamera()
        {
            Transform selected = Selection.activeTransform;
            GameObject obj = new GameObject("Pivot Arm Camera", typeof(PivotArmCamera));
            obj.transform.SetParent(selected, false);
        }

        [MenuItem("GameObject/NeoCambion/World Space Image", false, 0)]
        private static void CreateWorldSpaceImage()
        {
            Transform selected = Selection.activeTransform;
            GameObject obj = new GameObject("World Space Image", typeof(WorldSpaceImage));
            obj.transform.SetParent(selected, false);
            obj.GetOrAddComponent<SpriteRenderer>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        [MenuItem("GameObject/NeoCambion/World Space Mask", false, 0)]
        private static void CreateWorldSpaceMask()
        {
            Transform selected = Selection.activeTransform;
            GameObject obj = new GameObject("World Space Mask", typeof(WorldSpaceMask));
            obj.transform.SetParent(selected, false);
        }
    }
}