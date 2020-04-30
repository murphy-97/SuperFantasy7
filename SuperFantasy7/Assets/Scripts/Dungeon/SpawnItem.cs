using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    public Transform spawn;
    public Transform spawnPoint;
    public Transform endPoint;
    public float fallSpeed;
    public float spawnDelay;

    private GameObject item;
    private bool falling;
    private bool waiting;
    private float length;
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
        item = Instantiate(spawn, spawnPoint.position, spawnPoint.rotation).gameObject;
        falling = true;
        waiting = false;
        length = Vector3.Distance(spawnPoint.position, endPoint.position);
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(item == null)
        {
            if (!waiting)
            {
                StartCoroutine(delay());
            }
        }

        if (falling && item != null)
        {
            float distanceCovered = (Time.time - startTime) * fallSpeed;

            item.transform.position = Vector3.Lerp(spawnPoint.position, endPoint.position, distanceCovered/length);

            if (Vector3.Distance(item.transform.position, endPoint.position) <= 0.001f)
            {
                falling = false;
            }
        }
    }

    public IEnumerator delay()
    {
        waiting = true;
        yield return new WaitForSeconds(spawnDelay);
        item = Instantiate(spawn, spawnPoint.position, spawnPoint.rotation).gameObject;
        falling = true;
        waiting = false;
        length = Vector3.Distance(spawnPoint.position, endPoint.position);
        startTime = Time.time;
    }
}
