/*
 * The Spike defines how the spike object behaves. The spike will do damage to 
 * the player and then bounce them back.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public int damage;   //damage done to the player
    public int pushBack;    //how much force the spikes will push the player back.

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Player")
        {
            Vector3 enterDirection = collision.GetContact(0).point - transform.position;

            enterDirection.Normalize();

            enterDirection.z = 0;

            collision.gameObject.GetComponent<Rigidbody>().AddForce(enterDirection*pushBack);

            collision.gameObject.GetComponent<Player_Char>().Take_Damage(damage); 
        }
    }
}
