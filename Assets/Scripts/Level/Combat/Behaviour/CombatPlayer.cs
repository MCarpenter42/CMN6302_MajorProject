using NeoCambion.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatPlayer : CombatantCore
{
    public new PlayerData baseData;

    public override void GetData(CombatantData data, int level = 1)
    {
        gotData = data != null;
        if (gotData)
        {
            baseData = data as PlayerData;

            string modelPath = EntityModel.GetModelPathFromUID(data.modelHexUID);
            if (modelPath != null)
            {
                GameObject modelTemplate = Resources.Load<GameObject>(modelPath);
                modelObj = modelTemplate == null ? null : Instantiate(modelTemplate, pivot ?? transform).GetComponent<EntityModel>();
            }

            displayName = data.displayName;

            health = new CombatValue(this, baseData.baseHealth, baseData.healthScaling);
            health.Current = Mathf.RoundToInt(baseData.currentHealthPercent / 100f * health.Scaled);
            attack = new CombatValue(this, baseData.baseAttack, baseData.attackScaling);
            defence = new CombatValue(this, baseData.baseDefence, baseData.defenceScaling);
            speed = new CombatSpeed(this, baseData.speeds);

            attackType = data.attackType;
            weakAgainst = data.weakAgainst;

            brain = new CombatantBrain(this, false, true);
            brain.actions = ActionSet.GetSet(data.actionSet);
            brain.actions.GetPlayerActions(attackType);
        }
        else
            Debug.Log("Empty data object!");
    }
}

[System.Serializable]
public class Equipment
{

}
