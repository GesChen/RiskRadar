using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Main : MonoBehaviour
{
    LoadData loadDataComponent;
    DrawBounds drawBoundsComponent;

    // Start is called before the first frame update
    void Start()
    {
        loadDataComponent = GetComponent<LoadData>();
        drawBoundsComponent = GetComponent<DrawBounds>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			loadDataComponent.Load();
			stopwatch.Stop();
			UnityEngine.Debug.Log($"loading time: {stopwatch.Elapsed}");
			stopwatch.Reset();
			stopwatch.Start();
			drawBoundsComponent.Draw();
			stopwatch.Stop();
			UnityEngine.Debug.Log($"debug drawing time: {stopwatch.Elapsed}");
		}
    }
}
