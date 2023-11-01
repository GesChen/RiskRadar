using System.Collections;
using System.Collections.Generic;
using TMPro;
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
	[Space]
	public TextMeshProUGUI titleText;
	public TextMeshProUGUI bodyText;

	void Update()
	{
		arrowPart1.rotation = Quaternion.Lerp(arrowPart1.rotation, Quaternion.Euler(0, 0, open ? -45: 45), smoothness * Time.deltaTime);
		arrowPart2.rotation = Quaternion.Lerp(arrowPart2.rotation, Quaternion.Euler(0, 0, open ? 45 : -45) , smoothness * Time.deltaTime);

		transform.localPosition = Vector3.Lerp(transform.localPosition, open ? localOpenPos : localClosePos, smoothness * Time.deltaTime);
	}
	public void Toggle()
	{
		open = !open;
	}
}
