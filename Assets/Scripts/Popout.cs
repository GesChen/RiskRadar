using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popout : MonoBehaviour
{
	public bool open;
	public Vector3 localOpenPos;
	public Vector3 localClosePos;
	public float smoothness;
	[Space]
	public Transform arrowPart1;
	public Transform arrowPart2;

	void Update()
	{
		arrowPart1.rotation = Quaternion.Lerp(arrowPart1.rotation, Quaternion.Euler(0, 0, open ? -45: 45), smoothness);
		arrowPart2.rotation = Quaternion.Lerp(arrowPart2.rotation, Quaternion.Euler(0, 0, open ? 45 : -45) , smoothness);

		transform.localPosition = Vector3.Lerp(transform.localPosition, open ? localOpenPos : localClosePos, smoothness);
	}
	public void Toggle()
	{
		open = !open;
	}
}
