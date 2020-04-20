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
        if (col.gameObject.tag == "Breakable_Wall") {
            // Spawn rubble evenly distributed within radius
            for (int i = 0; i < rubble_count; i++) {

                float u = Random.value + Random.value;
                float r = rubble_radius * (u > 1.0f ? 2.0f - u : u);
                float a = Random.value * 2.0f * Mathf.PI;

                float x = r * Mathf.Cos(a);
                float y = r * Mathf.Sin(a);

                GameObject rubble = Instantiate(rubble_object);
                rubble.transform.position = new Vector3(
                    transform.position.x + x,
                    transform.position.y + y,
                    Random.Range(rubble_z_range.x, rubble_z_range.y)
                );
            }

            // Destroy wall
            Destroy(col.gameObject);
        }
    }

    // Object properties
    [SerializeField] private float lifetime;
    [SerializeField] private float fade_time;
    private Vector3 fade_rate;
    [SerializeField] private GameObject rubble_object;
    [SerializeField] private float rubble_radius;
    [SerializeField] private int rubble_count;
    [SerializeField] private Vector2 rubble_z_range;

    // Object methods
}
