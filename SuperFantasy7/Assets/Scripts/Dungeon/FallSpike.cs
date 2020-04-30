using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallSpike : MonoBehaviour
{
    public float fallSpeedDown;
    public float fallSpeedUp;
    public float fallDelay;
    public Transform startPoint;
    public Transform endPoint;
    public Transform spike;

    private bool deployed;
    //private bool activate;
    private bool away;
    private float length;
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
        deployed = false;
        //activate = false;
        away = true;
        length = Vector3.Distance(startPoint.position, endPoint.position);
    }

    // Update is called once per frame
    void Update()
    {
        if(deployed)
        {
            //make spike fall
            if (away)
            {
                float distanceCovered = (Time.time - startTime) * fallSpeedDown;

                spike.position = Vector3.Lerp(startPoint.position, endPoint.position, distanceCovered / length);
                
            }else
            {
                float distanceCovered = (Time.time - startTime) * fallSpeedUp;

                spike.position = Vector3.Lerp(endPoint.position, startPoint.position, distanceCovered / length);
            }

            //check the position of the spike
            if (Vector3.Distance(spike.position, endPoint.position) <= 0.001 && away)
            {
                away = false;
                startTime = Time.time;
            }
            if (Vector3.Distance(spike.position, startPoint.position) <= 0.001 && !away)
            {
                away = true;
                deployed = false;
                gameObject.GetComponent<BoxCollider>().enabled = !gameObject.GetComponent<BoxCollider>().enabled;
                //activate = false;
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.name == "Player")
        {
            /*if (!activate)
            {
                activate = true;*/
            gameObject.GetComponent<BoxCollider>().enabled = !gameObject.GetComponent<BoxCollider>().enabled;
            StartCoroutine(delay());
            //}
        }
    }

    private IEnumerator delay()
    {
        yield return new WaitForSeconds(fallDelay);
        deployed = true;
        startTime = Time.time;
    }


}
