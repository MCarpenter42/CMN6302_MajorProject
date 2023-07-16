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

public abstract class Modifiers<T>
{
	public static byte DictBit = 1 << 7;
	private static bool InMultiply(byte key)
	{
		return (key & DictBit) == 0;
	}

    public Dictionary<byte, float> multiply = new Dictionary<byte, float>();
	public Dictionary<byte, T> add = new Dictionary<byte, T>();

	public bool newChanges = false;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public bool Contains(byte key)
	{
		if (InMultiply(key))
		{
			return multiply.ContainsKey(key);
		}
		else
        {
            return add.ContainsKey(key);
        }
	}

	private byte NewKey(bool inAdd)
	{
		if (!inAdd)
			return (byte)(Random.Range(1, 127) ^ DictBit);
		else
			return (byte)(Random.Range(1, 127) | DictBit);
	}

	public byte AddToMultiply(byte key, float value)
	{
		if (key > 0 && !multiply.ContainsKey_Add(key,value))
		{
            multiply.Add(key, value);
            newChanges = true;
            return key;
        }
		return 0;
    }
	
	public byte AddToMultiply(float value)
	{
		if (multiply.Count < 127)
		{
			byte key;
			int tests = 0;
            while (tests < 128)
			{
				key = NewKey(false);
				if (!multiply.ContainsKey_Add(key, value))
                {
					newChanges = true;
                    return key;
                }
            }
        }
		return byte.MinValue;
	}

    public byte AddToAdd(byte key, T value)
    {
        if (key > 0 && !add.ContainsKey_Add(key, value))
        {
            add.Add(key, value);
            newChanges = true;
            return key;
        }
        return 0;
    }

    public byte AddToAdd(T value)
    {
        if (add.Count < 127)
        {
            byte key;
            int tests = 0;
            while (tests < 128)
            {
				key = NewKey(true);
                if (!add.ContainsKey_Add(key, value))
                {
                    newChanges = true;
                    return key;
                }
            }
        }
        return byte.MinValue;
    }

	public bool Remove(byte key)
	{
		if (InMultiply(key))
		{
			if (multiply.ContainsKey_Remove(key))
				return newChanges = true;
		}
		else
        {
            if (add.ContainsKey_Remove(key))
                return newChanges = true;
        }
		return false;
	}

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public virtual T Modify(T baseValue)
	{
		return baseValue;
	}
}

public class ModifiersInt : Modifiers<int>
{
    public override int Modify(int baseValue)
    {
		float mTotal = 1.0f;
		int aTotal = 0;
		if (multiply.Count > 0)
		{
            foreach (KeyValuePair<byte, float> kvp in multiply)
            {
                mTotal += kvp.Value;
            }
        }
		if (add.Count > 0)
		{
            foreach (KeyValuePair<byte, int> kvp in add)
            {
                aTotal += kvp.Value;
            }
        }
        newChanges = false;
        return Mathf.RoundToInt(baseValue * mTotal) + aTotal;
    }
}

public class ModifiersFloat : Modifiers<float>
{
	public override float Modify(float baseValue)
	{
        float mTotal = 1.0f;
        float aTotal = 0;
        if (multiply.Count > 0)
        {
            foreach (KeyValuePair<byte, float> kvp in multiply)
            {
                mTotal += kvp.Value;
            }
        }
        if (add.Count > 0)
        {
            foreach (KeyValuePair<byte, float> kvp in add)
            {
                aTotal += kvp.Value;
            }
        }
		newChanges = false;
        return baseValue * mTotal + aTotal;
    }
}
