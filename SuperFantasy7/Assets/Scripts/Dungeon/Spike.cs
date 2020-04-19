using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public int damge;
    public int pushBack;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Player")
        {
            Vector3 enterDirection = collision.GetContact(0).point - transform.position;

            enterDirection.Normalize();

            enterDirection.z = 0;

            collision.gameObject.GetComponent<Rigidbody>().AddForce(enterDirection*pushBack);
        }
    }
}
