using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class DrawBounds : MonoBehaviour
{
	public Transform container;
	public Material mat;
	public float width;
	public void Draw()
	{
		StartCoroutine(DrawCoroutine());
	}
	IEnumerator DrawCoroutine() 
	{
		foreach (string stateName in LoadData.Data.Keys)
		{
			StartCoroutine(DrawState(stateName));
			yield return new WaitForSeconds(.01f);
		}
	}
	IEnumerator DrawState(string stateName)
	{
		// iterate through cities in state
		State stateData = LoadData.Data[stateName];
		foreach (string cityName in stateData.Cities.Keys)
		{
			yield return new WaitForEndOfFrame();
			CityData cityData = stateData.Cities[cityName];

			List<List<List<List<float>>>> geometry = cityData.Geometry;

			// iterate through parts of city in this terrible data structure
			foreach (List<List<List<float>>> bounds in geometry)
			{
				// convert coords in form of list [x,y] to vec2

				List<Vector2> coords = new();
				for (int i = 0; i < bounds[0].Count - 1; i++) //exclude last point, duplicate
				{
					Vector2 coord = new (bounds[0][i][0], bounds[0][i][1]);
					if (!coords.Contains(coord)) // noduplicates
					{
						coords.Add(coord);
					}
				}

				CreateCityPart(stateName, cityName, coords.ToArray());
			}
		}

		Main.totalProgress++;
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

		Transform partTransform = new GameObject(cityName + " PART").transform;
		partTransform.parent = cityTransform;

		Mesh mesh = new();
		mesh.Clear();

		int[] tris = Triangulate(geometry);
		Vector3[] verts = new Vector3[geometry.Length];
		for (int i = 0; i < geometry.Length; i++)
		{
			verts[i] = geometry[i];
		}

		mesh.vertices = verts;
		mesh.triangles = tris;

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		partTransform.AddComponent<MeshFilter>().sharedMesh = mesh;
		partTransform.AddComponent<MeshRenderer>().material = mat;

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
		List<int> triangles = new();
		List<int> remainingIndexes = new();
		for (int i = 0; i < geometry.Length; i++)
			remainingIndexes.Add(i);
		
		while (remainingIndexes.Count > 3)
		{
			bool found = false;
			// check remaining points only
			for (int p = 0; p < remainingIndexes.Count; p++)
			{				
				int prev = remainingIndexes[p == 0 ? remainingIndexes.Count - 1 : p - 1]; //modulo wasnt doing what it was suposed to
				int cur  = remainingIndexes[p];
				int next = remainingIndexes[(p + 1) % remainingIndexes.Count];

				Vector2 a = geometry[prev];
				Vector2 b = geometry[cur];
				Vector2 c = geometry[next];

				if (collinear(a, b, c)) //collinear points break everything
				{
					// detect and remove, then skip processing
					Debug.LogWarning($"Found Collinear point at {cur}, removing");
					remainingIndexes.RemoveAt(p);
					continue;
				}

				// angle is less than 180
				if (validAngle(a, b, c))
				{
					// check if any points in remaining polygon other than tris 
					// own points are in the tri
					bool noneIn = true;
					for (int v = 0; v < remainingIndexes.Count; v++)
					{
						int check = remainingIndexes[v];
						// dont include any of the triangles own points
						if (!(check == prev || check == cur || check == next))
						{
							// slightly expensive, dont calculate if not needed
							if (pointInTri(a, b, c , geometry[remainingIndexes[v]]))
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

						remainingIndexes.RemoveAt(p);
						found = true;
						break;
					}
				}
			}
			if (!found)
			{
				Debug.LogWarning("how does this happend");
				triangles.Add(remainingIndexes[^1]);
				triangles.Add(remainingIndexes[0]);
				triangles.Add(remainingIndexes[1]);

				remainingIndexes.RemoveAt(0);
			}
		}

		// finally add last 3 verts for final tri
		try
		{
			triangles.Add(remainingIndexes[0]);
			triangles.Add(remainingIndexes[1]);
			triangles.Add(remainingIndexes[2]);
		}
		catch
		{
			Debug.LogWarning("the hell");
		}
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
