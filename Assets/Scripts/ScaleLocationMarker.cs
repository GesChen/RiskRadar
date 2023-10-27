using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleLocationMarker : MonoBehaviour
{
    public float scale;
    void Update()
    {
        transform.localScale = Camera.main.orthographicSize * scale * Vector3.one;
    }
}
