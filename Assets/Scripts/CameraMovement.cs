using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float drift;

	Vector3 dragStart;
	Vector3 targetPos;
 	bool dragging;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			dragging = true;
        }
        else if (Input.GetMouseButton(0))
        {
            targetPos = dragstart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		
        transform.position = Vector3.Lerp(transform.position, targetPos, drift);
	}
}
