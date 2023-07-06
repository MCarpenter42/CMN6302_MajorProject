namespace NeoCambion.Unity.Geometry
{
    using System;
    using System.Collections.Generic;
	using UnityEngine;
	using NeoCambion.Maths.Matrices;

	public struct Edge
	{
		public readonly Vector3[] ends;
		public readonly float length;

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		public Edge(Vector3 start, Vector3 end) : this()
		{
			ends = new Vector3[] { start, end };
			length = (end - start).magnitude;
		}

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		public bool Matches(Edge edge)
		{
			return (ends[0] == edge.ends[0] && ends[1] == edge.ends[1] || ends[0] == edge.ends[1] && ends[1] == edge.ends[0]);
		}
	}

	public struct Triangle
	{
		public readonly Vector3[] points;
		public readonly Edge[] edges;
		public readonly Vector3 normal;
		public readonly Vector3 centre;
		public readonly float area;

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		public Triangle(Vector3[] points) : this()
		{
			this.points = new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.zero };
			for (int i = 0; i < 3; i++)
			{
				if (points.Length > i)
				{
					this.points[i] = points[i];
				}
				else if (i > 0)
				{
					this.points[i] = this.points[i - 1] + Vector3.one;
				}
			}
			edges = new Edge[]
			{
				new Edge(points[0], points[1]),
				new Edge(points[1], points[2]),
				new Edge(points[2], points[0])
			};
			normal = Normal();
			centre = Centre();
			area = Area();
		}

		public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC) : this()
		{
			points = new Vector3[3] { pointA, pointB, pointC };
			edges = new Edge[]
			{
				new Edge(pointA, pointB),
				new Edge(pointB, pointC),
				new Edge(pointC, pointA)
			};
			normal = Normal();
			area = Area();
		}

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		private Vector3 Normal()
		{
			Vector3 ab = points[1] - points[0];
			Vector3 ac = points[2] - points[0];
			return Vector3.Cross(ab, ac);
		}

		private float Area()
		{
			return Normal().magnitude / 2.0f;
		}

		public Vector3 Centre()
		{
			float cX = (points[0].x + points[1].x + points[2].x) / 3.0f;
			float cY = (points[0].y + points[1].y + points[2].y) / 3.0f;
			float cZ = (points[0].z + points[1].z + points[2].z) / 3.0f;
			return new Vector3(cX, cY, cZ);
		}

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		public bool Matches(Triangle tri)
		{
			foreach (Vector3 point in tri.points)
			{
				bool faceFound = point == points[0] || point == points[1] || point == points[2];
				if (!faceFound)
					return false;
			}
			return true;
		}

		public bool SharedEdge(Triangle tri)
		{
			foreach (Edge edgeA in edges)
			{
				foreach (Edge edgeB in tri.edges)
				{
					if (edgeA.Matches(edgeB))
						return true;
				}
			}
			return false;
		}

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		public Triangle Inverted()
		{
			Vector3[] newPoints = new Vector3[] { points[0], points[2], points[1] };
			return new Triangle(newPoints);
		}

		public static List<Triangle> TrisFromPointAndEdges(Vector3 point, List<Edge> edges)
		{
			List<Triangle> tris = new List<Triangle>();
			foreach (Edge edge in edges)
			{
				tris.Add(new Triangle(point, edge.ends[0], edge.ends[1]));
			}
			return tris;
		}

		public static List<Edge> GetUncommonEdges(List<Triangle> tris)
		{
			List<Edge> edges = new List<Edge>();
			foreach (Triangle tri in tris)
			{
				foreach (Edge edge in tri.edges)
				{
					int existsAt = -1;
					for (int i = 0; i < edges.Count; i++)
					{
						if (edge.Matches(edges[i]))
						{
							existsAt = i;
							break;
						}
					}
					if (existsAt >= 0)
						edges.RemoveAt(existsAt);
					else
						edges.Add(edge);
				}
			}
			return edges;
		}
	}

	public struct Tetrahedron
	{
		public readonly Vector3[] points;
		public readonly Edge[] edges;
		public readonly Triangle[] faces;

		private float[] calcValues;
		public readonly Vector3 circumcentre;
		public readonly float circumradius;

		public readonly float surfaceArea;
		public readonly float volume;

		private int _edgeShortest;
		public readonly Edge edgeShortest { get { return edges[_edgeShortest]; } }
		private int _edgeLongest;
		public readonly Edge edgeLongest { get { return (_edgeLongest >= 0) ? edges[_edgeLongest] : new Edge(Vector3.zero, Vector3.zero); } }

		public readonly float circumShortestRatio { get { return circumradius / edgeShortest.length; } }
		public readonly float surfAreaVolRatio { get { return surfaceArea / volume; } }

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		public Tetrahedron(Vector3[] points) : this()
		{
			this.points = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
			for (int i = 0; i < 4; i++)
			{
				if (points.Length > i)
				{
					this.points[i] = points[i];
				}
				else if (i > 0)
				{
					this.points[i] = this.points[i - 1] + Vector3.one;
				}
			}
			edges = new Edge[6]
			{
				new Edge(this.points[0], this.points[1]),
				new Edge(this.points[0], this.points[2]),
				new Edge(this.points[0], this.points[3]),
				new Edge(this.points[1], this.points[2]),
				new Edge(this.points[1], this.points[3]),
				new Edge(this.points[2], this.points[3])
			};
			faces = new Triangle[4]
			{
				new Triangle(this.points[0], this.points[1], this.points[2]),
				new Triangle(this.points[1], this.points[2], this.points[3]),
				new Triangle(this.points[2], this.points[3], this.points[0]),
				new Triangle(this.points[3], this.points[0], this.points[1])
			};

			_edgeShortest = -1;
			float sLength = float.MaxValue;
			_edgeLongest = -1;
			float lLength = float.MinValue;
			for (int i = 0; i < 6; i++)
			{
				if (edges[i].length < sLength)
				{
					_edgeShortest = i;
					sLength = edges[i].length;
				}
				else if (edges[i].length > lLength)
				{
					_edgeLongest = i;
					lLength = edges[i].length;
				}
			}

			surfaceArea = faces[0].area + faces[1].area + faces[2].area + faces[3].area;
			float h = Mathf.Abs(Vector3.Dot(this.points[3] - this.points[0], faces[0].normal.normalized));
			volume = faces[0].area * h / 3.0f;

			calcValues = new float[5];
			float[,] invCayleyMenger = SquareMatrixUtility.Inverse(CayleyMengerMatrix());
			for (int i = 0; i < 5; i++)
			{
				calcValues[i] = -2.0f * invCayleyMenger[i, 0];
			}

			circumcentre = Circumcentre();
			circumradius = Circumradius();
		}

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		private float[,] CayleyMengerMatrix()
		{
			float d12 = Mathf.Pow((points[1] - points[0]).magnitude, 2);
			float d13 = Mathf.Pow((points[2] - points[0]).magnitude, 2);
			float d14 = Mathf.Pow((points[3] - points[0]).magnitude, 2);
			float d23 = Mathf.Pow((points[2] - points[1]).magnitude, 2);
			float d24 = Mathf.Pow((points[3] - points[1]).magnitude, 2);
			float d34 = Mathf.Pow((points[3] - points[2]).magnitude, 2);
			return new float[,]
			{
				{ 000, 001, 001, 001, 001 },
				{ 001, 000, d12, d13, d14 },
				{ 001, d12, 000, d23, d24 },
				{ 001, d13, d23, 000, d34 },
				{ 001, d14, d24, d34, 000 }
			};
		}

		private Vector3 Circumcentre()
		{
			Vector3 multiSum = calcValues[1] * points[0] + calcValues[2] * points[1] + calcValues[3] * points[2] + calcValues[4] * points[3];
			float divisor = calcValues[1] + calcValues[2] + calcValues[3] + calcValues[4];
			return multiSum / divisor;
		}

		private float Circumradius()
		{
			return 0.5f * Mathf.Pow(calcValues[0], 0.5f);
		}

		/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

		public bool InCircumsphere(Vector3 point)
		{
			float disp = (point - circumcentre).magnitude;
			return disp <= circumradius;
		}

		public Vector3 ClosestPoint(Vector3 point)
		{
			return points[ClosestIndex(point)];
		}

		public int ClosestIndex(Vector3 point)
		{
			float cDist = float.MaxValue;
			int index = -1;
			for (int i = 0; i < 4; i++)
			{
				if ((points[i] - point).magnitude < cDist)
				{
					cDist = points[i].magnitude;
					index = i;
				}
			}
			return index;
		}

		public Vector3 FurthestPoint(Vector3 point)
		{
			return points[FurthestIndex(point)];
		}

		public int FurthestIndex(Vector3 point)
		{
			float cDist = float.MinValue;
			int index = -1;
			for (int i = 0; i < 4; i++)
			{
				if ((points[i] - point).magnitude > cDist)
				{
					cDist = points[i].magnitude;
					index = i;
				}
			}
			return index;
		}

		public float[] EdgeLengthRatios()
		{
			float ab_cd = (edges[0].length > edges[5].length) ? edges[0].length / edges[5].length : edges[5].length / edges[0].length;
			float ac_bd = (edges[1].length > edges[4].length) ? edges[1].length / edges[4].length : edges[4].length / edges[1].length;
			float ad_bc = (edges[2].length > edges[3].length) ? edges[2].length / edges[3].length : edges[3].length / edges[2].length;
			return new float[] { ab_cd, ac_bd, ad_bc };
		}
	}
}
