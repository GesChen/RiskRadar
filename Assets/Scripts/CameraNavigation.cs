using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
    public float globalDrift;
	public float zoomSensitivity;

	Vector3 dragStart;
	Vector3 vel;
	Vector3 smoothedVel;
    void Update()
    {
		if (Input.GetMouseButtonDown(0))
		{
			dragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else if (Input.GetMouseButton(0))
		{
			vel = dragStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
			transform.position += vel;
		}
		else
		{
			vel = Vector3.zero;
			transform.position += smoothedVel;
		}
		if (vel != Vector3.zero)
		{
			smoothedVel = vel;
		}
		else
		{
			smoothedVel = Vector3.Lerp(smoothedVel, vel, globalDrift);
		}
	}
}
