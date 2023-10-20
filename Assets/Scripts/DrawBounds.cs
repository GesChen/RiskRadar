using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

		Transform partTransform = new GameObject(cityName + "PART", typeof(LineRenderer)).transform;
		partTransform.parent = cityTransform;
		
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
		lineRenderer.startWidth = width;
		lineRenderer.endWidth = width;
	}
}
