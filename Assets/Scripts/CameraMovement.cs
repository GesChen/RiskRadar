using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float drift;

	Vector3 lastMousePos;
	Vector3 vel;
    void Update()
    {
        Debug.Log(lastMousePos);
        Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            vel = lastMousePos - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
        transform.position += vel;
        vel *= 1 - drift;
        lastMousePos = Vector3.Lerp(lastMousePos, Camera.main.ScreenToWorldPoint(Input.mousePosition), drift);
    }
}
