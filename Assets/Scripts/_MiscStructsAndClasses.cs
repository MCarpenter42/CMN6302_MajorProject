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

public struct MeshData
{
	public Vector3[] verts;
	public Triangle[] tris;

	public MeshData(Triangle[] tris) : this()
	{
		this.tris = tris;
		this.verts = VertsFromTris();
	}

	public MeshData(Vector3[] verts, Triangle[] tris) : this()
	{
		this.verts = verts;
		this.tris = tris;
	}

	private Vector3[] VertsFromTris()
	{
		List<Vector3> vList = new List<Vector3>();
		foreach (Triangle tri in tris)
		{
			foreach (Vector3 point in tri.points)
			{
				if (!vList.Contains(point))
				{
					vList.Add(point);
				}
			}
		}
		return vList.ToArray();
	}

	public int[] GetMeshTriInds(bool invertTris = false)
	{
		int[] inds = new int[tris.Length * 3];
		for (int i = 0; i < tris.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int indPos = 3 * i + j;
				int jAdj = j == 1 ? 2 : (j == 2 ? 1 : 0);
				inds[indPos] = System.Array.IndexOf(verts, tris[i].points[invertTris ? jAdj : j]);
			}
		}
		return inds;
	}

	public static int[] GetMeshTriInds(Vector3[] points, Triangle[] triangles, bool invertTris = false)
	{
		int[] inds = new int[triangles.Length * 3];
		for (int i = 0; i < triangles.Length; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int indPos = 3 * i + j;
				int jAdj = j == 1 ? 2 : (j == 2 ? 1 : 0);
				inds[indPos] = System.Array.IndexOf(points, triangles[i].points[invertTris ? jAdj : j]);
			}
		}
		return inds;
	}
}