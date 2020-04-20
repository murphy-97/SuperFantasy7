/*

The "Player_Char" entitiy represents the actual character object controlled by
the player in the current life. It is created on spawn and destroyed on death.

The equipment, ,ax health, and abilities accessible to the player are stored in
static variables, guaranteeing persistence between scene loads.

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to track currently equipped item
enum PC_Item {
	None = 0,
	Hook,		    // PC has grappling hook equipped
	Staff			// PC has staff of blasting equipped
}

// Used to store grapple hook data
[System.Serializable]
public struct Item_Grapple_Hook
{
    public float hook_target_range;
	public float max_spring;
    public float rate_spring;
    public float damp_spring;
    public GameObject target;
};

[System.Serializable]
public struct Item_Blasting_Staff
{
    public Blast_Proj proj;
    public float fire_delay;
    public float fire_timer;    // Must be set to < 0 initially
    public float velocity;
};

public class Player_Char : MonoBehaviour
{
    /* CLASS ATTRIBUTES (STATIC) */

    /* Player data */
    private static int health_max;
    private static int attack_basic_damage;    

    private static bool inv_has_grapple_hook = false;
    private static bool inv_has_blast_staff = false;

    /* CLASS METHODS (STATIC) */

    /* OBJECT ATTRIBUTES */

    // Combat data
    [Header("Combat Data")]
    [SerializeField] private int health_current;

    [Header("Inventory Data")]
    [SerializeField] private PC_Item item_current;
    [SerializeField] private Item_Grapple_Hook item_grapple_hook;
    [SerializeField] private Item_Blasting_Staff item_blasting_staff;
    [SerializeField] private float switch_delay;
    private float switch_timer = -1.0f;

    // Movement data
    [Header("Movement Data")]
    [SerializeField] private float speed_max_run;
    [SerializeField] private float speed_run;
    [SerializeField] private float speed_jump;
    [Range(-1,0)][SerializeField] private float jump_fall_rate; // Used to end jump
    private Rigidbody rb;
    [SerializeField] private bool is_grounded = false;  // Serialized for debugging
    [SerializeField] private bool on_wall_l = false;  // Serialized for debugging
    [SerializeField] private bool on_wall_r = false;  // Serialized for debugging
    [SerializeField] private float wall_jump_dist;
    Vector3 speed_change;
    private bool stop_running;
    private bool is_hooked = false;

    // Respawn data
    [Header("Respawn Data")]
    [SerializeField] private float respawn_timer;
    private Vector3 respawn_loc;
    private bool dead = false;

    /* UNITY BUILT-IN METHODS */

    // Start is called before the first frame update
    void Start()
    {
        respawn_loc = transform.position;
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
        bool cycle_item = Input.GetButton("CycleItem") && (switch_timer < 0.0f);

        // Manage timers
        if (item_blasting_staff.fire_timer >= 0.0f) {
            item_blasting_staff.fire_timer -= Time.deltaTime;
        }
        if (switch_timer >= 0.0f) {
            switch_timer -= Time.deltaTime;
        }

        // Check for item use
        if (use_item && !cycle_item) {
            switch (item_current) {

                case PC_Item.Hook:

                    if (is_hooked) {
                        Grapple_Release();
                    } else {
                        // Perform raycast from player to mouse target
                        RaycastHit hit;
                        Vector3 sourcePos = gameObject.transform.position;
                        Vector3 targetPos = Get_Mouse_World_Position();
                        targetPos.z = sourcePos.z;
                        Vector3 direction = targetPos - sourcePos;
                        
                        // TO DO: Put finite range on the grappling hook?
                        if(Physics.Raycast(sourcePos, direction, out hit)) {
                            // If hit a hook target, then grapple
                            if (hit.collider.gameObject.tag == "Hook_Target") {
                                Grapple_Fire(hit.collider.gameObject.GetComponent<Rigidbody>());
                            }
                        }
                    }

                    break;

                case PC_Item.Staff:

                    if (item_blasting_staff.fire_timer < 0.0f) {
                        // Fire staff of blasting
                        Vector3 sourcePos = gameObject.transform.position;
                        Vector3 targetPos = Get_Mouse_World_Position();
                        targetPos.z = sourcePos.z;
                        Vector3 direction = targetPos - sourcePos;

                        float angle = Mathf.Atan2(direction.y, direction.x);
                        Vector3 speed = new Vector3(
                            item_blasting_staff.velocity * Mathf.Cos(angle),
                            item_blasting_staff.velocity * Mathf.Sin(angle),
                            0.0f
                        );

                        Blast_Proj proj = Instantiate(item_blasting_staff.proj);

                        float pl_ext_x = gameObject.GetComponent<Collider>().bounds.extents.x;
                        float pl_ext_y = gameObject.GetComponent<Collider>().bounds.extents.y;
                        float pr_ext_x = proj.gameObject.GetComponent<Collider>().bounds.extents.x;
                        float pr_ext_y = proj.gameObject.GetComponent<Collider>().bounds.extents.y;
                        float spawn_rad = Mathf.Max(pl_ext_x, pl_ext_y) + Mathf.Max(pr_ext_x, pr_ext_y);

                        Vector2 offset = new Vector2(
                            spawn_rad * Mathf.Cos(angle),
                            spawn_rad * Mathf.Sin(angle)
                        );

                        proj.transform.position = new Vector3(
                            transform.position.x + offset.x,
                            transform.position.y + offset.y,
                            transform.position.z
                        );
                        proj.gameObject.GetComponent<Rigidbody>().AddForce(
                            speed,
                            ForceMode.VelocityChange
                        );
                    }

                    break;
            }
        } else if (cycle_item && !use_item) {
            Cycle_Item();
        }

        // Interpret player controls
        
        // Jumping controls (before is grounded for grapple hook swing)
        if (is_grounded || (on_wall_l && move_r && !move_l) || (on_wall_r && move_l && !move_r)) {
            // Jumping controls
            if (move_up && !move_down && rb.velocity.y <= 0.0f) {
                // Jump
                speed_change.y = speed_jump - rb.velocity.y;
            }
        }

        float ground_dist = gameObject.GetComponent<Collider>().bounds.extents.y;
        float wall_dist = gameObject.GetComponent<Collider>().bounds.extents.x + wall_jump_dist;

        is_grounded = Physics.Raycast(transform.position, Vector3.down, ground_dist) && !is_hooked;

        // Checks if player is touching wall at body center or at feet
        on_wall_l = Physics.Raycast(transform.position, Vector3.left, wall_dist);
        on_wall_l = on_wall_l || Physics.Raycast(
            transform.position + new Vector3(0.0f, -0.5f * gameObject.GetComponent<Collider>().bounds.extents.y, 0.0f),
            Vector3.left,
            wall_dist
        );

        on_wall_r = Physics.Raycast(transform.position, Vector3.right, wall_dist);
        on_wall_r = on_wall_r || Physics.Raycast(
            transform.position + new Vector3(0.0f, -0.5f * gameObject.GetComponent<Collider>().bounds.extents.y, 0.0f),
            Vector3.right,
            wall_dist
        );
        
        // Moving controls
        if (is_grounded) {
            // Player is on the ground

            // Side-to-side controls
            if (move_l && !move_r) {
                // Move left
                if (rb.velocity.x > -1.0f * speed_max_run) {
                    speed_change.x = -1.0f * speed_run;
                }
            } else if (move_r && !move_l) {
                // Move right
                if (rb.velocity.x < speed_max_run) {
                    speed_change.x = speed_run;
                }
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

        } else {
            // Player is off the ground - use air controls

            // Early jump termination
            if (!is_hooked && !move_up && (rb.velocity.y > 0.0f)) {
                speed_change.y = jump_fall_rate * rb.velocity.y;
            }

            // Side-to-side air controls
            float side_thresh = 0.5f;

            if (move_l && !move_r) {
                // Move left
                if (rb.velocity.x > -1.0f * speed_max_run) {
                    if (rb.velocity.x > side_thresh) {
                        speed_change.x = -0.75f * speed_run;
                    } else {
                        speed_change.x = speed_run * (-2.0f / (1.0f + Mathf.Abs(rb.velocity.x)));
                    }
                }
            } else if (move_r && !move_l) {
                // Move right
                if (rb.velocity.x < speed_max_run) {
                    if (rb.velocity.x < -1.0f * side_thresh) {
                        speed_change.x = 0.75f * speed_run;
                    } else {
                        speed_change.x = speed_run * (2.0f / (1.0f + Mathf.Abs(rb.velocity.x)));
                    }
                }
            }

            // Grapple hook: reel in or let out and update line renderer
            LineRenderer line = gameObject.GetComponent<LineRenderer>();
            if (is_hooked) {
                SpringJoint joint = gameObject.GetComponent<SpringJoint>();
                if (move_up && !move_down) {
                    // Reel in (move up the chain)
                    joint.spring += item_grapple_hook.rate_spring * Time.deltaTime;
                } else if (move_down && !move_up) {
                    // Let out (move down the chain)
                    joint.spring -= item_grapple_hook.rate_spring * Time.deltaTime;
                }
                joint.spring = Mathf.Clamp(joint.spring, 0.0f, item_grapple_hook.max_spring);

                // Manage line renderer
                line.SetPosition(1, item_grapple_hook.target.transform.position - transform.position);
            } else {
                // Hide line renderer
                line.SetPosition(1, Vector3.zero);
            }
        }
    }

    void OnTriggerEnter(Collider other) {

         if (other.tag == "Pickup_Grapple") {

             Debug.Log("Picked up Grapple Hook");
             inv_has_grapple_hook = true;
             Cycle_Item();
             Destroy(other.gameObject);

         } else if (other.tag == "Pickup_Staff") {

             Debug.Log("Picked up Staff of Blasting");
             inv_has_blast_staff = true;
             Cycle_Item();
             Destroy(other.gameObject);
         }
     }

    // Used by the physics system
    void FixedUpdate()
    {
        // Update velocity
        rb.AddForce(speed_change, ForceMode.VelocityChange);
        speed_change = Vector3.zero;    // Reset speed changes for next update

        // Update grapple hook
        if (is_hooked) {
            // Verify that grapple hook still has line of sight to target
            RaycastHit hit;
            Vector3 sourcePos = gameObject.transform.position;
            sourcePos.y -= 0.5f * gameObject.GetComponent<Collider>().bounds.extents.y;
            Vector3 targetPos = item_grapple_hook.target.transform.position;
            targetPos.z = sourcePos.z;
            Vector3 direction = targetPos - sourcePos;
            
            if(Physics.Raycast(sourcePos, direction, out hit)) {
                // If hit a hook target, then grapple
                if (hit.collider == null || (hit.collider.gameObject.tag != "Hook_Target" && hit.collider.gameObject.tag != "Doesnt_Break_Grapple")) {
                    Grapple_Release();
                }
            }
        }
    }

    /* SPECIFIED METHODS */
    public Vector3 Get_Mouse_World_Position() {
        RaycastHit hit;
        if (Physics.Raycast (Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            return hit.point;
        }
        return Vector3.zero;
    }

    // Grapple hook methods
    public void Grapple_Fire(Rigidbody target) {
        SpringJoint joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false; // Must be false to swing
        joint.spring = item_grapple_hook.max_spring;

        joint.connectedBody = target;
        joint.anchor = target.gameObject.transform.position;
        joint.connectedAnchor = joint.anchor;   // Must be the same fo swing
        joint.damper = item_grapple_hook.damp_spring;

        item_grapple_hook.target = target.gameObject;
        is_hooked = true;
    }

    public void Grapple_Release() {
        SpringJoint joint = gameObject.GetComponent<SpringJoint>();
        joint.spring = 0.0f;
        joint.connectedBody = null;
        item_grapple_hook.target = null;
        is_hooked = false;
        is_grounded = true; // Allows player to fly upward when releasing
        Destroy(joint);
    }

    public void Cycle_Item() {
        switch (item_current) {
            
            case PC_Item.None:
                if (inv_has_grapple_hook) {
                    Debug.Log("Switching to Grappling Hook...");
                    item_current = PC_Item.Hook;
                    switch_timer = switch_delay;
                } else if (inv_has_blast_staff) {
                    Debug.Log("Switching to Staff of Blasting...");
                    item_current = PC_Item.Staff;
                    switch_timer = switch_delay;
                }
                break;

            case PC_Item.Hook:
                if (inv_has_blast_staff) {
                    if (is_hooked) {
                        Grapple_Release();
                    }
                    Debug.Log("Switching to Staff of Blasting...");
                    item_current = PC_Item.Staff;
                    switch_timer = switch_delay;
                }
                break;

            case PC_Item.Staff:
                if (inv_has_grapple_hook) {
                    Debug.Log("Switching to Grappling Hook...");
                    item_current = PC_Item.Hook;
                    switch_timer = switch_delay;
                }
                break;
        }
    }
}
