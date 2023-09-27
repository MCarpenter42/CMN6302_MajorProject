using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;

[System.Serializable]
public enum RunStatus { Unknown = -1, Ongoing, Successful, Failed, Abandoned }
public class RunDataActive
{
    #region [ PLAYER ]

    public int p_level;
    private static readonly int pDefault_level = 1;

    public int[][] p_healthValues = new int[4][];
    public float[] p_healthPercentages =>
        new float[4]
        {
            ((float)p_healthValues[0][0] / p_healthValues[0][1]),
            ((float)p_healthValues[1][0] / p_healthValues[1][1]),
            ((float)p_healthValues[2][0] / p_healthValues[2][1]),
            ((float)p_healthValues[3][0] / p_healthValues[3][1])
        };
    public bool[] p_alive =>
        new bool[4]
        {
            p_healthValues[0][0] > 0,
            p_healthValues[1][0] > 0,
            p_healthValues[2][0] > 0,
            p_healthValues[3][0] > 0
        };
    public bool p_allAlive => p_alive[0] && p_alive[1] && p_alive[2] && p_alive[3];

    public int p_ultCharge;
    private static readonly int pDefault_ultCharge = 0;

    #endregion

    #region [ ENEMIES ]

    public int e_level;
    private static readonly int eDefault_level = 3;

    public EnemyCountData e_spawnedInStage;
    private static EnemyCountData eDefault_spawnedInStage => EnemyCountData.None;

    public EnemyCountData e_killedInStage;
    private static EnemyCountData eDefault_killedInStage => EnemyCountData.None;

    #endregion

    #region [ STATS ]

    public RunStatus s_runStatus;
    private static readonly RunStatus sDefault_runStatus = RunStatus.Ongoing;

    public int s_stagesCleared;
    private static readonly int sDefault_stagesCleared = 0;

    public EnemyCountData s_overallKills;
    private static EnemyCountData sDefault_overallKills = EnemyCountData.None;

    public ItemCountData s_itemsUsed;
    private static ItemCountData sDefault_itemsUsed = ItemCountData.None;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static RunDataActive New() =>
    new RunDataActive()
    {
        p_level = pDefault_level,
        p_healthValues = new int[4][]
        {
            new int[2],
            new int[2],
            new int[2],
            new int[2]
        },
        p_ultCharge = pDefault_ultCharge,
        e_level = eDefault_level,
        e_spawnedInStage = eDefault_spawnedInStage,
        e_killedInStage = eDefault_killedInStage,
        s_runStatus = sDefault_runStatus,
        s_stagesCleared = sDefault_stagesCleared,
        s_overallKills = sDefault_overallKills,
        s_itemsUsed = sDefault_itemsUsed
    };

    public void OverrideNew()
    {
        p_level = pDefault_level;
        p_healthValues = new int[4][]
        {
            new int[2],
            new int[2],
            new int[2],
            new int[2]
        };
        p_ultCharge = pDefault_ultCharge;
        e_level = eDefault_level;
        e_spawnedInStage = eDefault_spawnedInStage;
        e_killedInStage = eDefault_killedInStage;
        s_runStatus = sDefault_runStatus;
        s_stagesCleared = sDefault_stagesCleared;
        s_overallKills = sDefault_overallKills;
        s_itemsUsed = sDefault_itemsUsed;
    }

    public void GetFromSave(RunSaveData saveData)
    {
        if (saveData == null)
        {
            OverrideNew();
        }
        else
        {
            p_level = saveData.p_level;
            p_healthValues = saveData.p_healthValues;
            p_ultCharge = saveData.p_ultCharge;
            e_level = saveData.e_level;
            e_spawnedInStage = saveData.e_spawnedInStage;
            e_killedInStage = saveData.e_killedInStage;
            s_runStatus = saveData.s_runStatus;
            s_stagesCleared = saveData.s_stagesCleared;
            s_overallKills = saveData.s_overallKills;
            s_itemsUsed = saveData.s_itemsUsed;
        }
    }

    public void StageCompletionChanges()
    {
        s_stagesCleared++;
        s_overallKills.Add(e_killedInStage);
        e_level += Mathf.RoundToInt(e_spawnedInStage.standard * 1.25f) + e_spawnedInStage.elite * 2 + e_spawnedInStage.boss * 3;
        p_level += e_killedInStage.standard * 1 + e_killedInStage.elite * 3 + e_killedInStage.boss * 5;
        e_spawnedInStage.Clear();
        e_killedInStage.Clear();
    }
}

[System.Serializable]
public class RunSaveData
{
    private const char item = ',';

    public int p_level; // 1 - 1
    public int[][] p_healthValues = new int[4][]; // 8 - 9
    public int p_ultCharge; // 1 - 10

    public int e_level; // 1 - 11
    public EnemyCountData e_spawnedInStage; // 3 - 14
    public EnemyCountData e_killedInStage; // 3 - 17

    public RunStatus s_runStatus; // 1 - 18
    public int s_stagesCleared; // 1 - 19
    public EnemyCountData s_overallKills; // 3 - 22
    public ItemCountData s_itemsUsed; // CURRENTLY NOT IN USE

