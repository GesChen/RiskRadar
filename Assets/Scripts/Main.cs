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
	LoadData loadDataComponent;
	DrawBounds drawBoundsComponent;
	LoadMap loadMapComponent;

	public int month;

	[Header("Loading")]
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
	public TextMeshProUGUI errorText;
	public Transform locationMarker;
	public float focusSmoothness;
	public float focusZoomLevel;

	// Start is called before the first frame update
	void Start()
	{
		loadDataComponent = GetComponent<LoadData>();
		drawBoundsComponent = GetComponent<DrawBounds>();
		loadMapComponent = GetComponent<LoadMap>();

		Loading = true;
		volume.profile.TryGet(out dof);
		progressBarBGImage = progressBar.transform.Find("Background").GetComponent<Image>();
		progressBarFillImage = progressBar.fillRect.GetComponent<Image>();
		loadingText = progressBar.transform.Find("loadingtext").GetComponent<TextMeshProUGUI>();

		month = DateTime.Now.Month;

		loadDataComponent.Load();
		drawBoundsComponent.Draw();
		loadMapComponent.Load(month);
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

		}
		else
		{
			progressBar.value = Mathf.Lerp(progressBar.value, totalProgress / finishProgress, .1f);
		}
		if (dof.focusDistance.value > 10)
		{
			doneTransitioning = true;
			progressBar.gameObject.SetActive(false);
			dof.active = false;
		}
	}
	void HandleDoubleTap()
	{
		if (Input.GetMouseButtonDown(0))
		{
			lastClickTime = Time.time;
			if(Time.time - lastClickTime < doubleTapMaxInterval && doubleClickValid) // double tapped
			{
				Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				StartCoroutine(ShowLocation(pos.x, pos.y));
				LocationInputField.text = $"{pos.x}, {pos.y}";
			}
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
	IEnumerator ShowLocation(float longitude,  float latitude)
	{
		movingCamera = true;
		Vector3 pos = new (longitude, latitude, -10);
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
}
