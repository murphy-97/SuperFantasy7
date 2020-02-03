/*

The "Player_Char" entitiy represents the actual character object controlled by
the player in the current life. It is created on spawn and destroyed on death.

The equipment, ,ax health, and abilities accessible to the player are stored in
static variables, guaranteeing persistence between scene loads.

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Char : MonoBehaviour
{
    /* CLASS ATTRIBUTES (STATIC) */

    /* Player data */
    private static int health_max;
    private static int attack_basic_damage;    

    /* CLASS METHODS (STATIC) */

    /* OBJECT ATTRIBUTES */

    // Combat data
    [Header("Combat Data")]
    [SerializeField] private int health_current;

    // Movement data
    [Header("Movement Data")]
    [SerializeField] private float speed_run;
    [SerializeField] private float speed_jump;
    [Range(-1,0)][SerializeField] private float jump_fall_rate; // Used to end jump
    private Rigidbody rb;
    [SerializeField] private bool is_grounded = false;  // Serialized for debugging
    Vector3 speed_change;
    private bool stop_running;

    // Respawn data
    [Header("Respawn Data")]
    [SerializeField] private float respawn_timer;
    [SerializeField] private Transform respawn_loc;
    private bool dead = false;

    /* UNITY BUILT-IN METHODS */

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        /* BEGIN PLAYER CONTROLS */
        /*
            Controls are set in the Unity configuration launch menu. By using
            these names instead of specifying a key or mouse button, we allow
            users to customize controls or use a control scheme other than a
            mouse and keyboard.
        */
        bool move_up = !dead && Input.GetAxisRaw("Vertical") > 0;
		bool move_down = !dead && Input.GetAxisRaw("Vertical") < 0;
		bool move_l = !dead && Input.GetAxisRaw("Horizontal") < 0;
		bool move_r = !dead && Input.GetAxisRaw("Horizontal") > 0;

        bool basic_attack = Input.GetButtonDown("BasicAttack");
        bool use_item = Input.GetButtonDown("UseItem");
        bool cycle_item = Input.GetButton("CycleItem");

        // Interpret player controls
        float ground_dist = gameObject.GetComponent<Collider>().bounds.extents.y;
        is_grounded = Physics.Raycast(transform.position, Vector3.down, ground_dist);

        if (is_grounded) {
            // Player is on the ground

            // Side-to-side controls
            if (move_l && !move_r) {
                // Move left
                speed_change.x = -1.0f * speed_run;
            } else if (move_r && !move_l) {
                // Move right
                speed_change.x = speed_run;
            } else if (!move_l && !move_r) {
                // Stop side-to-side movement
                // Force slowing down for first bit, then use drag

                // Player actively slows down while speed above slow_thresh
                float slow_thresh = 0.5f;
                // Player slows down at multiple of slow_rate. Must be < 0
                float slow_rate = -0.45f;

                if (Mathf.Abs(rb.velocity.x) > slow_thresh * speed_run) {
                    speed_change.x = slow_rate * rb.velocity.x;
                }
            }

            // Jumping controls
            if (move_up && !move_down) {
                // Jump
                speed_change.y = speed_jump;
            }

        } else {
            // Player is off the ground - use air controls

            // Early jump termination
            if (!move_up && (rb.velocity.y > 0.0f)) {
                speed_change.y = jump_fall_rate * rb.velocity.y;
            }

            // Side-to-side air controls
            float side_thresh = 0.5f;

            if (move_l && !move_r) {
                // Move left
                if (rb.velocity.x > side_thresh) {
                    speed_change.x = -0.75f * speed_run;
                } else {
                    speed_change.x = speed_run * (-2.0f / (1.0f + Mathf.Abs(rb.velocity.x)));
                }
            } if (move_r && !move_l) {
                // Move right
                if (rb.velocity.x < -1.0f * side_thresh) {
                    speed_change.x = 0.75f * speed_run;
                } else {
                    speed_change.x = speed_run * (2.0f / (1.0f + Mathf.Abs(rb.velocity.x)));
                }
            }
        }
    }

    // Used by the physics system
    void FixedUpdate()
    {
        // Update velocity
        rb.AddForce(speed_change, ForceMode.VelocityChange);
        speed_change = Vector3.zero;    // Reset speed changes for next update
    }

    /* SPECIFIED METHODS */
}
