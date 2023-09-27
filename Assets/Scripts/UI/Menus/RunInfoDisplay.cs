using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using NeoCambion.Unity;

public class RunInfoDisplay : UIObject
{
    private static RunDataActive RunData => GameDataStorage.Data.runData;

    public TMP_Text currentStage;
    public TMP_Text playerLevel;
    public TMP_Text enemyLevel;
    public TMP_Text enemiesKilledT;
    public TMP_Text enemiesKilledS;
    public TMP_Text enemiesKilledE;
    public TMP_Text enemiesKilledB;

    public override void OnShow()
    {
        base.OnShow();
        currentStage.text = LevelManager.currentStage.ToString();
        playerLevel.text = RunData.p_level.ToString();
        enemyLevel.text = RunData.e_level.ToString();
        enemiesKilledT.text = RunData.e_killedInStage.total.ToString();
        enemiesKilledS.text = RunData.e_killedInStage.standard.ToString();
        enemiesKilledE.text = RunData.e_killedInStage.elite.ToString();
        enemiesKilledB.text = RunData.e_killedInStage.boss.ToString();
    }
}
