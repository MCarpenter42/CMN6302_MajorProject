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

public class InDevMeshMerge : Core
{
    GameObject[] templateObjects = new GameObject[0];

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        templateObjects = new GameObject[transform.childCount];
        for (int i = 0; i < templateObjects.Length; i++)
        {
            templateObjects[i] = transform.GetChild(i).gameObject;
        }
        MeshFilter mFil = gameObject.GetOrAddComponent<MeshFilter>();
        mFil.mesh = MergeMeshes();
        //mFil.sharedMesh = MergeMeshes();
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public Mesh MergeMeshes()
    {
        CombineInstance[] combine = new CombineInstance[templateObjects.Length];
        for (int i = 0; i < combine.Length; i++)
        {
            combine[i].mesh = templateObjects[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = templateObjects[i].transform.localToWorldMatrix;
            templateObjects[i].SetActive(false);
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        return mesh;
    }
}
