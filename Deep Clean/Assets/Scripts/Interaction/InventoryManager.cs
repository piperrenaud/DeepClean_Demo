using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int maxItems = 32;

    [System.Serializable]
    public class CollectedEntry
    {
        public string itemID;
        public EvidenceType type;
        public string itemDescription;
        public string playerDialogue;
        public string explanation;
        public bool isPhoto;
        public Sprite itemIcon;
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

    public bool AddItem(string itemID, EvidenceType type, string description, string dialogue, string explanation, bool isPhoto = false, Sprite itemIcon = null)
    { 
        if (collectedItems.Count >= maxItems)
        {
            Debug.Log("Inventory full! cannot take more items.");
            return false; //reject adding
        }

       collectedItems.Add(new CollectedEntry
       {
        itemID = itemID,
        type = type,
        itemDescription = description,
        playerDialogue = dialogue,
        explanation = explanation,
        isPhoto = isPhoto,
        itemIcon = itemIcon != null ? itemIcon : null
       });

       Debug.Log("Collected evidence: " + itemID + "(" + type + ")");
       
       if (InventoryUIController.Instance != null)
            InventoryUIController.Instance.RefreshUI();

       return true;
    }

    public void RemoveItem(string itemID)
    {
        for (int i = 0; i < collectedItems.Count; i++)
        {
            if (collectedItems[i].itemID == itemID)
            {
                collectedItems.RemoveAt(i);
                break;
            }
        }
    }

    public List<CollectedEntry> GetAllItems()
    {
        return collectedItems;
    }

    public void UpdateItemWithExplanation(string itemID, string explanation)
    {
        foreach (var entry in collectedItems)
        {
            if (entry.itemID == itemID)
            {
                entry.explanation = explanation;

                //add explanation to description if not already there
                if (!string.IsNullOrEmpty(explanation) && !entry.itemDescription.Contains("Explanation:"))
                {
                    entry.itemDescription += "\n\nExplanation: " + explanation;
                }

                //refresh UI so player sees update
                if (InventoryUIController.Instance != null)
                    InventoryUIController.Instance.RefreshUI();

                Debug.Log($"Updated {itemID} with explanation.");
                return;
            }
        }
    }
}
