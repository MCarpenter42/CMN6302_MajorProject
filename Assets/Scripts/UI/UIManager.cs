using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Unity;

public class UIManager : Core
{
    #region [ OBJECTS / COMPONENTS ]

    public HUDManager HUD;
    public GenericMenu menu;

    public UIObject combatOverlay;

    public HashSet<ControlIcon> controlIcons = new HashSet<ControlIcon>();

    public ControllerSelectHandler selHandler = new ControllerSelectHandler();
    public void MoveHighlighted(CompassBearing_Precision0 direction) => selHandler.MoveHighlighted(direction);
    public void PressHighlighted() => selHandler.PressHighlighted();

    public UIObject surveyPrompt;
    public TMP_Text runCodeDisplay;

    #endregion

    #region [ PROPERTIES ]



    #endregion

    #region [ COROUTINES ]

    private Coroutine c_CombatTransitionOverlay = null;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected override void Initialise()
    {
        if (!GameManager.Instance.SceneAttributes.GameplayScene && GameManager.LastRunCode != null)
        {
            runCodeDisplay.text = GameManager.LastRunCode;
            StartCoroutine(ISurveyPrompt());
        }
    }
    private IEnumerator ISurveyPrompt()
    {
        yield return null;
        menu.Show(false);
        yield return null;
        surveyPrompt.Show(true);
    }

    /*public void AddSelectables(UIObject container, List<ControllerSelectWrapper> selectables)
    {
        selHandler.Add(container, selectables);
        nSelectable = selHandler.selectables.Count;
    }

    public void RemoveSelectables(UIObject container)
    {
        selHandler.Remove(container);
        nSelectable = selHandler.selectables.Count;
    }*/

    public void SetSelectables(UIObject container)
    {
        selHandler.SetAvailable(container);
    }
    public void ClearSelectables(UIObject container)
    {
        if (container == selHandler.container)
            selHandler.SetAvailable(null);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Unpause()
    {
		Resume();
    }

    public void UpdateControlIcons(ControlScheme controlScheme, GamepadType gamepadType)
	{
		foreach (ControlIcon icon in controlIcons)
		{
			if (icon != null)
				icon.ControlSchemeUpdate(controlScheme, gamepadType);
		}
	}

    public void SaveRunInfo()
    {
        if (GameManager.Instance.SceneAttributes.GameplayScene)
        {
            LevelManager.SaveData();
            GameDataStorage.Data.SaveRunData();
        }
    }

    public void ToScene_MainMenu() => GameManager.GoToScene(SceneID.MainMenu);
    public void ToScene_Gameplay() => GameManager.GoToScene(SceneID.Gameplay);

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void CombatTransitionOverlay(float duration = 1.0f)
    {
		if (c_CombatTransitionOverlay != null)
			StopCoroutine(c_CombatTransitionOverlay);
		c_CombatTransitionOverlay = StartCoroutine(ICombatTransitionOverlay(duration));

	}
	private IEnumerator ICombatTransitionOverlay(float duration)
    {
        combatOverlay.rTransform.anchorMin = new Vector2(1f, 0f);
        combatOverlay.rTransform.anchorMax = new Vector2(1f, 1f);
        Vector3 pStart = new Vector2(0f, 0.5f), pTarg = new Vector2(1f, 0.5f);
        combatOverlay.rTransform.pivot = pStart;
        combatOverlay.rTransform.anchoredPosition = Vector2.zero;
		combatOverlay.Show(true);

		float t = 0f, d1 = duration * 0.30f, d2 = duration * 0.40f;
		while (t <= d1)
		{
			yield return null;
			t += Time.deltaTime;
            combatOverlay.rTransform.pivot = Vector2.Lerp(pStart, pTarg, t / d1);
            combatOverlay.rTransform.anchoredPosition = Vector2.zero;
        }
        combatOverlay.rTransform.pivot = pTarg;
        combatOverlay.rTransform.anchoredPosition = Vector2.zero;

        yield return new WaitForSeconds(d2);

        combatOverlay.rTransform.anchorMin = new Vector2(0f, 0f);
        combatOverlay.rTransform.anchorMax = new Vector2(0f, 1f);
        pStart = new Vector2(0f, 0.5f); pTarg = new Vector2(1f, 0.5f);
        combatOverlay.rTransform.pivot = pStart;
        combatOverlay.rTransform.anchoredPosition = Vector2.zero;

        t = 0f;
        while (t <= d1)
        {
            yield return null;
            t += Time.deltaTime;
            combatOverlay.rTransform.pivot = Vector2.Lerp(pStart, pTarg, t / d1);
            combatOverlay.rTransform.anchoredPosition = Vector2.zero;
        }
        combatOverlay.rTransform.pivot = pTarg;
        combatOverlay.rTransform.anchoredPosition = Vector2.zero;
        combatOverlay.Show(false);
	}

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void RandSettingsToClipboard() => RandTuning.SettingsString.CopyToClipboard();
    public void OpenSurvey() => GameManager.OpenSurvey();
    public void ClearRunCode() => GameManager.LastRunCode = null;
}

public class ControllerSelectHandler
{
    public UIObject container = null;
    private List<ControllerSelectWrapper> empty = new List<ControllerSelectWrapper>();
    public List<ControllerSelectWrapper> selectables => container == null ? empty : container.selectables;
    public int currentInd = -1;
	public ControllerSelectWrapper current
    {
        get
        {
            if (currentInd < 0 || selectables == null || selectables.Count == 0)
                return ControllerSelectWrapper.Null;
            else
                return selectables[currentInd];
        }
    }

