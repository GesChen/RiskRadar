using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
    public float globalDrift;
	public float zoomSensitivity;
	public float zoom;
	public float minZoom;
	public float maxZoom;

	Vector3 dragStart;
	[HideInInspector] public Vector3 vel;
	Vector3 smoothedVel;

	float targetZoom;
	void Start()
	{
		targetZoom = zoom;
	}
	void Update()
	{
		HandleDragMovement();
		ApplyVelocity();
		HandleZoom();
	}

	private void HandleDragMovement()
	{
		if (Input.GetMouseButtonDown(0))
		{
			dragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else if (Input.GetMouseButton(0))
		{
			vel = dragStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else
		{
			vel = Vector3.zero;
		}
	}
	private void ApplyVelocity()
	{
		transform.position += (Input.GetMouseButton(0)) ? vel : smoothedVel;
		smoothedVel = (Input.GetMouseButton(0)) ? vel : Vector3.Lerp(smoothedVel, vel, globalDrift);
	}
	private void HandleZoom()
	{
		targetZoom += -Input.mouseScrollDelta.y * zoomSensitivity * Time.deltaTime;
		targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

		zoom = Mathf.Lerp(zoom, targetZoom, globalDrift);

		Camera.main.orthographicSize = zoom;
	}

}