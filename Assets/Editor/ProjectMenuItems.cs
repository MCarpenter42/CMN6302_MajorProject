#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class ProjectMenuItems
{
    private static string CustomButtonBorderSpritePath = "Images/UI/Cut Corners Button/button_cut_corners_border_64x";
    private static Sprite CustomButtonBorderSprite => Resources.Load<Sprite>(CustomButtonBorderSpritePath);
    private static string CustomButtonInnerSpritePath = "Images/UI/Cut Corners Button/button_cut_corners_inner_64x";
    private static Sprite CustomButtonInnerSprite => Resources.Load<Sprite>(CustomButtonInnerSpritePath);
    private static ColorBlock CustomButtonDefaultColors => new ColorBlock
    {
        normalColor =      new Color(0.7264151f, 0.7264151f, 0.7264151f, 1.00f), // #B9B9B9FF
        highlightedColor = new Color(0.8584906f, 0.8584906f, 0.8584906f, 1.00f), // #DBDBDBFF
        pressedColor =     new Color(1.0000000f, 1.0000000f, 1.0000000f, 1.00f), // #FFFFFFFF
        selectedColor =    new Color(0.8584906f, 0.8584906f, 0.8584906f, 1.00f), // #DBDBDBFF
        disabledColor =    new Color(0.7264151f, 0.7264151f, 0.7264151f, 0.50f), // #B9B9B97F
        colorMultiplier = 1f,
        fadeDuration = 0.10f
    };

    [MenuItem("GameObject/Project-Specific/World/Interact Point", false, -100)]
    public static void CreateInteractPoint()
    {
        Transform selTransform = Selection.activeTransform;
        GameObject pointObj = new GameObject("Interact Point", typeof(InteractPoint));
        pointObj.transform.SetParent(selTransform.transform);
    }

    [MenuItem("GameObject/Project-Specific/UI/Custom Button", false, -95)]
    public static void CreateCustomButton()
    {
        Transform selTransform = Selection.activeTransform;
        GameObject newButton = new GameObject("Button", typeof(CanvasRenderer), typeof(Image), typeof(Button));
        newButton.transform.SetParent(selTransform);
        Image bgImage = newButton.GetComponent<Image>();
        bgImage.sprite = CustomButtonBorderSprite;
        bgImage.type = Image.Type.Sliced;
        newButton.GetComponent<Button>().targetGraphic = bgImage;
        Button btn = newButton.GetComponent<Button>();
        btn.colors = CustomButtonDefaultColors;

        GameObject buttonInside = new GameObject("Inner", typeof(CanvasRenderer), typeof(Image));
        buttonInside.GetComponent<Image>().sprite = CustomButtonInnerSprite;
        buttonInside.GetComponent<Image>().type = Image.Type.Sliced;
        RectTransform rTransformInside = buttonInside.GetComponent<RectTransform>();
        rTransformInside.SetParent(newButton.transform);
        rTransformInside.anchorMin = Vector2.zero;
        rTransformInside.anchorMax = Vector2.one;
        rTransformInside.offsetMin = Vector2.zero;
        rTransformInside.offsetMax = Vector2.zero;

        GameObject buttonTextObj = new GameObject("Label", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        TMP_Text buttonText = buttonTextObj.GetComponent<TMP_Text>();
        buttonText.text = "Button";
        buttonText.color = Color.black;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 32f;
        RectTransform rTransformText = buttonTextObj.GetComponent<RectTransform>();
        rTransformText.SetParent(newButton.transform);
        rTransformText.anchorMin = Vector2.zero;
        rTransformText.anchorMax = Vector2.one;
        rTransformText.offsetMin = Vector2.zero;
        rTransformText.offsetMax = Vector2.zero;

        RectTransform rTransform = newButton.GetComponent<RectTransform>();
        rTransform.sizeDelta = new Vector2(240f, 60f);
        rTransform.anchoredPosition = Vector2.zero;
    }
}
#endif
