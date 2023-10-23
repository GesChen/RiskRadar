using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DrawBounds : MonoBehaviour
{
	public Transform container;
	public Material lineMat;
	public float width;
    public void Draw()
    {
		foreach (string stateName in LoadData.Data.Keys)
		{
			State stateData = LoadData.Data[stateName];
			foreach (string cityName in stateData.Cities.Keys)
			{
				CityData cityData = stateData.Cities[cityName];

				List<List<List<List<float>>>> geometry = cityData.Geometry;

				foreach (List<List<List<float>>> bounds in geometry)
				{
					Vector2[] coords = new Vector2[bounds[0].Count];
					for (int i = 0; i < bounds[0].Count; i++)
					{
						coords[i] = new(bounds[0][i][0], bounds[0][i][1]);
					}
					CreateCityPart(stateName, cityName, coords);
				}
			}
		}
	}
	void CreateCityPart(string stateName, string cityName, Vector2[] geometry)
	{
		Transform stateTransform = container.Find(stateName);
		if (stateTransform == null)
		{
			stateTransform = new GameObject(stateName).transform;
			stateTransform.parent = container;
		}
		Transform cityTransform = stateTransform.Find(cityName);
		if (cityTransform == null)
		{
			cityTransform = new GameObject(cityName).transform;
			cityTransform.parent = stateTransform;
		}

		Transform partTransform = new GameObject(cityName + " PART", typeof(MeshFilter), typeof(MeshRenderer)).transform;
		partTransform.parent = cityTransform;

		Mesh mesh = new Mesh();
		mesh.Clear();

		int[] tris = Triangulate(geometry);
		Vector3[] verts = new Vector3[geometry.Length];
		for (int i = 0;i < geometry.Length; i++)
		{
			verts[i] = geometry[i];
		}

		mesh.vertices = verts;
		mesh.triangles = tris;

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		
		partTransform.GetComponent<MeshFilter>().sharedMesh = mesh;

		/*
		LineRenderer lineRenderer = partTransform.GetComponent<LineRenderer>();
		Vector3[] geometryV3 = new Vector3[geometry.Length];
		for (int i = 0;i < geometry.Length; i++)
		{
			geometryV3[i] = geometry[i];
		}
		lineRenderer.positionCount = geometryV3.Length;
		lineRenderer.SetPositions(geometryV3);
		lineRenderer.material = lineMat;
		lineRenderer.alignment = LineAlignment.TransformZ;
		lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lineRenderer.receiveShadows = false;

		lineRenderer.startWidth = width;
		lineRenderer.endWidth = width;*/
	}
	int[] Triangulate(Vector2[] geometry)
	{
		List<int> triangles = new ();
		List<int> remainingVerts = geometry.ToList();
		
		for(int i = 0; i < geometry.Length; i++) 
			remainingVerts.Add(geometry[i]);
		
		while (remainingVerts.Count > 3)
		{
			// check remaining points only
			for (int p = 0; p < remainingVerts.Count; p++)
			{
				int prev = remainingVerts[p == 0 ? remainingVerts.Count - 1 : (p - 1)]; //modulo wasnt doing what it was suposed to
				int cur  = remainingVerts[p];
				int next = remainingVerts[(p + 1) % remainingVerts.Count];

				Vector2 a = geometry[prev];
				Vector2 b = geometry[cur];
				Vector2 c = geometry[next];

				if (collinear(a, b, c)) //collinear points break everything
				{ 
					// detect and remove, then skip processing
					Debug.Log($"Found Collinear point at {cur}, removing");
					remainingVerts.RemoveAt(p);
					continue;
				}

				if (validAngle(a, b, c))
				{
					bool noneIn = true;
					for (int v = 0; v < geometry.Length; v++)
					{
						if (remainingVerts.Contains(v) && v != p - 1 && v != p && v != p + 1)
						{
							if (pointInTri(geometry[v], a, b, c }))
							{
								noneIn = false;
								break;
							}
						}
					}
					if (noneIn)
					{
						// valid, add to triangles and remove from remaining verts
						triangles.Add(prev);
						triangles.Add(cur);
						triangles.Add(next);

						remainingVerts.RemoveAt(p);
						break;
					}
				}
			}
		}

		// finally add last 3 verts for final tri
		triangles.Add(remainingVerts[0]);
		triangles.Add(remainingVerts[1]);
		triangles.Add(remainingVerts[2]);

		return triangles.ToArray();
	}
	bool validAngle(Vector2 a, Vector2 b,  Vector2 c)
	{
		return cross(a, b, c) > 0;
	}
	bool collinear(Vector2 a, Vector2 b, Vector2 c)
	{ 
		return cross(a, b, c) == 0;
	}
	float cross(Vector2 a, Vector2 b, Vector2 c) 
	{
		Vector2 v0 = a - b;
		Vector2 v1 = c - b;

		return v0.x * v1.y - v1.x * v0.y;
	}
	public static bool pointInTri(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float u = (A.x * (C.y - A.y) + (P.y - A.y) * (C.x - A.x) - P.x * (C.y - A.y)) / ((B.y - A.y) * (C.x - A.x) - (B.x - A.x) * (C.y - A.y));
		float v = (P.y - A.y - u * (B.y - A.y)) / (C.y - A.y);

		// Check if point is in triangle
		return (u >= 0) && (v >= 0) && (u + v <= 1);
	}
}