    /*public void Add(UIObject container, List<ControllerSelectWrapper> selectables)
    {
        selectables.AddRange(container, selectables);
		if (current.isNull && selectables.Count > 0)
			SetHighlighted(0);
    }

    public void Remove(UIObject container)
    {
        foreach (ControllerSelectWrapper removed in selectables.ReturnRemove(container))
        {
            removed.HighlightThis(false);
			if (removed.uiObject.highlighted)
			{
                removed.HighlightThis(false);
                current = ControllerSelectWrapper.Null;
            }
        }
    }*/

    public void SetAvailable(UIObject container)
    {
        this.container = container;
        if (container == null || container.selectables.Count == 0)
            ClearHighlighted();
        else
            SetHighlighted(0);
    }

    public void ClearHighlighted()
    {
        if (current.uiObject != null)
            current.HighlightThis(false);
        currentInd = -1;
    }
	public void SetHighlighted(int index)
	{
		if (selectables.InBounds(index))
		{
            if (index != currentInd)
            {
                if (index < 0)
                    ClearHighlighted();
                else
                {
                    current.HighlightThis(false);
                    currentInd = index;
                    current.HighlightThis(true);
                }
            }
            /*ControllerSelectWrapper value = selectables.GetAt(index).Value;
            if (!value.Equals(current))
            {
                if (!current.isNull)
                    current.HighlightThis(false);
                Debug.Log(index + ": " + value.uiObject.gameObject.name + "" + value.selectionPos);
                value.HighlightThis(true);
                current = value;
            }*/
		}
	}
    public void MoveHighlighted(CompassBearing_Precision0 direction)
    {
        if (selectables.Count > 0)
        {
            int? moveTo = direction switch
            {
                CompassBearing_Precision0.North => GetUp(current.selectionPos),
                CompassBearing_Precision0.East => GetRight(current.selectionPos),
                CompassBearing_Precision0.South => GetDown(current.selectionPos),
                CompassBearing_Precision0.West => GetLeft(current.selectionPos),
                _ => null,
            };
            if (moveTo != null)
                SetHighlighted(moveTo.Value);
        }
    }

