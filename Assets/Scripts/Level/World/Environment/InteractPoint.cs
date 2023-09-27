using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using NeoCambion;

public class InteractPoint : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] UnityEvent onInteract;
    private WorldPlayer player;

    #endregion

    #region [ PROPERTIES ]

    public Vector3 flatPosition { get { return new Vector3(transform.position.x, 0f, transform.position.z); } }
    [HideInInspector] public bool inRange = false;
    [HideInInspector] public bool active = false;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected override void Initialise()
    {
        player = GameManager.Instance.Player;
    }

    public void Trigger()
    {
        onInteract.Invoke();
    }
}
