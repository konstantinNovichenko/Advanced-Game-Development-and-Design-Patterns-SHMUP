using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls rotation of the disc in Enemy_5
/// </summary>
public class Rotation : MonoBehaviour
{
    [Header("Set In Inspector")]
    public float rotationSpeed = 5.0f;
    // Start is called before the first frame update
    
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, Time.deltaTime * rotationSpeed, 0, Space.Self); // rotate
    }
}
