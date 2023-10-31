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
		if (!disableDraw)    drawBoundsComponent.Draw();
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
			if(Time.time - lastClickTime < doubleTapMaxInterval && doubleClickValid 
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

	[HideInInspector] public bool movingCamera;
	IEnumerator ShowLocation(float longitude,  float latitude)
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

	public void VizData()
	{
		dataViz.Visualize(typeSelector.options[typeSelector.value].text);
	}
}
