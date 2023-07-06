using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;

public class CombatEnemy : Core
{
    #region [ OBJECTS / COMPONENTS ]

    public EnemyData baseData;
    public EntityModel modelObj;

    #endregion

    #region [ PROPERTIES ]



    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {

    }

    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void GetData(EnemyData data)
    {
        baseData = data;
        //Debug.Log((data == null) + (data == null ? "" : "" + (data.model == null)));
        modelObj = Instantiate(baseData.model, transform);
    }
}
