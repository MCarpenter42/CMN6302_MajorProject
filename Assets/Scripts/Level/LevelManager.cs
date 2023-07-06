using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NeoCambion;

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

        camWorld.enabled = true;
        camCombat.enabled = true;
        if (entering)
            camWorld.enabled = false;
        else
            camCombat.enabled = false;
        GameManager.Instance.UI.HUD.ShowHUD(entering ? ControlState.Combat : ControlState.World);

        yield return new WaitForSeconds(duration * 0.5f);

        GameManager.controlState = entering ? ControlState.Combat : ControlState.World;
    }

    public void INDEV_ExitCombat()
    {
        GameManager.Instance.OnCombatEnd();
    }
}
