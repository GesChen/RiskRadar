using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraNavigation : MonoBehaviour
{
    public float globalDrift;
	public float zoomSensitivity;
	public float zoom;
	public float minZoom;
	public float maxZoom;

	public Vector2 boundsStart;
	public Vector2 boundsEnd;

	Vector3 dragStart;
	[HideInInspector] public Vector3 vel;
	Vector3 smoothedVel;

	float targetZoom;
	Main main;
	void Start()
	{
		targetZoom = zoom;
		main = FindAnyObjectByType<Main>();
	}
	void Update()
	{
		if (!Main.Loading && !main.movingCamera)
		{
			HandleDragMovement();
			ApplyVelocity();
			HandleZoom();
			RestrictMovement();
		}
		else if (main.movingCamera)
		{
			zoom = main.focusZoomLevel;
			targetZoom = main.focusZoomLevel;
		}
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
		if (!EventSystem.current.IsPointerOverGameObject()) // mouse not over UI
		{
			targetZoom += -Input.mouseScrollDelta.y * zoomSensitivity * Time.deltaTime;
			targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
		}

		zoom = Mathf.Lerp(zoom, targetZoom, globalDrift);

		Camera.main.orthographicSize = zoom;
	}
	private void RestrictMovement()
	{
		Vector3 pos = transform.position;
		pos.x = Mathf.Clamp(pos.x, boundsStart.x, boundsEnd.x);
		pos.y = Mathf.Clamp(pos.y, boundsStart.y, boundsEnd.y);
		transform.position = pos;
	}
}