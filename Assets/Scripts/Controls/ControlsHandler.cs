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

public class ControlsHandler : Core
{
	#region [ OBJECTS / COMPONENTS ]

	private InputActions actions;

	#endregion

	#region [ PROPERTIES ]



	#endregion

	#region [ COROUTINES ]



	#endregion

	/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

	void Awake()
	{
		actions = new InputActions();

		actions.Menu.showHide.performed += ActMenu.OnShowHide;

		actions.World.move.performed += ActWorld.OnMove;
		actions.World.move.canceled += ActWorld.OnMove;
        actions.World.cameraTurn.performed += ActWorld.OnCameraTurn;
        actions.World.cameraTurn.canceled += ActWorld.OnCameraTurn;

		actions.InDev.cursorLockToggle.performed += ActInDev.OnCursorLockToggle;
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

	void OnEnable()
    {
        actions.Menu.Enable();
        SetControlState(GameManager.controlState);

        actions.InDev.Enable();
    }

	void OnDisable()
	{
		actions.Menu.Disable();
		actions.World.Disable();
		actions.Combat.Disable();

		actions.InDev.Disable();
	}

	/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	
	public void SetControlState(ControlState state)
    {
        switch (state)
		{
			default:
			case ControlState.Menu:
                actions.World.Disable();
                actions.Combat.Disable();
				break;

			case ControlState.World:
                actions.World.Enable();
                actions.Combat.Disable();
				break;

			case ControlState.Combat:
                actions.World.Disable();
                actions.Combat.Enable();
				break;
        }
    }

	/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

	private class ActMenu
	{
		public static void OnShowHide(InputAction.CallbackContext context)
		{
			TogglePause();
		}
	}

	private class ActWorld
	{
		public static void OnMove(InputAction.CallbackContext context)
		{
			Vector2 mv = context.ReadValue<Vector2>();
            GameManager.Instance.playerW.velScale = new Vector3(mv.x, 0, mv.y);
        }

		public static void OnCameraTurn(InputAction.CallbackContext context)
		{
            if (Cursor.lockState == CursorLockMode.Locked)
			{
                Vector2 val = context.ReadValue<Vector2>();
				Vector2 rot = new Vector2(-val.y, val.x) * 0.1f;
				GameManager.Instance.cameraW.SetRotSpeed(rot);
			}
		}
	}

	private class ActCombat
	{

	}

	private class ActInDev
	{
		public static void OnCursorLockToggle(InputAction.CallbackContext context)
		{
			//Debug.Log("Toggling cursor lock state | " + Time.time);
			if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
			else
				Cursor.lockState = CursorLockMode.Locked;
		}
	}
}
