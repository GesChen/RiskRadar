using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScaling : MonoBehaviour
{
    public float scale;
    public float multiplier;

    void Update()
    {
        float actualScale = scale * multiplier;
        transform.localScale = actualScale * Vector3.one;
        transform.position = new Vector3(-actualScale / 2, actualScale / 2, 1);
    }
}
