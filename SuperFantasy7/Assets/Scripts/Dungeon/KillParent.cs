using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillParent : MonoBehaviour
{
    void OnDestroy()
    {
        Destroy(transform.parent.gameObject);
    }
}
