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
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;

public class TurnOrderItem : UIObject
{
    #region [ OBJECTS / COMPONENTS ]

    [SerializeField] Image icon;
    [SerializeField] TMP_Text displayName;

    #endregion

    #region [ PROPERTIES ]



    #endregion

    #region [ COROUTINES ]



    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected override void Initialise()
    {
        base.Initialise();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public void SetIcon(Texture2D tex)
    {
        SetIcon(tex, new Rect(0.0f, 0.0f, 1.0f, 1.0f), Vector2.one * 0.5f);
    }
    
    public void SetIcon(Texture2D tex, Rect posInSource)
    {
        SetIcon(tex, posInSource, Vector2.one * 0.5f);
    }
    
    public void SetIcon(Texture2D tex, Rect posInSource, Vector2 relativePivot)
    {
        if (tex == null)
            icon.sprite = null;
        else
            icon.sprite = Sprite.Create(tex, posInSource, relativePivot);
    }

    public void SetName(string text)
    {
        displayName.text = text;
    }


}
