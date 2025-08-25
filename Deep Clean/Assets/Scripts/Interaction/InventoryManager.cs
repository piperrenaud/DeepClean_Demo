using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Keep track of collected items
    private HashSet<string> collectedItems = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(string itemID)
    {
        if (!collectedItems.Contains(itemID))
        {
            collectedItems.Add(itemID);
            Debug.Log("Collected: " + itemID);
        }
    }

    public bool HasItem(string itemID)
    {
        return collectedItems.Contains(itemID);
    }

    public HashSet<string> GetAllItems()
    {
        return collectedItems;
    }
}
