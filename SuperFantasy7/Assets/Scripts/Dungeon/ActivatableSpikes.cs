/*
 * The spikes will appear some amount of time after the player has 
 * stepped on them and spike the player rather than just being there.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableSpikes : MonoBehaviour
{
    public float spikeDelay; //how much time will pass before the spikes appear
    public bool isSpiked; //determines if the spikes are already activated
    private Transform spike1; //the first spike
    private Transform spike2; //the second spike
    private Vector3 move; //how far the spikes will move

    //Start is called before the first frame update
    private void Start()
    {
        spike1 = transform.GetChild(0);
        spike2 = transform.GetChild(1);
        move = new Vector3(0f, 0.75f, 0f);
        isSpiked = false;
    }

    //called when the player enters the trigger zone
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.name == "Player" && !isSpiked)
        {
            StartCoroutine(SpikeDelay());
            isSpiked = true;
        }
    }

    //moves the spikes after "spikeDelay" seconds
    private IEnumerator SpikeDelay()
    {
        yield return new WaitForSeconds(spikeDelay);
        spike1.Translate(move, Space.World);
        spike2.Translate(move, Space.World);
    }
}
