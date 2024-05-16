using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform[] teleportDestinations;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile"))
        {
            Transform freeDestination = FindFreeDestination();

            if (freeDestination != null)
            {
                Debug.Log("Телепорт совершен");
                other.transform.position = freeDestination.position;
            }
            else
            {
                Debug.LogWarning("Нет свободных позиций для телепортации!");
            }
        }
    }

    private Transform FindFreeDestination()
    {
        foreach (Transform destination in teleportDestinations)
        {
            Debug.LogWarning("destination");
            Collider[] colliders = Physics.OverlapSphere(destination.position, 0.01f);
            bool isFree = true;
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Tile"))
                {
                    isFree = false;
                    break;
                }
            }
            if (isFree)
            {
                return destination;
            }
        }

        return null;
    }
}