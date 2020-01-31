/*

The "Player_Char" entitiy represents the actual character object controlled by
the player in the current life. It is created on spawn and destroyed on death.
The equipment, health, and abilities accessible to the player are determined by
the state of the Player entity defined in Player.cs.

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Character : MonoBehaviour
{
    /* CLASS ATTRIBUTES (STATIC) */

    /* CLASS METHODS (STATIC) */

    /* OBJECT ATTRIBUTES */

    // Respawn data
    [SerializeField] private float respawn_timer;
    private Transform respawn_loc;
    private bool dead = false;

    /* UNITY BUILT-IN METHODS */

    // Start is called before the first frame update
    void Start()
    {
        
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
        bool move_up = !dead && Input.GetAxis("Vertical") > 0;
		bool move_down = !dead && Input.GetAxis("Vertical") > 0;
		bool move_l = !dead && Input.GetAxis("Horizontal") < 0;
		bool move_r = !dead && Input.GetAxis("Horizontal") < 0;

        bool basic_attack = Input.GetButtonDown("BasicAttack");
        bool use_item = Input.GetButtonDown("UseItem");
        bool cycle_item = Input.GetButton("CycleItem");
    }

    /* SPECIFIED METHODS */
}
