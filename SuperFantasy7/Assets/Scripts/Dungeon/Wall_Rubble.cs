using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall_Rubble : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Randomize rotation
        transform.eulerAngles = new Vector3(
            Random.value * 360.0f,
            Random.value * 360.0f,
            Random.value * 360.0f
        );
    }

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime < 0.0f) {
            Destroy(gameObject);
        }
    }

    [SerializeField] private float lifetime;
}
