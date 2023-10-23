using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tests : MonoBehaviour
{
	public Transform A;
	public Transform B;
	public Transform C;

	public Transform P;
	
    void Update()
	{ 
		Debug.Log(DrawBounds.pointInTri(A, B, C, P));
	}
}
