using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;

[System.Serializable]
public enum EnemyClass { Minion = -1, Standard, Elite, Boss }

public class CombatEnemy : CombatantCore
{
    public EnemyClass Class;
}
