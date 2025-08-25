using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ItemEntry
    {
        public string itemID;
        public GameObject prefab;
        public Transform spawnPoint;
    }

    public List<ItemEntry> itemsToSpawn;

    void Start()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("No InventoryManager found in scene 2!");
            return;
        }

        HashSet<string> collected = InventoryManager.Instance.GetAllItems();
        Debug.Log("Items in inventory at scene load: " + collected.Count);

        foreach (var entry in itemsToSpawn)
        {
            if (collected.Contains(entry.itemID))
            {
                Debug.Log("Spawning: " + entry.itemID);
                Instantiate(entry.prefab, entry.spawnPoint.position, entry.spawnPoint.rotation);
            }
        }
    }
}
