using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallScript : MonoBehaviour
{

    public float destroyDelay;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Player")
        {
            StartCoroutine(DestroyDelay());
        }
    }

    IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }


}