    private int? GetUp(Vector2Int position)
	{
		List<int> checkInds = new List<int>();
		int n = selectables.Count;
		checkInds.IncrementalPopulate(n - 1, -1, n);

		int? best = null;
		int i, j, bestDist = int.MaxValue;
		Vector2Int disp;
        bool enabled;
		for (i = 0; i <= 2 && checkInds.Count > 0 && best == null; i++)
		{
            for (j = checkInds.Count - 1; j >= 0; j--)
            {
                disp = selectables[checkInds[j]].selectionPos - position;
                enabled = selectables[checkInds[j]].interactable && selectables[checkInds[j]].visible;
                if (enabled && disp.x == i)
                {
                    if (disp.y > 0 && disp.y < bestDist)
                    {
                        bestDist = disp.y;
                        best = checkInds[j];
                    }
                    checkInds.RemoveAt(j);
                }
            }
        }
		return best;
    }
    private int? GetDown(Vector2Int position)
	{
		List<int> checkInds = new List<int>();
		int n = selectables.Count;
		checkInds.IncrementalPopulate(n - 1, -1, n);

		int? best = null;
		int i, j, bestDist = int.MinValue;
		Vector2Int disp;
        bool enabled;
        for (i = 0; i <= 2 && checkInds.Count > 0 && best == null; i++)
        {
            for (j = checkInds.Count - 1; j >= 0; j--)
            {
                disp = selectables[checkInds[j]].selectionPos - position;
                enabled = selectables[checkInds[j]].interactable && selectables[checkInds[j]].visible;
                if (enabled && disp.x == i)
                {
                    if (disp.y < 0 && disp.y > bestDist)
                    {
                        bestDist = disp.y;
                        best = checkInds[j];
                    }
                    checkInds.RemoveAt(j);
                }
            }
        }
		return best;
    }
    private int? GetRight(Vector2Int position)
	{
		List<int> checkInds = new List<int>();
		int n = selectables.Count;
		checkInds.IncrementalPopulate(n - 1, -1, n);

		int? best = null;
		int i, j, bestDist = int.MaxValue;
		Vector2Int disp;
        bool enabled;
        for (i = 0; i <= 2 && checkInds.Count > 0 && best == null; i++)
		{
            for (j = checkInds.Count - 1; j >= 0; j--)
            {
                disp = selectables[checkInds[j]].selectionPos - position;
                enabled = selectables[checkInds[j]].interactable && selectables[checkInds[j]].visible;
                if (enabled && disp.y == i)
                {
                    if (disp.x > 0 && disp.x < bestDist)
                    {
                        bestDist = disp.x;
                        best = checkInds[j];
                    }
                    checkInds.RemoveAt(j);
                }
            }
        }
		return best;
    }
    private int? GetLeft(Vector2Int position)
	{
		List<int> checkInds = new List<int>();
		int n = selectables.Count;
		checkInds.IncrementalPopulate(n - 1, -1, n);

		int? best = null;
		int i, j, bestDist = int.MinValue;
		Vector2Int disp;
        bool enabled;
        for (i = 0; i <= 2 && checkInds.Count > 0 && best == null; i++)
		{
            for (j = checkInds.Count - 1; j >= 0; j--)
            {
                disp = selectables[checkInds[j]].selectionPos - position;
                enabled = selectables[checkInds[j]].interactable && selectables[checkInds[j]].visible;
                if (enabled && disp.y == i)
                {
                    if (disp.x < 0 && disp.x > bestDist)
                    {
                        bestDist = disp.x;
                        best = checkInds[j];
                    }
                    checkInds.RemoveAt(j);
                }
            }
        }
		return best;
    }

	/*private int LinearDist(Vector2Int from, Vector2Int to, CompassBearing_Precision0 direction)
	{
        return direction switch
        {
            CompassBearing_Precision0.North => to.y - from.y,
            CompassBearing_Precision0.South => from.y - to.y,
            CompassBearing_Precision0.East => to.x - from.x,
            CompassBearing_Precision0.West => from.x - to.x,
            _ => int.MaxValue,
        };
    }*/

    public void PressHighlighted()
	{
        if (GameManager.controlState == ControlState.Menu && !current.isNull)
            current.PressThis();
    }
}
