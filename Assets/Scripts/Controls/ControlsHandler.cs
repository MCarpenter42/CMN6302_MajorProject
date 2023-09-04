using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEditor;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Unity.Editor;

public enum ControlScheme { Other, MouseAndKeyboard, Gamepad }
public enum GamepadType { None, Generic, Xbox, PlayStation, Nintendo }

public class ControlsHandler : Core
{
	#region [ OBJECTS / COMPONENTS ]

	private InputActions actions;

	public struct DeviceInfo
	{
        public string deviceClass;
        public bool empty;
        public string interfaceName;
        public string manufacturer;
        public string product;
		public string valueType;
        public bool native;

		public DeviceInfo(string deviceClass, bool empty, string interfaceName, string manufacturer, string product, string valueType, bool native)
		{
			this.deviceClass = deviceClass;
			this.empty = empty;
			this.interfaceName = interfaceName;
			this.manufacturer = manufacturer;
			this.product = product;
			this.valueType = valueType;
			this.native = native;
		}

		public DeviceInfo(InputDeviceDescription description, string valueType, bool native)
		{

            deviceClass = description.deviceClass;
            empty = description.empty;
            interfaceName = description.interfaceName;
            manufacturer = description.manufacturer;
            product = description.product;
            this.valueType = valueType;
            this.native = native;
        }
	}

	#endregion

	#region [ PROPERTIES ]

	public Dictionary<int, DeviceInfo> devices = new Dictionary<int, DeviceInfo>();
	public KeyValuePair<int, DeviceInfo> lastDevice;

	public ControlScheme activeControlScheme;
	private GamepadType _activeGamepadType;
	public GamepadType activeGamepadType
	{
		get
		{
			return activeControlScheme == ControlScheme.Gamepad ? _activeGamepadType : GamepadType.None;
		}
		set
		{
			if (value == GamepadType.None)
				_activeGamepadType = GamepadType.Generic;
			else
				_activeGamepadType = value;
		}
	}

	#region < Log Toggles >
#if UNITY_EDITOR
	public bool log_deviceClass;
	public bool log_empty;
	public bool log_interfaceName;
	public bool log_manufacturer;
    public bool log_product;
    public bool log_valueType;
    public bool log_native;
#endif
    #endregion

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
		actions.World.minimap.performed += ActWorld.OnMap;

		actions.Combat.basic.performed += ActCombat.OnBasic;
		actions.Combat.skill.performed += ActCombat.OnSkill;
		actions.Combat.ult.performed += ActCombat.OnUlt;
        actions.Combat.use.performed += ActCombat.OnUse;
		actions.Combat.moveSelLeft.performed += ActCombat.OnMoveSelLeft;
		actions.Combat.moveSelRight.performed += ActCombat.OnMoveSelRight;
#if UNITY_EDITOR
		actions.InDev.cursorLockToggle.performed += ActInDev.OnCursorLockToggle;
#endif
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
#if UNITY_EDITOR
		actions.InDev.Enable();
#endif
		InputSystem.onEvent.Where(e => e.HasButtonPress()).Call(eventPtr => GetLastDevice(eventPtr.GetAllButtonPresses()));
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

	public void GetLastDevice(IEnumerable<InputControl> buttonPresses)
	{
		if (Application.isPlaying)
		{
            InputControl last = buttonPresses.ToArray().Last();
            InputDevice device = last.device;
            DeviceInfo info = new DeviceInfo(device.description, device.valueType.Name, device.native);
            if (device.deviceId != lastDevice.Key)
                OnLastDeviceChange(info);
            lastDevice = devices.GetOrAdd(device.deviceId, info);
        }
	}

	public void OnLastDeviceChange(DeviceInfo info)
	{
		ControlScheme controlScheme;
		if (info.native)
		{
			string devClass = info.deviceClass.ToUpper();
			switch (devClass)
			{
				default:
                    controlScheme = ControlScheme.Other;
					break;

				case "MOUSE":
				case "KEYBOARD":
                    controlScheme = ControlScheme.MouseAndKeyboard;
					break;

				case "GAMEPAD":
                    controlScheme = ControlScheme.Gamepad;
					break;
			}
		}
		else
		{
            controlScheme = ControlScheme.Other;
		}
		if (controlScheme != activeControlScheme)
		{
			GameManager.Instance.UI.UpdateControlIcons(controlScheme);
			activeControlScheme = controlScheme;
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
			if (GameManager.controlState == ControlState.World && GameManager.Instance.playerW.targetInteract != null)
				GameManager.Instance.playerW.targetInteract.Trigger();
        }

		public static void OnMap(InputAction.CallbackContext context)
        {
            if (GameManager.controlState == ControlState.World)
                GameManager.Instance.UI.HUD.ToggleMinimap();
        }
	}

	private class ActCombat
	{
		public static void OnBasic(InputAction.CallbackContext context)
		{
			if (GameManager.controlState == ControlState.Combat)
			{
				UIManager.HUD.AbilityButtonPressed(ActionPoolCategory.Standard);
            }
        }

		public static void OnSkill(InputAction.CallbackContext context)
		{
            if (GameManager.controlState == ControlState.Combat)
            {
                UIManager.HUD.AbilityButtonPressed(ActionPoolCategory.Advanced);
            }
        }

		public static void OnUlt(InputAction.CallbackContext context)
		{
            if (GameManager.controlState == ControlState.Combat)
            {
                UIManager.HUD.AbilityButtonPressed(ActionPoolCategory.Special);
            }
        }

		public static void OnUse(InputAction.CallbackContext context)
		{
			if (GameManager.controlState == ControlState.Combat)
            {
                LevelManager.Combat.TriggerPlayerAbility();
            }
        }

