using NeoCambion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : Core
{
    private static GameDataStorage GameData => GameManager.Instance.GameData;

    public ItemRarity rarity;
    public LevelArea.AreaID areaID;
    public Vector3 position { get { return transform.position; } set { transform.position = value; } }
    public int yaw { get { return (int)transform.eulerAngles.y; } set { transform.eulerAngles = new Vector3(0, value, 0); } }
    public Vector3Int rotation { get { return yaw * Vector3Int.up; } set { yaw = value.y; } }

    public void Setup(LevelArea area, Vector3 position, ItemRarity[] rarities = null)
    {
        areaID = area.ID;
        transform.position = position;
        transform.eulerAngles = Random.Range(0f, 360f) * Vector3.up;
        //GetFromPool(rarities);
    }

    /*public void GetFromPool(ItemRarity[] rarities)
    {

    }*/

    private PromptButtonData[] HealOrLevel => new PromptButtonData[]
    {
        new PromptButtonData("HEAL 30%", new Callback[] { DeselAndHide, HealPlayers, Consume }),
        new PromptButtonData("+2 LEVELS", new Callback[] { DeselAndHide, IncPlayerLevel, Consume }),
        new PromptButtonData("CANCEL", new Callback[] { DeselAndHide })
    };

    public void OnInteract() => UIManager.HUD.ShowPrompt("CONSUMABLE ITEM", "You can end your run here with a success, or keep pushing onwards.", HealOrLevel[0], HealOrLevel[1], HealOrLevel[2]);

    private void HealPlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            GameData.playerData[i].currentHealthPercent = Mathf.Clamp(GameData.playerData[i].currentHealthPercent + 0.3f, 0f, 1f);
            GameData.runData.p_healthValues[i][0] = Mathf.RoundToInt(GameData.runData.p_healthValues[i][1] * GameData.playerData[i].currentHealthPercent);
        }
        UIManager.HUD.UpdateWorldHealthBars(true);
    }

    private void IncPlayerLevel() => GameData.runData.p_level += 2;

    private void DeselAndHide() { EventSystem.SetSelectedGameObject(null); UIManager.selHandler.ClearHighlighted(); UIManager.HUD.HidePrompt(); }
    private void Consume() => LevelManager.RemoveItem(this);
}
