using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [System.Serializable]
    public class CollectedEntry
    {
        public string itemID;
        public EvidenceType type;
        public string description;
        public string explanation;
        public bool isPhoto;
    }

    // Keep track of collected items
    private List<CollectedEntry> collectedItems = new List<CollectedEntry>();

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

    public void AddItem(string itemID, EvidenceType type, string description, string explanation, bool isPhoto = false)
    {
       collectedItems.Add(new CollectedEntry
       {
        itemID = itemID,
        type = type,
        description = description,
        explanation = explanation,
        isPhoto = isPhoto
       });
       Debug.Log("Collected evidence: " + itemID + "(" + type + ")");
    }

    public List<CollectedEntry> GetAllItems()
    {
        return collectedItems;
    }
}
