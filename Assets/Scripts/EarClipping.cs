using System.Collections.Generic;
using UnityEngine;

public class EarClipping
{
	public static List<int> Triangulate(List<Vector2> polygon)
	{
		List<int> triangles = new List<int>();

		if (polygon.Count < 3)
		{
			Debug.LogWarning("Input polygon must have at least 3 vertices.");
			return triangles;
		}

		List<Vector2> remainingVertices = new List<Vector2>(polygon);

		while (remainingVertices.Count >= 3)
		{
			int earTipIndex = FindEarTip(remainingVertices);

			if (earTipIndex == -1)
			{
				Debug.LogError("No ear tip found. Input polygon may be self-intersecting.");
				break;
			}

			int prevIndex = (earTipIndex - 1 + remainingVertices.Count) % remainingVertices.Count;
			int nextIndex = (earTipIndex + 1) % remainingVertices.Count;

			triangles.Add(prevIndex);
			triangles.Add(earTipIndex);
			triangles.Add(nextIndex);

			remainingVertices.RemoveAt(earTipIndex);
		}

		return triangles;
	}

	private static int FindEarTip(List<Vector2> vertices)
	{
		int vertexCount = vertices.Count;

		for (int i = 0; i < vertexCount; i++)
		{
			int prevIndex = (i - 1 + vertexCount) % vertexCount;
			int nextIndex = (i + 1) % vertexCount;

			Vector2 prevVertex = vertices[prevIndex];
			Vector2 currentVertex = vertices[i];
			Vector2 nextVertex = vertices[nextIndex];

			if (IsConvex(prevVertex, currentVertex, nextVertex) && !IsAnyPointInsideTriangle(prevVertex, currentVertex, nextVertex, vertices))
			{
				return i;
			}
		}

		return -1;
	}

	private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
	{
		Vector2 side1 = b - a;
		Vector2 side2 = c - b;
		float crossProduct = side1.x * side2.y - side1.y * side2.x;

		return crossProduct < 0; // Convex if the cross product is negative
	}

	private static bool IsAnyPointInsideTriangle(Vector2 a, Vector2 b, Vector2 c, List<Vector2> vertices)
	{
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector2 point = vertices[i];
			if (point != a && point != b && point != c && IsPointInTriangle(point, a, b, c))
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
	{
		float u = ((a.x - p.x) * (b.y - a.y) - (b.x - a.x) * (a.y - p.y)) /
				  ((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (a.y - b.y));
		float v = ((c.x - p.x) * (a.y - c.y) - (a.x - c.x) * (c.y - p.y)) /
				  ((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (a.y - b.y));

		return u >= 0 && v >= 0 && (u + v) <= 1;
	}
}
