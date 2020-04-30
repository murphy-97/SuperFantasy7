using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawMove : MonoBehaviour
{
    public Transform saw;
    public Transform endPoint;
    public float sawSpeed;
    
    private bool away = true;
    private float length;
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        length = Vector3.Distance(transform.position, endPoint.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (away) {
            float distanceCovered = (Time.time - startTime) * sawSpeed;

            saw.position = Vector3.Lerp(transform.position, endPoint.position, distanceCovered/length);
        }
        else
        {
            float distanceCovered = (Time.time - startTime) * sawSpeed;

            saw.position = Vector3.Lerp(endPoint.position, transform.position, distanceCovered / length);
        }
        if (Vector3.Distance(saw.position, endPoint.position) == 0 && away)
        {
            away = false;
            startTime = Time.time;
        }
        if (Vector3.Distance(saw.position, transform.position) == 0 && !away)
        {
            away = true;
            startTime = Time.time;
        }
    }
}