    public string[] DataStrings => GetDataStrings();

    public RunSaveData(int p_level, int[][] p_healthValues, int p_ultCharge, int e_level, EnemyCountData e_spawnedInStage, EnemyCountData e_killedInStage, RunStatus s_runStatus, int s_stagesCleared, EnemyCountData s_overallKills)
    {
        this.p_level = p_level;
        this.p_healthValues = p_healthValues;
        this.p_ultCharge = p_ultCharge;
        this.e_level = e_level;
        this.e_spawnedInStage = e_spawnedInStage;
        this.e_killedInStage = e_killedInStage;
        this.s_runStatus = s_runStatus;
        this.s_stagesCleared = s_stagesCleared;
        this.s_overallKills = s_overallKills;
    }
    public RunSaveData(RunDataActive source)
    {
        this.p_level = source.p_level;
        this.p_healthValues = source.p_healthValues;
        this.p_ultCharge = source.p_ultCharge;
        this.e_level = source.e_level;
        this.e_spawnedInStage = source.e_spawnedInStage;
        this.e_killedInStage = source.e_killedInStage;
        this.s_runStatus = source.s_runStatus;
        this.s_stagesCleared = source.s_stagesCleared;
        this.s_overallKills = source.s_overallKills;
    }

    public static RunSaveData FromDataStrings(string[] dataStrings)
    {
        if (dataStrings == null || dataStrings.Length == 0)
        {
            return null;
        }
        else
        {
            int[][] vals = new int[][]
        {
            new int[10],
            new int[7],
            new int[5]
        };
            int i, j, n, l, search, delim;
            for (i = 0; i < 3; i++)
            {
                n = vals[i].Length;
                l = dataStrings[i].Length;
                search = 0;
                for (j = 0; j < n && search < l; j++)
                {
                    delim = dataStrings[i].IndexOf(',', search);
                    vals[i][j] = dataStrings[i].RangeToInt(search, delim);
                    search = delim + 1;
                }
            }
            int[][] p_HlV = new int[][]
            {
            new int[] { vals[0][1], vals[0][2] },
            new int[] { vals[0][3], vals[0][4] },
            new int[] { vals[0][5], vals[0][6] },
            new int[] { vals[0][7], vals[0][8] }
            };
            EnemyCountData e_SIS = new EnemyCountData(vals[1][1], vals[1][2], vals[1][3]);
            EnemyCountData e_KIS = new EnemyCountData(vals[1][4], vals[1][5], vals[1][6]);
            EnemyCountData s_OvK = new EnemyCountData(vals[2][2], vals[2][3], vals[2][4]);
            return new RunSaveData(vals[0][0], p_HlV, vals[0][9], vals[1][0], e_SIS, e_KIS, (RunStatus)vals[2][0], vals[2][1], s_OvK);
        }
    }
    public string[] GetDataStrings()
    {
        string strP = "" + p_level + ',';
        for (int i = 0; i < p_healthValues.Length; i++)
        {
            strP += "" + p_healthValues[i][0] + ',' + p_healthValues[i][1] + ',';
        }
        strP += "" + p_ultCharge + ',';

        string strE = "" + e_level + ',';
        strE += "" + e_spawnedInStage.standard + ',' + e_spawnedInStage.elite + ',' + e_spawnedInStage.boss + ',';
        strE += "" + e_killedInStage.standard + ',' + e_killedInStage.elite + ',' + e_killedInStage.boss + ',';

        string strS = "" + (int)s_runStatus + ',' + s_stagesCleared + ',';
        strS += "" + s_overallKills.standard + ',' + s_overallKills.elite + ',' + s_overallKills.boss + ',';
        strS += "0" + ',' + "0" + ',' + "0" + ',' + "0" + ',';

        return new string[] { strP, strE, strS };
    }
}

[System.Serializable]
public class EnemyCountData
{
    public int standard;
    public int elite;
    public int boss;
    public int total { get { return standard + elite + boss; } }

    public EnemyCountData(int standard, int elite, int boss)
    {
        this.standard = standard;
        this.elite = elite;
        this.boss = boss;
    }
    public static EnemyCountData None => new EnemyCountData(0, 0, 0);

    public bool empty { get { return standard == 0 && elite == 0 && boss == 0; } }

    public void Add(EnemyCountData toAdd)
    {
        standard += toAdd.standard;
        elite += toAdd.elite;
        boss += toAdd.boss;
    }
    public void Set(int standard, int elite, int boss)
    {
        this.standard = standard;
        this.elite = elite;
        this.boss = boss;
    }
    public void Clear()
    {
        standard = 0;
        elite = 0;
        boss = 0;
    }
}

[System.Serializable]
public class ItemCountData
{
    public int common;
    public int rare;
    public int epic;
    public int legendary;
    public int total { get { return common + rare + epic + legendary; } }

    public static ItemCountData None => new ItemCountData() { common = 0, rare = 0, epic = 0, legendary = 0 };

    public bool empty { get { return common == 0 && rare == 0 && epic == 0 && legendary == 0; } }
}

public struct StageTransitionData
{

}
