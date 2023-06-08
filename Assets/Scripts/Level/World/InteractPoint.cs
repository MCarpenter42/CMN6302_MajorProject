using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.Interpolation;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
//using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;

public class InteractPoint : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] UnityEvent onInteract;
    private WorldPlayer player;

    #endregion

    #region [ PROPERTIES ]

    public float distanceToPlayer { get { return (player.transform.position - transform.position).magnitude; } }
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
