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

    public float distanceToPlayer { get { return (player.posInteract - transform.position).magnitude; } }
    [HideInInspector] public bool inRange = false;
    [HideInInspector] public bool active = false;

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
        player = GameManager.Instance.playerW;
        if (player != null)
            player.AddInteraction(this);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {

    }

    void OnDestroy()
    {
        if (player != null)
            player.RemoveInteraction(this);
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void Trigger()
    {
        onInteract.Invoke();
    }
}
