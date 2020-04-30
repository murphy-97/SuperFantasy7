using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawCollision : MonoBehaviour
{
    public int damage;
    public int pushBack;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Vector3 enterDirection = collision.GetContact(0).point - transform.position;

            enterDirection.Normalize();

            enterDirection.z = 0;

            collision.gameObject.GetComponent<Rigidbody>().AddForce(enterDirection * pushBack);

            collision.gameObject.GetComponent<Player_Char>().Take_Damage(damage);
        }
    }
}
