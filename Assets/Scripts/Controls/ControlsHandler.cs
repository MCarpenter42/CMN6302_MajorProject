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
	}

	void OnDisable()
	{
		actions.Menu.Disable();
	}

	/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

	private class ActMenu
	{
		public static void OnShowHide(InputAction.CallbackContext context)
		{
			TogglePause();
		}
	}
}
