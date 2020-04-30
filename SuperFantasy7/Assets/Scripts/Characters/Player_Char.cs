/*

The "Player_Char" entitiy represents the actual character object controlled by
the player in the current life. It is created on spawn and destroyed on death.

The equipment, ,ax health, and abilities accessible to the player are stored in
static variables, guaranteeing persistence between scene loads.

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public float use_delay;
    public float use_timer;     // Must be set to < 0 initially
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
    public static string Format_Time(float t, int precision)
     {
        int minutes = (int)t / 60;
        float seconds = (float)System.Math.Round(t % 60.0f, precision);

        string output = (
            (minutes < 10 ? "0" : "" ) +
            minutes +
            ":" +
            (seconds < 10 ? "0" : "") +
            seconds
        );
        return output;
     }

    /* OBJECT ATTRIBUTES */

    [SerializeField] private Transform model;
    [SerializeField] private Animator animator;
    [Range(-1,1)][SerializeField] private int heading;

    // Combat data
    [Header("Combat Data")]
    [SerializeField] private int health;

    [Header("Inventory Data")]
    [SerializeField] private PC_Item item_current;
    [SerializeField] private Item_Grapple_Hook item_grapple_hook;
    [SerializeField] private Item_Blasting_Staff item_blasting_staff;
    [SerializeField] private float switch_delay;
    private float switch_timer = -1.0f;

    [SerializeField] private float speed_boost_mult;
    [SerializeField] private float speed_boost_time;
    private float speed_boost_timer = -1.0f;

    [SerializeField] private float jump_boost_mult;
    [SerializeField] private float jump_boost_time;
    private float jump_boost_timer = -1.0f;

    [Header("Item Display Data")]
    [SerializeField] private GameObject hook_object;
    [SerializeField] private GameObject hook_pivot;
    [SerializeField] private float hook_pivot_min;
    [SerializeField] private float hook_pivot_max;
    private float hook_pivot_default;
    [SerializeField] private GameObject staff_object;
    [SerializeField] private Vector2 staff_offset;

    // Movement data
    [Header("Movement Data")]
    [SerializeField] private Dungeon dungeon;
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
    [SerializeField] private float respawn_time_penalty;
    private Vector3 respawn_loc;
    private bool dead = false;

    [Header("UI Data")]
    [SerializeField] private Text equipped_item;
    [SerializeField] private Text elapsed_time;
    [SerializeField] private int clock_precision;

    [SerializeField] private Text end_notice;
    [SerializeField] private GameObject end_screen;
    [SerializeField] private Text end_time;

    [SerializeField] private Image health_bar;

    private float level_timer = 0.0f;
    private bool run_timer = false;

    /* UNITY BUILT-IN METHODS */

    void Awake()
    {
        health_max = health;
        
        // Reset inventory
        // Originally stored static to manage inventory across dungeons
        // Multiple dungeon feature was removed
        inv_has_grapple_hook = false;
        inv_has_blast_staff = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        hook_pivot_default = hook_pivot.transform.eulerAngles.x;
        respawn_loc = transform.position;
        rb = gameObject.GetComponent<Rigidbody>();
        Start_Timer();
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
        if (item_grapple_hook.use_timer >= 0.0f) {
            item_grapple_hook.use_timer -= Time.deltaTime;
        }
        if (item_blasting_staff.fire_timer >= 0.0f) {
            item_blasting_staff.fire_timer -= Time.deltaTime;
        }
        if (switch_timer >= 0.0f) {
            switch_timer -= Time.deltaTime;
        }
        if (run_timer) {
            level_timer += Time.deltaTime;
            elapsed_time.text = Format_Time(level_timer, clock_precision);
        }
        if (speed_boost_timer >= 0.0f) {
            speed_boost_timer -= Time.deltaTime;
        }
        if (jump_boost_timer >= 0.0f) {
            jump_boost_timer -= Time.deltaTime;
        }

        // Check for item use
        if (use_item && !cycle_item) {
            switch (item_current) {

                case PC_Item.Hook:

                    if (is_hooked && item_grapple_hook.use_timer < 0.0f) {
                        Grapple_Release();
                        item_grapple_hook.use_timer = item_grapple_hook.use_delay;
                    } else if (item_grapple_hook.use_timer < 0.0f) {
                        // Perform raycast from player to mouse target
                        RaycastHit hit;
                        Vector3 sourcePos = hook_object.gameObject.transform.position;
                        Vector3 targetPos = Get_Mouse_World_Position();
                        targetPos.z = sourcePos.z;
                        Vector3 direction = targetPos - sourcePos;
                        
                        heading = (targetPos.x < sourcePos.x ? -1 : 1);

                        // TO DO: Put finite range on the grappling hook?
                        if(Physics.Raycast(sourcePos, direction, out hit)) {
                            // If hit a hook target, then grapple
                            if (hit.collider.gameObject.tag == "Hook_Target") {
                                Grapple_Fire(hit.collider.gameObject.GetComponent<Rigidbody>());
                                item_grapple_hook.use_timer = item_grapple_hook.use_delay;
                            }
                        }
                    }

                    break;

                case PC_Item.Staff:

                    if (item_blasting_staff.fire_timer < 0.0f) {
                        // Fire staff of blasting
                        Vector3 sourcePos = staff_object.gameObject.transform.position;
                        Vector3 targetPos = Get_Mouse_World_Position();
                        targetPos.z = sourcePos.z;
                        Vector3 direction = targetPos - sourcePos;
                        
                        heading = (targetPos.x < sourcePos.x ? -1 : 1);

                        float angle = Mathf.Atan2(direction.y, direction.x);
                        Vector3 speed = new Vector3(
                            item_blasting_staff.velocity * Mathf.Cos(angle),
                            item_blasting_staff.velocity * Mathf.Sin(angle),
                            0.0f
                        );

                        Blast_Proj proj = Instantiate(item_blasting_staff.proj);

                        /*
                        float pl_ext_x = gameObject.GetComponent<Collider>().bounds.extents.x;
                        float pl_ext_y = gameObject.GetComponent<Collider>().bounds.extents.y;
                        float pr_ext_x = proj.gameObject.GetComponent<Collider>().bounds.extents.x;
                        float pr_ext_y = proj.gameObject.GetComponent<Collider>().bounds.extents.y;
                        float spawn_rad = Mathf.Max(pl_ext_x, pl_ext_y) + Mathf.Max(pr_ext_x, pr_ext_y);

                        Vector2 offset = new Vector2(
                            spawn_rad * Mathf.Cos(angle),
                            spawn_rad * Mathf.Sin(angle)
                        );

                        if (!staff_use_offset) {
                            offset = Vector2.zero;
                        }
                        */

                        proj.transform.position = new Vector3(
                            transform.position.x + (staff_offset.x * (heading < 0 ? -1.0f : 1.0f)),
                            transform.position.y + staff_offset.y,
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
                speed_change.y = (speed_jump - rb.velocity.y) * (jump_boost_timer > 0.0f ? jump_boost_mult : 1.0f);
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
                heading = -1;
                if (rb.velocity.x > -1.0f * speed_max_run) {
                    speed_change.x = -1.0f * speed_run * (speed_boost_timer > 0.0f ? speed_boost_mult : 1.0f);
                }
            } else if (move_r && !move_l) {
                // Move right
                heading = 1;
                if (rb.velocity.x < speed_max_run) {
                    speed_change.x = speed_run * (speed_boost_timer > 0.0f ? speed_boost_mult : 1.0f);
                }
            } else if (!move_l && !move_r) {
                // Stop side-to-side movement
                // Force slowing down for first bit, then use drag

                // Player actively slows down while speed above slow_thresh
                float slow_thresh = 0.5f;
                // Player slows down at multiple of slow_rate. Must be < 0
                float slow_rate = -0.45f;

                if (Mathf.Abs(rb.velocity.x) > slow_thresh * speed_run) {
                    speed_change.x = slow_rate * rb.velocity.x * (speed_boost_timer > 0.0f ? speed_boost_mult : 1.0f);
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
                heading = -1;
                if (rb.velocity.x > -1.0f * speed_max_run) {
                    if (rb.velocity.x > side_thresh) {
                        speed_change.x = -0.75f * speed_run * (speed_boost_timer > 0.0f ? speed_boost_mult : 1.0f);
                    } else {
                        speed_change.x = speed_run * (-2.0f / (1.0f + Mathf.Abs(rb.velocity.x)))  * (speed_boost_timer > 0.0f ? speed_boost_mult : 1.0f);
                    }
                }
            } else if (move_r && !move_l) {
                // Move right
                heading = 1;
                if (rb.velocity.x < speed_max_run) {
                    if (rb.velocity.x < -1.0f * side_thresh) {
                        speed_change.x = 0.75f * speed_run * (speed_boost_timer > 0.0f ? speed_boost_mult : 1.0f);
                    } else {
                        speed_change.x = speed_run * (2.0f / (1.0f + Mathf.Abs(rb.velocity.x))) * (speed_boost_timer > 0.0f ? speed_boost_mult : 1.0f);
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
                line.SetPosition(0, hook_object.transform.position - transform.position);
                line.SetPosition(1, item_grapple_hook.target.transform.position - transform.position);

                // Aim hook pivot
                float hook_angle = Mathf.Rad2Deg * Mathf.Atan2(
                    hook_object.transform.position.y - item_grapple_hook.target.transform.position.y,
                    hook_object.transform.position.x - item_grapple_hook.target.transform.position.x
                );
                hook_angle = (hook_angle + 720.0f) % 360.0f;    // Standardize angle
                if (hook_angle >= 270.0f || hook_angle <= 90.0f) {
                    if (hook_angle > 180.0f) {
                        hook_angle = 360.0f - hook_angle;
                    } else {
                        hook_angle *= -1.0f;
                    }
                } else {
                    hook_angle -= 180.0f;
                }
                if (heading > 0) {
                    hook_angle *= -1.0f;
                }

                hook_pivot.transform.eulerAngles = new Vector3(
                    Mathf.Clamp(hook_angle, hook_pivot_min, hook_pivot_max),
                    hook_pivot.transform.eulerAngles.y,
                    hook_pivot.transform.eulerAngles.z
                );
            } else {
                // Hide line renderer
                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, Vector3.zero);

                // Reset hook pivot
                hook_pivot.transform.eulerAngles = new Vector3(
                    hook_pivot_default,
                    hook_pivot.transform.eulerAngles.y,
                    hook_pivot.transform.eulerAngles.z
                );
            }
        }

        // Update model facing direction
        model.transform.localScale = new Vector3(
            model.transform.localScale.x,
            model.transform.localScale.y,
            Mathf.Abs(model.transform.localScale.z) * (heading < 0 ? -1.0f : 1.0f)
        );
        // Update animator
        animator.SetInteger("speed", (((move_l || move_r) && is_grounded) ? 1 : 0));
        animator.SetBool("grounded", (is_grounded || on_wall_l || on_wall_r));
        animator.SetBool("falling", rb.velocity.y < 0.0f);
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

         } else if (other.tag == "Pickup_Finish") {

            run_timer = false;
            // Report time
            string message = "Finished the dungeon!\n";
            message += "Dungeon Seed: " + dungeon.Get_Seed() + "\n";
            message += "Dungeon Tmie: " + Format_Time(level_timer, clock_precision) + "\n";
            Debug.Log(message);

            end_time.text = Format_Time(level_timer, clock_precision);

            if (Main_Menu.Consider_Best_Time(level_timer, dungeon.Get_Seed())) {
                Debug.Log("NEW BEST TIME");
                end_notice.text += " New Best Time!";
            }

            end_screen.SetActive(true);
            Destroy(other.gameObject);
             
         } else if (other.tag == "Pickup_Speed") {

             Debug.Log("Picked up a speed boost");
             speed_boost_timer = speed_boost_time;
             Destroy(other.gameObject);

         } else if (other.tag == "Pickup_Jump") {

             Debug.Log("Picked up a jump boost");
             jump_boost_timer = jump_boost_time;
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
            //Vector3 sourcePos = hook_object.gameObject.transform.position;
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

        // Update health UI
        if (health_bar != null) {
            health_bar.fillAmount = Mathf.Clamp((float)health / (float)health_max, 0.0f, 1.0f);
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

    public void Start_Timer() {
        run_timer = true;
    }

    public void Take_Damage(int d) {
        if (health > 0) {
            health -= d;
            if (health <= 0) {
                transform.position = respawn_loc;
                health = health_max;
                level_timer += respawn_time_penalty;
            }
        }
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
        if (switch_timer >= 0.0f) {
            return;
        }

        switch (item_current) {
            
            case PC_Item.None:
                if (inv_has_grapple_hook) {
                    Debug.Log("Switching to Grappling Hook...");
                    equipped_item.text = "Grapple Hook";
                    item_current = PC_Item.Hook;
                    switch_timer = switch_delay;
                } else if (inv_has_blast_staff) {
                    Debug.Log("Switching to Staff of Blasting...");
                    equipped_item.text = "Blasting Staff";
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
                    equipped_item.text = "Blasting Staff";
                    item_current = PC_Item.Staff;
                    switch_timer = switch_delay;
                }
                break;

            case PC_Item.Staff:
                if (inv_has_grapple_hook) {
                    Debug.Log("Switching to Grappling Hook...");
                    equipped_item.text = "Grapple Hook";
                    item_current = PC_Item.Hook;
                    switch_timer = switch_delay;
                }
                break;
        }

        hook_object.gameObject.SetActive(item_current == PC_Item.Hook);
        staff_object.gameObject.SetActive(item_current == PC_Item.Staff);
    }

    public void Load_Scene(string scene) {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void Replay_Dungeon(string scene) {
        Dungeon.Set_Seed(dungeon.Get_Seed());
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
