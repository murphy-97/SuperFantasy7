/*
 * The spikes will appear some amount of time after the player has 
 * stepped on them and spike the player rather than just being there.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableSpikes : MonoBehaviour
{
    public int damage;
    public int pushBack;
    public float spikeDelay; //how much time will pass before the spikes appear
    public float spikeDuration; //how long the spikes are deployed
    public float spikeSpeed; //how long untill spikes are fully extedned
    public Transform goal1; //how far the spikes will move
    public Transform goal2; //how far the spikes will move
    public Transform origin1; //original spike pos
    public Transform origin2; //original spike pos
    private Transform spike1; //the first spike
    private Transform spike2; //the second spike
    private bool isSpiked; //determines if the spikes are activated
    private bool inSpikes; //determines if the player is in the spike damage range


    //Start is called before the first frame update
    private void Start()
    {
        spike1 = transform.GetChild(0);
        spike2 = transform.GetChild(1);
        isSpiked = false;
        inSpikes = false;
    }

    //called when the player enters the trigger zone
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.name == "Player" && !isSpiked)
        {
            StartCoroutine(SpikeDelay(collider));
            isSpiked = true;
        }
        if (collider.gameObject.name == "Player")
        {
            inSpikes = true;
        }
    }

    //makes sure that the player in within spike range
    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            inSpikes = false;
        }
    }

    //moves the spikes after "spikeDelay" seconds
    private IEnumerator SpikeDelay(Collider collider)
    {
        float elapsedTime = 0;

        yield return new WaitForSeconds(spikeDelay);
        while (elapsedTime <= spikeSpeed)
        {
            SpikeEm(collider);
            spike1.transform.position = Vector3.Lerp(spike1.position, goal1.position, (elapsedTime / spikeSpeed));
            spike2.transform.position = Vector3.Lerp(spike2.position, goal2.position, (elapsedTime / spikeSpeed));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        elapsedTime = 0;
        while (elapsedTime <= spikeDuration)
        {
            SpikeEm(collider);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        elapsedTime = 0;
        while (elapsedTime <= spikeSpeed)
        {
            spike1.transform.position = Vector3.Lerp(spike1.position, origin1.position, (elapsedTime / spikeSpeed));
            spike2.transform.position = Vector3.Lerp(spike2.position, origin2.position, (elapsedTime / spikeSpeed));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isSpiked = false;
    }

    /**
     * pushes the player back and deals damage
     */
    private void SpikeEm(Collider collider)
    {
        if (inSpikes)
        {
            Vector3 pushDirection = collider.transform.position - transform.position;

            pushDirection.Normalize();

            pushDirection.z = 0;

            collider.gameObject.GetComponent<Rigidbody>().AddForce(pushDirection * pushBack);

            collider.gameObject.GetComponent<Player_Char>().Take_Damage(damage);

            inSpikes = false;
        }
    }
}
