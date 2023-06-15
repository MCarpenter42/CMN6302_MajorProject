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
//using NeoCambion.Random;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.IO;

public class LevelManager : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [HideInInspector] public CombatManager Combat = null;
    [SerializeField] Camera camWorld;
    [SerializeField] Camera camCombat;

    #endregion

    #region [ PROPERTIES ]

    

    #endregion

    #region [ COROUTINES ]

    private Coroutine c_CombatTransition;

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        Combat = FindObjectOfType<CombatManager>();
        if (Combat == null)
            Debug.LogError("Combat manager not found!");
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

    private bool inTransition = false;
    public void CombatTransition(bool entering, float duration = 1.0f)
    {
        if (!inTransition)
        {
            if (c_CombatTransition != null)
                StopCoroutine(c_CombatTransition);
            c_CombatTransition = StartCoroutine(ICombatTransition(entering, duration));
        }
    }

    private IEnumerator ICombatTransition(bool entering, float duration)
    {
        GameManager.controlState = ControlState.None;
        Cursor.lockState = CursorLockMode.Locked;
        GameManager.Instance.UI.CombatTransitionOverlay(duration);
        yield return new WaitForSeconds(duration * 0.5f);
        if (entering)
        {
            camCombat.enabled = true;
            camWorld.enabled = false;
            GameManager.Instance.UI.HUD.ShowHUD(ControlState.Combat);
        }
        else
        {
            camWorld.enabled = true;
            camCombat.enabled = true;
            GameManager.Instance.UI.HUD.ShowHUD(ControlState.World);
        }
        yield return new WaitForSeconds(duration * 0.5f);
        if (entering)
        {
            Cursor.lockState = CursorLockMode.None;
            GameManager.controlState = ControlState.Combat;
        }
        else
        {
            GameManager.controlState = ControlState.World;
        }
    }

    public void INDEV_ExitCombat()
    {
        GameManager.Instance.OnCombatEnd();
    }
}