		public static void OnMoveSelLeft(InputAction.CallbackContext context)
        {
            if (GameManager.controlState == ControlState.Combat)
            {
				LevelManager.Combat.MoveSelect(false);
            }
        }

		public static void OnMoveSelRight(InputAction.CallbackContext context)
        {
            if (GameManager.controlState == ControlState.Combat)
            {
                LevelManager.Combat.MoveSelect(true);
            }
        }
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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

	public void LogLastDevice()
	{
		bool logAny = false;
		string str = null;
		if (log_deviceClass)
        {
            if (str == null)
                str = "Device Class: " + GameManager.Instance.ControlsHandler.lastDevice.Value.deviceClass;
            else
                str += " | Device Class: " + GameManager.Instance.ControlsHandler.lastDevice.Value.deviceClass;
            logAny = true;
		}
		if (log_empty)
        {
            if (str == null)
                str = "Empty: " + GameManager.Instance.ControlsHandler.lastDevice.Value.empty;
            else
                str += " | Empty: " + GameManager.Instance.ControlsHandler.lastDevice.Value.empty;
            logAny = true;
        }
		if (log_interfaceName)
        {
            if (str == null)
                str = "Interface Name: " + GameManager.Instance.ControlsHandler.lastDevice.Value.interfaceName;
            else
                str += " | Interface Name: " + GameManager.Instance.ControlsHandler.lastDevice.Value.interfaceName;
            logAny = true;
        }
		if (log_manufacturer)
        {
            if (str == null)
                str = "Manufacturer: " + GameManager.Instance.ControlsHandler.lastDevice.Value.manufacturer;
            else
                str += " | Manufacturer: " + GameManager.Instance.ControlsHandler.lastDevice.Value.manufacturer;
            logAny = true;
        }
		if (log_product)
        {
            if (str == null)
                str = "Empty: " + GameManager.Instance.ControlsHandler.lastDevice.Value.product;
            else
                str += " | Empty: " + GameManager.Instance.ControlsHandler.lastDevice.Value.product;
            logAny = true;
        }
		if (log_valueType)
        {
            if (str == null)
                str = "Value Type: " + GameManager.Instance.ControlsHandler.lastDevice.Value.valueType;
            else
                str += " | Value Type: " + GameManager.Instance.ControlsHandler.lastDevice.Value.valueType;
            logAny = true;
        }
		if (log_native)
        {
            if (str == null)
                str = "Is Native: " + GameManager.Instance.ControlsHandler.lastDevice.Value.native;
            else
                str += " | Is Native: " + GameManager.Instance.ControlsHandler.lastDevice.Value.native;
            logAny = true;
        }
		if (logAny)
			Debug.Log(str);
	}
}

[CustomEditor(typeof(ControlsHandler))]
[CanEditMultipleObjects]
public class ControlsHandlerEditor : Editor
{
    ControlsHandler targ { get { return target as ControlsHandler; } }
	Rect elementRect;
	GUIContent label = new GUIContent();

	bool disableAll = false;

	public override void OnInspectorGUI()
	{
		EditorElements.RequiredComponent("Necessary for user input functionality");

		EditorGUILayout.Space(4);
		EditorElements.SeparatorBar();
		EditorGUILayout.Space(4);

		bool[] bools = new bool[8] { false, false, false, false, false, false, false, false };

		EditorGUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(4, 4, 0, 0) });
		{
            EditorElements.SectionHeader("Information Log Toggles");

            EditorGUILayout.Space(6);

            bools[1] = EditorElements.Toggle(disableAll ? false : targ.log_deviceClass, "Device Class");
			if (bools[1] != targ.log_deviceClass)
			{
                targ.log_deviceClass = bools[1];
				bools[0] = true;
            }
            bools[2] = EditorElements.Toggle(disableAll ? false : targ.log_empty, "Is Empty");
            if (bools[2] != targ.log_empty)
            {
                targ.log_empty = bools[2];
                bools[0] = true;
            }
            bools[3] = EditorElements.Toggle(disableAll ? false : targ.log_interfaceName, "Interface Name");
            if (bools[3] != targ.log_interfaceName)
            {
                targ.log_interfaceName = bools[3];
                bools[0] = true;
            }
            bools[4] = EditorElements.Toggle(disableAll ? false : targ.log_manufacturer, "Manufacturer");
            if (bools[4] != targ.log_manufacturer)
            {
                targ.log_manufacturer = bools[4];
                bools[0] = true;
            }
            bools[5] = EditorElements.Toggle(disableAll ? false : targ.log_product, "Product");
            if (bools[5] != targ.log_product)
            {
                targ.log_product = bools[5];
                bools[0] = true;
            }
            bools[6] = EditorElements.Toggle(disableAll ? false : targ.log_valueType, "Value Type");
            if (bools[6] != targ.log_valueType)
            {
                targ.log_valueType = bools[6];
                bools[0] = true;
            }
            bools[7] = EditorElements.Toggle(disableAll ? false : targ.log_native, "Is Native");
            if (bools[7] != targ.log_native)
            {
                targ.log_native = bools[7];
                bools[0] = true;
            }
            EditorGUILayout.Space(4);
            disableAll = EditorElements.Toggle(false, "Disable All");
        }
		EditorGUILayout.EndVertical();

		if (bools[0])
		{
			//PrefabUtility.ApplyObjectOverride(targ.gameObject, AssetDatabase.GetAssetPath(targ.gameObject), InteractionMode.AutomatedAction);
			PrefabUtility.ApplyPrefabInstance(targ.gameObject, InteractionMode.AutomatedAction);
		}
	}
}
