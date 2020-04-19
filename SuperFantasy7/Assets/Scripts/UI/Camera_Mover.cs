using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Mover : MonoBehaviour
{
    /* CLASS ATTRIBUTES (STATIC) */
    private static Camera_Mover active_camera;  

    /* CLASS METHODS (STATIC) */
    public static Camera_Mover Get_Active_Camera()
    {
        return active_camera;
    }

    /* OBJECT ATTRIBUTES */
    [Header("Camera Objects")]
    [SerializeField] private Transform pivot;
    [SerializeField] private Camera camera;

    [Header("Tracking Data")]
    [SerializeField] private bool track_pivot;
    [SerializeField] private Transform player_char;
    [SerializeField] private Vector2 tracking_distance;
    [SerializeField] private float panning_speed;
    [SerializeField] private float panning_precision;
    private Vector2 target_position;

    /* UNITY BUILT-IN METHODS */

    void Awake()
    {
        target_position = new Vector2(
            transform.position.x,
            transform.position.y
        );
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move to target position
        Vector2 move_mult = Vector2.zero;

        if (transform.position.x - target_position.x > panning_precision) {
            move_mult.x = -1.0f;

        } else if (transform.position.x - target_position.x < -1.0f * panning_precision) {
            move_mult.x = 1.0f;
        }
        
        if (transform.position.y - target_position.y > panning_precision) {
            move_mult.y = -1.0f;

        } else if (transform.position.y - target_position.y < -1.0f * panning_precision) {
            move_mult.y = 1.0f;
        }

        // Update position
        transform.position = new Vector3(
            transform.position.x + (Time.deltaTime * panning_speed * move_mult.x),
            transform.position.y + (Time.deltaTime * panning_speed * move_mult.y),
            transform.position.z
        );
    }

    void FixedUpdate()
    {
        // Update rotation based on player height
        if (track_pivot) {
            float angle = (40.0f * player_char.position.y / 17.0f) - 1.0f;
            pivot.eulerAngles = new Vector3(
                Mathf.Clamp(angle, 0.0f, 20.0f),
                pivot.eulerAngles.y,
                pivot.eulerAngles.z
            );
        }

        // Update panning target based on player position
        if (target_position.x - player_char.position.x > tracking_distance.x) {
            // Camera too far to the right
            target_position.x -= (tracking_distance.x + tracking_distance.x);

        } else if (target_position.x - player_char.position.x < -1.0f * tracking_distance.x) {
            // Camera too far to the left
            target_position.x += (tracking_distance.x + tracking_distance.x);
        }
        
        if (target_position.y - player_char.position.y > tracking_distance.y) {
            // Camera too far up
            target_position.y -= (tracking_distance.y + tracking_distance.y);

        } else if (target_position.y - player_char.position.y < -1.0f * tracking_distance.y) {
            // Camera too far down
            target_position.y += (tracking_distance.y + tracking_distance.y);
        }
    }

    /* SPECIFIED METHODS */
}
