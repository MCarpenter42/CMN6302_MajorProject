using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.Interpolation;
using NeoCambion.IO;
using NeoCambion.IO.Unity;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Random.Unity;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
#if UNITY_EDITOR
using NeoCambion.Unity.Editor;
#endif
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;

[System.Serializable]
public struct AbilityIcon
{
    public Sprite texNormal;
    public Sprite texHover;
    public Sprite texSelect;

    public bool empty { get { return texNormal == null && texHover == null && texSelect == null; } }

    public AbilityIcon(Sprite tex)
    {
        texNormal = tex;
        texHover = tex;
        texSelect = tex;
    }

    public AbilityIcon(Sprite texNormal, Sprite texSelect)
    {
        this.texNormal = texNormal;
        this.texHover = texSelect;
        this.texSelect = texSelect;
    }

    public AbilityIcon(Sprite texNormal, Sprite texHover, Sprite texSelect)
    {
        this.texNormal = texNormal;
        this.texHover = texHover;
        this.texSelect = texSelect;
    }

    public Sprite Get(IconState state)
    {
        switch (state)
        {
            default:
            case IconState.Normal:
                return texNormal;

            case IconState.Hovered:
                return texHover;

            case IconState.Selected:
                return texSelect;
        }
    }

    public void CopyFrom(AbilityIcon source)
    {
        texNormal = source.texNormal;
        texHover = source.texHover;
        texSelect = source.texSelect;
    }
}

public class AbilityButton : UIObjectInteractable
{
    #region [ OBJECTS / COMPONENTS ]

    public CombatManager CombatManager { get { return LevelManager.Combat; } }
    public HUDManager HUDManager { get { return UIManager.HUD; } }

    public AbilityIcon background;
    public AbilityIcon backgroundDefault;
    public Image backgroundImage;
    
    public AbilityIcon icon;
    public AbilityIcon iconDefault;
    public Image iconImage;

    public UIObject hoverRing;
    public UIObject selRing;

    public Color selColourUsable;
    public Color selColourUnusable;

    #endregion

    #region [ PROPERTIES ]

    public ActionPoolCategory abilityToTrigger;

    public bool isEnabled { get; private set; }
    public bool hovered { get; private set; } = false;
    public bool selected { get; private set; } = false;

    public IconState iconState { get { return selected ? IconState.Selected : (hovered ? IconState.Hovered : IconState.Normal); } }
    private IconState iconStateLast;

