using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Main : MonoBehaviour
{
    LoadData loadDataComponent;
    DrawBounds drawBoundsComponent;
    LoadMap loadMapComponent;

    // Start is called before the first frame update
    void Start()
    {
        loadDataComponent = GetComponent<LoadData>();
        drawBoundsComponent = GetComponent<DrawBounds>();
        loadMapComponent = GetComponent<LoadMap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
			loadDataComponent.Load();
			drawBoundsComponent.Draw();
            loadMapComponent.Load(DateTime.Now.Month);
		}
    }
}
