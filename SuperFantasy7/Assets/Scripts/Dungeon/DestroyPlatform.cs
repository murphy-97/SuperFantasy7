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
    public float platformScale;

    private void Start()
    {

    }

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
        //yield return new WaitForSeconds(destroyDelay);
        float elapsedTime = 0;

        Vector3 newSize = new Vector3(platformScale, gameObject.transform.localScale.y, gameObject.transform.localScale.z);

        while (elapsedTime < destroyDelay)
        {
            gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, newSize, elapsedTime/destroyDelay);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }


}