    private static Color clrEnabled = Color.white;
    private static Color clrDisabled = new Color(0.6235294f, 0.6235294f, 0.6235294f, 1f);

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Initialise()
    {
        base.Initialise();
        selRing.Show(false);
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void OnEnable()
    {
        isEnabled = true;
        UpdateIconState();
    }

    void OnDisable()
    {
        isEnabled = false;
        hovered = false;
        selected = false;
        UpdateIconState();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ INTERFACE FUNCTIONS ]

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        Hover(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        Hover(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        Select(true);
        HUDManager.OnAbilityButtonSelected(abilityToTrigger);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        Select(false);
        HUDManager.OnAbilityButtonDeselected(abilityToTrigger);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (selected)
        {
            EventSystem.SetSelectedGameObject(null);
            CombatManager.TriggerPlayerAbility(abilityToTrigger);
        }
        else
        {
            EventSystem.SetSelectedGameObject(gameObject);
            CombatManager.StartPlayerTargetSelection(abilityToTrigger);
        }
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public override void OnShow()
    {
        base.OnShow();
    }

    /*public void SetEnabled(bool enable)
    {
        if (enable != isEnabled)
        {
            isEnabled = enable;
            if (enable)
            {

            }
            else
            {
                hovered = false;
                selected = false;
            }
        }
    }*/

    public void Hover(bool hover)
    {
        iconStateLast = iconState;
        hovered = hover;
        UpdateIconState();
    }
    
    public void Select(bool select)
    {
        iconStateLast = iconState;
        selected = select;
        UpdateIconState();
    }

    public void UpdateIconState()
    {
        if (isEnabled)
        {
            backgroundImage.color = clrEnabled;
            iconImage.color = clrEnabled;
            if (iconStateLast != iconState)
            {
                backgroundImage.sprite = background.empty ? backgroundDefault.Get(iconState) : background.Get(iconState);
                iconImage.sprite = icon.empty ? iconDefault.Get(iconState) : icon.Get(iconState);
            }
            hoverRing.Show(hovered);
        }
        else
        {
            backgroundImage.color = clrDisabled;
            iconImage.color = clrDisabled;
            backgroundImage.sprite = background.empty ? backgroundDefault.Get(IconState.Normal) : background.Get(IconState.Normal);
            iconImage.sprite = icon.empty ? iconDefault.Get(IconState.Normal) : icon.Get(IconState.Normal);
            hoverRing.Show(false);
        }
    }

    public void SelRingColour(bool available)
    {
        if (available)
            selRing.ImageColor = selColourUsable;
        else
            selRing.ImageColor = selColourUnusable;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AbilityButton))]
[CanEditMultipleObjects]
public class AbilityButtonEditor : Editor
{
    AbilityButton targ { get { return target as AbilityButton; } }

    Color oldColour;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(4);

        EditorElements.SectionHeader("Background");

        EditorGUILayout.Space(1);

        UpdateBackgroundSprite(EditorElements.SpriteField(new GUIContent("Normal Sprite"), targ.background.texNormal));
        EditorGUILayout.Space(0);
        targ.background.texHover = EditorElements.SpriteField(new GUIContent("Hovered Sprite"), targ.background.texHover);
        EditorGUILayout.Space(0);
        targ.background.texSelect = EditorElements.SpriteField(new GUIContent("Selected Sprite"), targ.background.texSelect);
        targ.backgroundDefault.CopyFrom(targ.background);
        EditorGUILayout.Space(0);
        targ.backgroundImage = EditorGUILayout.ObjectField(new GUIContent("Image Object"), targ.backgroundImage, typeof(Image), true) as Image;
        EditorGUILayout.Space(0);

        EditorGUILayout.Space(4);
        
        EditorElements.SectionHeader("Icon");

        EditorGUILayout.Space(1);

        UpdateIconSprite(EditorElements.SpriteField(new GUIContent("Normal Sprite"), targ.icon.texNormal));
        EditorGUILayout.Space(0);
        targ.icon.texHover = EditorElements.SpriteField(new GUIContent("Hovered Sprite"), targ.icon.texHover);
        EditorGUILayout.Space(0);
        targ.icon.texSelect = EditorElements.SpriteField(new GUIContent("Selected Sprite"), targ.icon.texSelect);
        targ.iconDefault.CopyFrom(targ.icon);
        EditorGUILayout.Space(0);
        targ.iconImage = EditorGUILayout.ObjectField(new GUIContent("Image Object"), targ.iconImage, typeof(Image), true) as Image;
        EditorGUILayout.Space(0);

        EditorGUILayout.Space(4);

        EditorElements.SectionHeader("Selection");

        EditorGUILayout.Space(1);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("selRing"), new GUIContent("Selection Ring"));
        EditorGUILayout.Space(0);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selColourUsable"), new GUIContent("Available Colour"));
        EditorGUILayout.Space(0);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selColourUnusable"), new GUIContent("Unavailable Colour"));
        EditorGUILayout.Space(0);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hoverRing"), new GUIContent("Hover Highlight Ring"));
        EditorGUILayout.Space(0);
        serializedObject.ApplyModifiedProperties();
        UpdateSelRingColour(oldColour);
        oldColour = targ.selColourUsable;
    }

    private void UpdateBackgroundSprite(Sprite sprite)
    {
        if (sprite != targ.background.texNormal)
        {
            targ.background.texNormal = sprite;
            if (targ.backgroundImage != null)
                targ.backgroundImage.sprite = targ.background.texNormal;
        }
    }

    private void UpdateIconSprite(Sprite sprite)
    {
        if (sprite != targ.icon.texNormal)
        {
            targ.icon.texNormal = sprite;
            if (targ.iconImage != null)
                targ.iconImage.sprite = targ.icon.texNormal;
        }
    }

    private void UpdateSelRingColour(Color oldColour)
    {
        if (targ.selColourUsable != oldColour)
        {
            targ.selRing.ImageColor = targ.selColourUsable;
        }
    }
}
#endif
