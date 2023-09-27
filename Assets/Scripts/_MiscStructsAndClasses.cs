using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
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

public struct MeshData
{
	public Vector3[] verts;
    public int[] triInds;
	public Triangle[] tris;

	public MeshData(Triangle[] tris) : this()
	{
		this.tris = tris;
		this.verts = VertsFromTris();
        this.triInds = GetMeshTriInds();
	}

	public MeshData(Vector3[] verts, int[] triInds, bool useTris = true) : this()
	{
		this.verts = verts;
        this.triInds = triInds;
        if (useTris)
            tris = GetTris();
        else
            tris = null;
    }
    
	public MeshData(Vector3[] verts, Triangle[] tris, bool useTriInds = true) : this()
	{
		this.verts = verts;
		this.tris = tris;
        if (useTriInds)
            triInds = GetMeshTriInds();
        else
            triInds = null;
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

    public Triangle[] GetTris()
    {
        int n = triInds.Length <= verts.Length * 3 ? triInds.Length - (triInds.Length % 3) : verts.Length * 3;
        Triangle[] tris = new Triangle[n];
        for (int i = 0; i < n; i++)
        {
            tris[i] = new Triangle(verts[triInds[3 * n]], verts[triInds[3 * n + 1]], verts[triInds[3 * n + 2]]);
        }
        return tris;
    }

    public static Triangle[] GetTris(Vector3[] verts, int[] triInds)
    {
        int n = triInds.Length - (triInds.Length % 3);
        Triangle[] tris = new Triangle[n];
        for (int i = 0; i < n; i++)
        {
            tris[i] = new Triangle(verts[triInds[3 * n]], verts[triInds[3 * n + 1]], verts[triInds[3 * n + 2]]);
        }
        return tris;
    }
}

// GENERIC NON-ABSTRACT DEPRECATED
// For the generic version to be non-abstract, the "dynamic" data type must be used.
// Unfortunately, due to platform support issues, "dynamic" is not supported in Unity.
/// <summary>
/// A collection of modifiers that can be applied to a numerical value.
/// </summary>
/// <typeparam name="T">
/// Requires a numerical type that implements the same interfaces as all numerical types in System.
/// </typeparam>
public abstract class Modifiers<T> where T : System.IComparable, System.IComparable<T>, System.IConvertible, System.IEquatable<T>, System.IFormattable
{
    private static System.Type[] integralTypes = new System.Type[]
    {
        typeof(byte),
        typeof(int),
        typeof(nint),
        typeof(nuint),
        typeof(long),
        typeof(short),
        typeof(sbyte),
        typeof(uint),
        typeof(ulong),
        typeof(ushort),
    };

	public static byte DictBit = 1 << 7;
	private static bool InMultiply(byte key)
	{
		return (key & DictBit) == 0;
	}

	public Dictionary<byte, float> multiply = new Dictionary<byte, float>();
	public Dictionary<byte, T> add = new Dictionary<byte, T>();

	public bool newChanges = false;

	public Callback onChanges = null;
	public Callback onAdd = null;
	public Callback onRemove = null;

	/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

	public Modifiers(Callback onAdd, Callback onRemove)
	{
		this.onChanges = null;
		this.onAdd = onAdd;
		this.onRemove = onRemove;
	}
	
	public Modifiers(Callback onChanges = null, Callback onAdd = null, Callback onRemove = null)
	{
		this.onChanges = onChanges;
		this.onAdd = onAdd;
		this.onRemove = onRemove;
	}

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

	public byte AddToMultiply(byte key, float value, bool suppressCallback = false)
	{
		if (key > 0 && multiply.TryAdd(key, value))
		{
            newChanges = true;
            if (!suppressCallback)
            {
                Ext_Callback.InvokeIfValid(onChanges);
                Ext_Callback.InvokeIfValid(onAdd);
            }
            return key;
        }
        return 0;
    }
	
	public byte AddToMultiply(float value, bool suppressCallback = false)
	{
		if (multiply.Count < 127)
		{
			byte key;
			int tests = 0;
            while (tests < 128)
			{
				key = NewKey(false);
				if (multiply.TryAdd(key, value))
                {
					newChanges = true;
                    return key;
                }
            }
        }
        if (!suppressCallback)
        {
            Ext_Callback.InvokeIfValid(onChanges);
            Ext_Callback.InvokeIfValid(onAdd);
        }
        return byte.MinValue;
	}

    public byte AddToAdd(byte key, T value, bool suppressCallback = false)
    {
        if (key > 0 && add.TryAdd(key, value))
        {
            newChanges = true;
            if (!suppressCallback)
            {
                Ext_Callback.InvokeIfValid(onChanges);
                Ext_Callback.InvokeIfValid(onAdd);
            }
            return key;
        }
        return 0;
    }

    public byte AddToAdd(T value, bool suppressCallback = false)
    {
        if (add.Count < 127)
        {
            byte key;
            int tests = 0;
            while (tests < 128)
            {
				key = NewKey(true);
                if (add.TryAdd(key, value))
                {
                    newChanges = true;
                    return key;
                }
            }
        }
        if (!suppressCallback)
        {
            Ext_Callback.InvokeIfValid(onChanges);
            Ext_Callback.InvokeIfValid(onAdd);
        }
        return byte.MinValue;
    }

	public bool Remove(byte key, bool suppressCallback = false)
	{
		if (InMultiply(key))
		{
			if (multiply.TryRemove(key))
				return newChanges = true;
		}
		else
        {
            if (add.TryRemove(key))
                return newChanges = true;
        }
        if (!suppressCallback)
        {
            Ext_Callback.InvokeIfValid(onChanges);
            Ext_Callback.InvokeIfValid(onRemove);
        }
        return false;
	}

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public virtual T Modify(T baseValue)
    {
        return baseValue;
    }

    /*public virtual T Modify(T baseValue)
    {
        float mTotal = 1.0f;
        T aTotal = default;
        if (multiply.Count > 0)
        {
            foreach (KeyValuePair<byte, float> kvp in multiply)
            {
                mTotal += kvp.Value;
            }
        }
        if (add.Count > 0)
        {
            foreach (KeyValuePair<byte, T> kvp in add)
            {
                try
                {
                    dynamic dA = aTotal;
                    dynamic dB = kvp.Value;
                    aTotal = dA + dB;
                }
                catch
                { }
            }
        }
        newChanges = false;
        dynamic dynBase = baseValue;
        if (integralTypes.Contains(typeof(T)))
            return (T)(Mathf.RoundToInt(dynBase * mTotal) + aTotal);
        else
            return (T)(dynBase * mTotal + aTotal);
    }*/
}

public class ModifiersInt : Modifiers<int>
{
    public ModifiersInt(Callback onAdd, Callback onRemove)
    {
        this.onChanges = null;
        this.onAdd = onAdd;
        this.onRemove = onRemove;
    }

    public ModifiersInt(Callback onChanges = null, Callback onAdd = null, Callback onRemove = null)
    {
        this.onChanges = onChanges;
        this.onAdd = onAdd;
        this.onRemove = onRemove;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

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
    public ModifiersFloat(Callback onAdd, Callback onRemove)
    {
        this.onChanges = null;
        this.onAdd = onAdd;
        this.onRemove = onRemove;
    }

    public ModifiersFloat(Callback onChanges = null, Callback onAdd = null, Callback onRemove = null)
    {
        this.onChanges = onChanges;
        this.onAdd = onAdd;
        this.onRemove = onRemove;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

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
