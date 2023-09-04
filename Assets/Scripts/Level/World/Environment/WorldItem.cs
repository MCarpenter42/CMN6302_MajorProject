using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : Core
{
    [HideInInspector] public PositionInArea originPoint;

    public void Setup(PositionInArea originPoint, ItemRarity[] rarities)
    {
        this.originPoint = originPoint;
        transform.position = originPoint.worldPosition;
        GetFromPool(rarities);
        LevelManager.AddItem(this);
    }

    public void GetFromPool(ItemRarity[] rarities)
    {

    }
}
