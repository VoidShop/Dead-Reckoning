using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    private float zBound = 50.0f;
    private float xBound = 50.0f;


    void Update()
    {
        //destroys object that passes out of boundary
        if (transform.position.z > zBound)
        {
            Destroy(gameObject);
        }
        if (transform.position.z < -zBound)
        {
            Destroy(gameObject);
        }
        if (transform.position.x > xBound)
        {
            Destroy(gameObject);
        }
        if (transform.position.x < -xBound)
        {
            Destroy(gameObject);
        }
    }
}
