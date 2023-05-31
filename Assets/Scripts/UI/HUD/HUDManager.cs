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
using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;

public class HUDManager : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] UIObject interactHighlight;

    #endregion

    #region [ PROPERTIES ]

    [HideInInspector] public bool interactHLVisible = false;

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        interactHighlight.Show(false);
    }

    void Start()
    {

    }

    void Update()
    {
        if (interactHLVisible != interactHighlight.visible)
            interactHighlight.Show(interactHLVisible);
        if (interactHLVisible)
            UpdateInteractHighlight();
    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private void UpdateInteractHighlight()
    {
        Vector3 objPos = GameManager.Instance.playerW.targetInteract.transform.position;
        interactHighlight.transform.position = GameManager.Instance.cameraW.cam.WorldToScreenPoint(objPos);
    }
}
