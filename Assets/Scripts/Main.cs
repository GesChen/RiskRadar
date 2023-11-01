using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
	[Header("DEBUG")]
	public bool disableLoadData;
	public bool disableDraw;
	public bool disableLoadMap;
	public bool disableLoading;

	LoadData loadDataComponent;
	DrawBounds drawBoundsComponent;
	LoadMap loadMapComponent;
	CameraNavigation cameraNavigation;

	public int month;

	[Header("Loading")]
	public GameObject[] hideWhileLoading;
	public Volume volume;
	DepthOfField dof;
	public Slider progressBar;
	public static int totalProgress;
	public static bool Loading = true;
	private float finishProgress = 50 + 1 + 8; // last number is completely arbitrary, i have no clue where it came from
	bool doneTransitioning;

	Image progressBarFillImage;
	Image progressBarBGImage;
	TextMeshProUGUI loadingText;

	[Header("Location Input")]
	public TMP_InputField LocationInputField;
	public float doubleTapMaxInterval;
	float lastClickTime;
	bool doubleClickValid;
	Vector3 lastClickPos;
	public TextMeshProUGUI errorText;
	public Transform locationMarker;
	public float focusSmoothness;
	public float focusZoomLevel;

	[Header("Data Viz")]
	public DataViz dataViz;
	public TMP_Dropdown typeSelector;
	public TextMeshProUGUI scaleMaxText;
	public Popout DataPopOut;

	// Start is called before the first frame update
	void Start()
	{
		foreach (GameObject obj in hideWhileLoading)
		{
			obj.SetActive(false);
		}

		loadDataComponent = GetComponent<LoadData>();
		drawBoundsComponent = GetComponent<DrawBounds>();
		loadMapComponent = GetComponent<LoadMap>();
		cameraNavigation = GetComponent<CameraNavigation>();

		Loading = true;
		volume.profile.TryGet(out dof);
		progressBarBGImage = progressBar.transform.Find("Background").GetComponent<Image>();
		progressBarFillImage = progressBar.fillRect.GetComponent<Image>();
		loadingText = progressBar.transform.Find("loadingtext").GetComponent<TextMeshProUGUI>();

		month = DateTime.Now.Month;

		if (!disableLoadData) loadDataComponent.Load();
		if (!disableDraw) drawBoundsComponent.Draw();
		if (!disableLoadMap) loadMapComponent.Load(month);
		if (disableLoading) finishProgress = 0;

	}

	// Update is called once per frame
	void Update()
	{
		HandleLoadingTransition();
		HandleDoubleTap();
	}
	void HandleLoadingTransition()
	{
		if (totalProgress >= finishProgress && !doneTransitioning)
		{
			Loading = false;
			dof.focusDistance.value += .1f;

			progressBar.value = Mathf.Lerp(progressBar.value, 1, .1f);

			Color fillCol = progressBarFillImage.color;
			fillCol.a = Mathf.Lerp(fillCol.a, 0, .1f);
			progressBarFillImage.color = fillCol;
			loadingText.color = fillCol;

			Color bgCol = progressBarBGImage.color;
			bgCol.a = Mathf.Lerp(bgCol.a, 0, .1f);
			progressBarBGImage.color = bgCol;

			foreach (GameObject obj in hideWhileLoading)
			{
				obj.SetActive(true);
			}
		}
		else
		{
			progressBar.value = Mathf.Lerp(progressBar.value, totalProgress / finishProgress, .1f);
		}
		if (dof.focusDistance.value > 10 && !doneTransitioning)
		{
			doneTransitioning = true;
			progressBar.gameObject.SetActive(false);
			dof.active = false;
			StartCoroutine(startupLoad());
		}
	}
	IEnumerator startupLoad()
	{
		yield return new WaitForSeconds(3);
		VizData();
	}

	void HandleDoubleTap()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (Time.time - lastClickTime < doubleTapMaxInterval && doubleClickValid
				&& Vector3.Distance(lastClickPos, Input.mousePosition) < 40) // double tapped
			{
				Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				StartCoroutine(ShowLocation(pos.x, pos.y));
				LocationInputField.text = $"{pos.x}, {pos.y}";
			}
			lastClickTime = Time.time;
			lastClickPos = Input.mousePosition;
			doubleClickValid = true;
		}
		if (Time.time > lastClickTime + doubleTapMaxInterval)
		{
			doubleClickValid = false;
		}
	}

	public void Load()
	{
		StartCoroutine(TryLoad());
	}
	public IEnumerator TryLoad()
	{
		string query = LocationInputField.text;

		float longitude = 0;
		float latitude = 0;

		// checks if is coordinate format (regex magic)
		string pattern = @"^\s*(-?\d+(\.\d+)?)\s*,\s*(-?\d+(\.\d+)?)\s*$";
		Match match = Regex.Match(query, pattern);
		if (match.Success)
		{
			longitude = float.Parse(match.Groups[3].Value);
			latitude = float.Parse(match.Groups[1].Value);
		}
		else // try to find address
		{
			string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={UnityWebRequest.EscapeURL(query)}&key=AIzaSyC8eYyUYKeBR_hLPzCfnGDJqHZw62y_zdo";

			using (UnityWebRequest www = UnityWebRequest.Get(url))
			{
				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.Success)
				{
					string jsonResult = www.downloadHandler.text;
					try
					{
						// Parse the JSON response
						GeocodingResponse data = JsonConvert.DeserializeObject<GeocodingResponse>(jsonResult);


						// Check if the response contains results
						if (data.results.Count > 0)
						{
							// Extract latitude and longitude from the first result
							latitude = data.results[0].geometry.location.lat;
							longitude = data.results[0].geometry.location.lng;
						}
						else
						{
							Debug.LogWarning("No results found in the geocoding response.");
						}
					}
					catch (Exception e)
					{
						Debug.LogError($"Error parsing geocoding response: {e.Message}");
					}
				}
				else
				{
					Debug.LogError($"Error: {www.error}");
				}
			}

		}

		// successfully got coords
		if (longitude != 0 || latitude != 0)
		{
			StartCoroutine(ShowLocation(longitude, latitude));
		}
		else // error
		{
			errorText.gameObject.SetActive(true);
			errorText.text = "Could not find address";
			yield return new WaitForSeconds(2);
			errorText.gameObject.SetActive(false);
		}
	}

	[HideInInspector] public bool movingCamera;
	IEnumerator ShowLocation(float longitude, float latitude)
	{
		// out of bounds
		if (longitude < cameraNavigation.boundsStart.x || longitude > cameraNavigation.boundsEnd.x || latitude < cameraNavigation.boundsStart.y || latitude > cameraNavigation.boundsEnd.y)
		{
			errorText.gameObject.SetActive(true);
			errorText.text = "Out of bounds error";
			yield return new WaitForSeconds(2);
			errorText.gameObject.SetActive(false);
			yield break;
		}

		HandleDataPopout(longitude, latitude);
		movingCamera = true;
		Vector3 pos = new(longitude, latitude, -10);
		locationMarker.position = pos + 5 * Vector3.forward;
		while (!Input.GetMouseButton(0) || Input.mouseScrollDelta.y > .1f)
		{
			transform.position = Vector3.Lerp(transform.position, pos, focusSmoothness);
			Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, focusZoomLevel, focusSmoothness);
			if (Vector3.Distance(transform.position, pos) < .01f)
				break;
			yield return null;
		}
		movingCamera = false;
	}

	void HandleDataPopout(float longitude, float latitude)
	{
		Place place = getPlace(longitude, latitude);
		if (place.City != null)
		{
			DataPopOut.open = true;
			List<WeatherEvent> weatherEvents = LoadData.Data[place.State].Cities[place.City].Events;

			// set title text
			DataPopOut.titleText.text = $"{place.City}, {place.State}";

			// set body text
			TextMeshProUGUI text = DataPopOut.bodyText;
			text.text = "";

			foreach (WeatherEvent weatherEvent in weatherEvents)
			{
				text.text += $"{weatherEvent.Type} has occurred {weatherEvent.TotalCount} times \n";
			}
		}
	}

	struct Place
	{
		public string City { get; set; }
		public string State { get; set; }
	}
	// finds what city point is in, otherwise returns closest city
	Place getPlace(float longitude, float latitude) 
	{
		Vector2 point = new(longitude, latitude);
		float closestDistanceSqrd = Mathf.Infinity;
		Place closestPlace = new Place();
		foreach (Transform state in GameObject.Find("Visuals").transform)
		{
			foreach(Transform city in state)
			{
				foreach(MeshFilter part in city.GetComponentsInChildren<MeshFilter>())
				{
					if (pointInPoly(part.mesh.vertices, point))
					{
						return new Place { City = city.name, State = state.name };
					}

					float dist = ((Vector3) point - part.transform.position).sqrMagnitude;
					if (dist < closestDistanceSqrd) // keep track of closest part if 
					{ 
						closestDistanceSqrd = dist;
						closestPlace = new Place { City = city.name, State = state.name };
					}
				}
			}
		}
		return closestPlace;
	}
	bool pointInPoly(Vector3[] polygon, Vector2 point)
	{
		int polygonLength = polygon.Length;
		bool inside = false;

		for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
		{
			if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
				point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
			{
				inside = !inside;
			}
		}


		return inside;

	}


	public class GeocodingResponse
	{
		public List<Result> results;
	}
	public class Result
	{
		public Geometry geometry;
	}
	public class Geometry
	{
		public Location location;
	}
	public class Location
	{
		public float lat;
		public float lng;
	}

	public void VizData()
	{
		dataViz.Visualize(typeSelector.options[typeSelector.value].text);
	}
}
