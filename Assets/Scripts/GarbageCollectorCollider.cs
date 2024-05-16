using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCollectorCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile") && other.name.Contains("_to_rm"))
        {
            Destroy(other.gameObject);
        }
    }
}
