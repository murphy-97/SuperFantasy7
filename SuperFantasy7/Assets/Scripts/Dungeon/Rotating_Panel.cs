using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct Panel_Angle {
    public float angle_target;
    public float angle_tolerance;
    public float speed;
    public float wait_time;
 }

public class Rotating_Panel : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        tolerance_min = angles[next_angle].angle_tolerance;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for target angle
        float angle = (gameObject.transform.eulerAngles.z + 360.0f) % 360.0f;
        float angle_target = (angles[next_angle].angle_target + 360.0f) % 360.0f;

        /*
        Debug.Log("Angle = " + angle);
        Debug.Log("Target = " + angle_target);
        Debug.Log("Diff = " + Mathf.Abs(angle - angle_target));
        */

        float tolerance = Mathf.Max(angles[next_angle].angle_tolerance, tolerance_min);
        if (Mathf.Abs(angle - angle_target) < tolerance) {
            // Within tolerance of target angle
            if (wait_timer < 0.0f) {
                // Ready to move to next angle
                next_angle = (next_angle + 1) % angles.Length;
                wait_timer = angles[next_angle].wait_time;
            } else {
                // Wait at this angle until timer expires
                wait_timer -= Time.deltaTime;
                gameObject.transform.eulerAngles = new Vector3(
                    gameObject.transform.eulerAngles.x,
                    gameObject.transform.eulerAngles.y,
                    angle_target
                );
            }

        } else {
            // Move to next angle
            float angle_change = Time.deltaTime * angles[next_angle].speed;
            gameObject.transform.eulerAngles = new Vector3(
                gameObject.transform.eulerAngles.x,
                gameObject.transform.eulerAngles.y,
                gameObject.transform.eulerAngles.z + angle_change
            );
        }

        tolerance_min = 1.5f * Time.deltaTime * angles[next_angle].speed;
    }

    // Object attributes
    [SerializeField] private int next_angle = 0;
    private float wait_timer = -1.0f;
    private float tolerance_min = 0.0f;
    [SerializeField] private Panel_Angle[] angles;
}
