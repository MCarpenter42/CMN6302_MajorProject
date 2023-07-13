using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;

public class CombatEnemy : CombatantCore
{
    #region [ OBJECTS / COMPONENTS ]

    public CombatantData baseData;
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

    public void GetData(CombatantData data)
    {
        baseData = data;
        //Debug.Log((data == null) + (data == null ? "" : "" + (data.model == null)));
        modelObj = Instantiate(baseData.model, transform);
    }
}
