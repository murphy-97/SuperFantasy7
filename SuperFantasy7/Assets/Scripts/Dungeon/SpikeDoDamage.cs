using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDoDamage : MonoBehaviour
{
    public int damage;
    private bool invincible = false;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            if (!invincible)
            {
                collider.gameObject.GetComponent<Player_Char>().Take_Damage(damage);//Player_Char TmpMove
                invincible = true;
            }
        }

    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            invincible = false;
        }
    }
}
