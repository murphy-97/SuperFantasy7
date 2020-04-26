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
    public float spikeDuration; //how long the spikes are deployed
    public float spikeSpeed; //how long untill spikes are fully extedned
    public bool isSpiked; //determines if the spikes are already activated
    public bool inSpikes; //determines if the player is in the spike damage range
    public Transform goal1; //how far the spikes will move
    public Transform goal2; //how far the spikes will move
    public Transform origin1; //original spike pos
    public Transform origin2; //original spike pos
    private Transform spike1; //the first spike
    private Transform spike2; //the second spike
    

    //Start is called before the first frame update
    private void Start()
    {
        spike1 = transform.GetChild(0);
        spike2 = transform.GetChild(1);
        //move1 = new Vector3(spike1.position.x, spike1.position.y + 0.75f, 0);
        //move2 = new Vector3(spike2.position.x, spike2.position.y + 0.75f, 0);
        isSpiked = false;
        inSpikes = false;
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
        float elapsedTime = 0;

        /*Vector3 startSpike1 = spike1.position;
        Vector3 startSpike2 = spike2.position;*/

        yield return new WaitForSeconds(spikeDelay);
        while (elapsedTime <= spikeSpeed)
        {
            spike1.transform.position = Vector3.Lerp(spike1.position, goal1.position, (elapsedTime / spikeSpeed));
            spike2.transform.position = Vector3.Lerp(spike2.position, goal2.position, (elapsedTime / spikeSpeed));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        elapsedTime = 0;
        yield return new WaitForSeconds(spikeDuration);
        while (elapsedTime <= spikeSpeed)
        {
            spike1.transform.position = Vector3.Lerp(spike1.position, origin1.position, (elapsedTime / spikeSpeed));
            spike2.transform.position = Vector3.Lerp(spike2.position, origin2.position, (elapsedTime / spikeSpeed));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isSpiked = false;
        /*yield return new WaitForSeconds(spikeDelay);
        spike1.Translate(move, Space.World);
        spike2.Translate(move, Space.World);*/
    }
}
