using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectPrefabs;

    void Start()
{
    Debug.Log("Spawning taken objects...");

    foreach (string id in InventoryManager.Instance.takenObjectIDs)
    {
        Debug.Log("Looking for object with ID: " + id);

        foreach (GameObject prefab in objectPrefabs)
        {
            Interactable interact = prefab.GetComponent<Interactable>();
            if (interact != null)
            {
                Debug.Log("Checking prefab: " + prefab.name + " with ID: " + interact.objectID);
                if (interact.objectID == id)
                {
                    Instantiate(prefab, Vector3.zero, Quaternion.identity);
                    Debug.Log("Spawned: " + prefab.name);
                }
            }
        }
    }
}
}
