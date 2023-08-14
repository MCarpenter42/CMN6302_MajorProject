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
using NeoCambion.IO;
using NeoCambion.IO.Unity;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Random.Unity;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;
using NeoCambion.Unity.Interpolation;

#region [ ENUM TYPES ]

public enum ControlState { None, Menu, World, Combat }

#endregion

public class Core : MonoBehaviour
{
    #region [ OBJECTS / COMPONENTS ]



    #endregion

    #region [ PROPERTIES ]

    public const char markdownDelimiter = '%';

    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static void Pause()
    {
        GameManager.Instance.OnPause();
    }

    public static void Resume()
    {
        GameManager.Instance.OnResume();
    }

    public static void TogglePause()
    {
        if (GameManager.allowPauseToggle)
        {
            if (GameManager.gamePaused)
                GameManager.Instance.OnResume();
            else
                GameManager.Instance.OnPause();
        }
    }

    public static void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }
}
