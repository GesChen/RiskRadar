using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
	LoadData loadDataComponent;
	DrawBounds drawBoundsComponent;
	LoadMap loadMapComponent;

	public Volume volume;
	DepthOfField dof;

	public int month;

	public Slider progressBar;
	[HideInInspector] public static int totalProgress;
	private float finishProgress = 50 + 1 + 8 + 19; // 19 extra states???????????
	bool doneTransitioning;

	Image progressBarFillImage;
	Image progressBarBGImage;

	// Start is called before the first frame update
	void Start()
	{
		loadDataComponent = GetComponent<LoadData>();
		drawBoundsComponent = GetComponent<DrawBounds>();
		loadMapComponent = GetComponent<LoadMap>();

		volume.profile.TryGet(out dof);
		progressBarBGImage = progressBar.transform.Find("Background").GetComponent<Image>();
		progressBarFillImage = progressBar.fillRect.GetComponent<Image>();

		month = DateTime.Now.Month;

		loadDataComponent.Load();
		drawBoundsComponent.Draw();
		loadMapComponent.Load(month);
	}

	// Update is called once per frame
	void Update()
	{
		if (totalProgress >= finishProgress && !doneTransitioning)
		{
			dof.focusDistance.value += .1f;

			progressBar.value = Mathf.Lerp(progressBar.value, 1, .1f);

			Color fillCol = progressBarFillImage.color; 
			fillCol.a = Mathf.Lerp(fillCol.a, 0, .1f);
			progressBarFillImage.color = fillCol;
			
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
		}
	}
}
