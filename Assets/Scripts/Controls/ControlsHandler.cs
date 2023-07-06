using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using NeoCambion;

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
		actions.World.sprint.performed += ActWorld.OnSprint;
		actions.World.sprint.canceled += ActWorld.OnSprint;
        actions.World.cameraTurn.performed += ActWorld.OnCameraTurn;
        actions.World.cameraTurn.canceled += ActWorld.OnCameraTurn;
		actions.World.interact.performed += ActWorld.OnInteract;

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
			if (GameManager.controlState == ControlState.World)
            {
				Vector2 mv = context.ReadValue<Vector2>();
				GameManager.Instance.playerW.velScale = new Vector3(mv.x, 0, mv.y);
			}
        }

		public static void OnSprint(InputAction.CallbackContext context)
		{
			if (GameManager.controlState == ControlState.World)
			{
				GameManager.Instance.playerW.sprintActive = context.ReadValueAsButton();
			}
		}

		public static void OnCameraTurn(InputAction.CallbackContext context)
		{
			if (GameManager.controlState == ControlState.World)
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					Vector2 val = context.ReadValue<Vector2>();
					Vector2 rot = new Vector2(-val.y, val.x) * 0.1f;
					GameManager.Instance.cameraW.SetRotSpeed(rot);
				}
				else
				{
					GameManager.Instance.cameraW.SetRotSpeed(Vector3.zero);
				}
			}
		}

		public static void OnInteract(InputAction.CallbackContext context)
		{
			if (GameManager.controlState == ControlState.World)
			{
				if (GameManager.controlState == ControlState.World && GameManager.Instance.playerW.targetInteract != null)
					GameManager.Instance.playerW.targetInteract.Trigger();
			}
        }
	}

	private class ActCombat
	{
		/*if (GameManager.controlState == ControlState.Combat)
		{

		}*/
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
