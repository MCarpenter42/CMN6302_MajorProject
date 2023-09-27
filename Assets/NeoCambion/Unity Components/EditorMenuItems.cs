using UnityEditor;

namespace NeoCambion.Unity
{
    using UnityEngine;
    using UnityEditor;
    using TMPro;

#if UNITY_EDITOR
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

        [MenuItem("GameObject/NeoCambion/World Space Image", false, 1)]
        private static void CreateWorldSpaceImage()
        {
            Transform selected = Selection.activeTransform;
            GameObject obj = new GameObject("World Space Image", typeof(WorldSpaceImage));
            obj.transform.SetParent(selected, false);
            obj.GetOrAddComponent<SpriteRenderer>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        [MenuItem("GameObject/NeoCambion/World Space Mask", false, 2)]
        private static void CreateWorldSpaceMask()
        {
            Transform selected = Selection.activeTransform;
            GameObject obj = new GameObject("World Space Mask", typeof(WorldSpaceMask));
            obj.transform.SetParent(selected, false);
        }

        [MenuItem("GameObject/NeoCambion/Shadow Text", false, 3)]
        private static void CreateShadowText()
        {
            Transform selected = Selection.activeTransform;
            GameObject obj = new GameObject("Shadow Text", typeof(ShadowText));
            obj.transform.SetParent(selected, false);
            ShadowText sText = obj.GetComponent<ShadowText>();
            sText.shadowText = new GameObject("Shadow", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            sText.shadowText.transform.SetParent(sText.transform, false);
            sText.mainText = new GameObject("Text", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            sText.mainText.transform.SetParent(sText.transform, false);
            sText.Text = "New Text";
            sText.mainText.text = "New Text";
            sText.shadowText.text = "New Text";
            sText.TextMesh.raycastTarget = false;
        }
    }
#endif
}