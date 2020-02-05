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
    [SerializeField] private Transform player_char;

    /* UNITY BUILT-IN METHODS */

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        // Update based on player height
        float angle = (40.0f * player_char.position.y / 17.0f) - 1.0f;
        pivot.eulerAngles = new Vector3(
            Mathf.Clamp(angle, 0.0f, 20.0f),
            pivot.eulerAngles.y,
            pivot.eulerAngles.z
        );
    }

    /* SPECIFIED METHODS */
}
