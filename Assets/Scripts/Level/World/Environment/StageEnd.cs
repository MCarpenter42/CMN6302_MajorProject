using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;
using System;

public class StageEnd : Core
{
    private static PromptButtonData[] ContOrComp => new PromptButtonData[]
    {
        new PromptButtonData("CONTINUE", new Callback[] { Deselect, OnContinueSelected }),
        new PromptButtonData("COMPLETE", new Callback[] { Deselect, OnCompleteSelected }),
        new PromptButtonData("CANCEL", new Callback[] { DeselAndHide })
    };
    private static PromptButtonData[] Continue => new PromptButtonData[]
    {
        new PromptButtonData("CONFIRM", new Callback[] { DeselAndHide, ExecuteStageCompletion }),
        new PromptButtonData("", new Callback[0], false),
        (LevelManager.inRestStage ? new PromptButtonData("BACK", new Callback[] { Deselect, OnBack }) : new PromptButtonData("CANCEL", new Callback[] { DeselAndHide }))
    };
    private static PromptButtonData[] Complete => new PromptButtonData[]
    {
        new PromptButtonData("CONFIRM", new Callback[] { DeselAndHide, LevelManager.OnRunWon }),
        new PromptButtonData("", new Callback[0], false),
        new PromptButtonData("BACK", new Callback[] { Deselect, OnBack })
    };
    private static PromptButtonData[] Empty => new PromptButtonData[]
    {
        new PromptButtonData("", new Callback[0]),
        new PromptButtonData("", new Callback[0], false),
        new PromptButtonData("", new Callback[0])
    };

    public void TriggerStageCompletion()
    {
        if (LevelManager.inRestStage)
            UIManager.HUD.ShowPrompt("CONTINUE OR COMPLETE", "You can end your run here with a success, or keep pushing onwards.", ContOrComp[0], ContOrComp[1], ContOrComp[2]);
        else
            UIManager.HUD.ShowPrompt("NEXT STAGE", "Are you sure you want to leave this stage? You can't come back once you do.", Continue[0], Continue[1], Continue[2]);
    }

    private static void OnContinueSelected() => UIManager.HUD.UpdatePrompt("NEXT STAGE", "Are you sure you want to leave this stage? You can't come back once you do.", Continue[0], Continue[1], Continue[2]);
    private static void OnCompleteSelected() => UIManager.HUD.UpdatePrompt("END RUN", "Are you sure you want to complete your run here? You won't be able to continue it later if you do.", Complete[0], Complete[1], Complete[2]);
    private static void OnBack() => UIManager.HUD.UpdatePrompt("CONTINUE OR COMPLETE", "You can end your run here with a success, or keep pushing onwards.", ContOrComp[0], ContOrComp[1], ContOrComp[2]);
    
    private static void Deselect() { EventSystem.SetSelectedGameObject(null); UIManager.selHandler.ClearHighlighted(); }
    private static void DeselAndHide() { EventSystem.SetSelectedGameObject(null); UIManager.selHandler.ClearHighlighted(); UIManager.HUD.HidePrompt(); }
    //private static void HidePrompt() => UIManager.HUD.HidePrompt();

    private static void ExecuteStageCompletion() => LevelManager.OnStageComplete();
}