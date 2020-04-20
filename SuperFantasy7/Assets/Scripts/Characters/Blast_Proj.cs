using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blast_Proj : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        fade_rate = new Vector3(
            transform.localScale.x / fade_time,
            transform.localScale.y / fade_time,
            transform.localScale.z / fade_time
        );
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime >= 0.0f) {
            lifetime -= Time.deltaTime;
        } else if (fade_time >= 0.0f) {
            fade_time -= Time.deltaTime;
            transform.localScale = new Vector3(
                transform.localScale.x - (fade_rate.x * Time.deltaTime),
                transform.localScale.y - (fade_rate.y * Time.deltaTime),
                transform.localScale.z - (fade_rate.z * Time.deltaTime)
            );
        } else {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter (Collision col)
    {
        Debug.Log("Collided with " + col.gameObject.tag);
        if (col.gameObject.tag == "Breakable_Wall") {
            // Spawn rubble
            Destroy(col.gameObject);
        }
    }

    // Object properties
    [SerializeField] private float lifetime;
    [SerializeField] private float fade_time;
    private Vector3 fade_rate;

    // Object methods
}
