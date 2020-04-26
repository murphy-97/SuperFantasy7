/**
 * TmpMove exsits just to test out my obsticals
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TmpMove : MonoBehaviour
{
    public int health_max;
    public int health;

    private void Start()
    {
        health = health_max;
    }

    public void Take_Damage(int d)
    {
        health -= d;
    }
}
