/*
 * The DestroyPlatform script will destroy the platform that it is applied 
 * to after a specified amout of time
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlatform : MonoBehaviour
{

    public float destroyDelay; //time in seconds before the platform is destroyed.

    /*Collision Events*/
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Player")
        {
            StartCoroutine(DestroyDelay());
        }
    }

    /*Destroy this game object after a "destroyDelay" amount of seconds*/
    IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }


}
