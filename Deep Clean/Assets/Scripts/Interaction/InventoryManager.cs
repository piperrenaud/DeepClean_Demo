using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Limits")]
    public int maxObjects = 6;
    public int maxPhotos = 6;

    [System.Serializable]
    public class CollectedEntry
    {
        public string itemID;
        public EvidenceType type;
        public string itemDescription;
        public string playerDialogue;
        public string explanation;
        public bool isPhoto;
        public bool isEvidence;
        public Sprite itemIcon;
    }

    public EnemyWander enemyWander;

    // Keep track of collected items
    private List<CollectedEntry> collectedItems = new List<CollectedEntry>();

    private Dictionary<string, Interactable> storedObjects = new Dictionary<string, Interactable>();

    private float suspicion = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }

    void Update()
    {
        if (enemyWander != null)
        {
            suspicion = enemyWander.GetSuspicion();
        }
    }

    public bool AddItem(string itemID, EvidenceType type, string description, string dialogue, string explanation, bool isPhoto = false, bool isEvidence = false, Sprite itemIcon = null)
    { 
        int objectCount = collectedItems.FindAll(i => !i.isPhoto).Count;
        int photoCount = collectedItems.FindAll(i => i.isPhoto).Count;

        if (!isPhoto && objectCount >= maxObjects)
        {
            Debug.Log("Object inventory full!. Cant take more objects.");
            return false;
        }
        else if (isPhoto && photoCount >= maxPhotos)
        {
            Debug.Log("Photo inventory full! Cannot take more photos.");
            return false;
        }

       collectedItems.Add(new CollectedEntry
       {
        itemID = itemID,
        type = type,
        itemDescription = description,
        playerDialogue = dialogue,
        explanation = explanation,
        isPhoto = isPhoto,
        isEvidence = isEvidence,
        itemIcon = itemIcon
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

        if (InventoryUIController.Instance != null)
        {
            InventoryUIController.Instance.RefreshUI();
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

    public bool IsFull(bool isPhoto)
    {
        if (isPhoto)
        {
            int photoCount = collectedItems.FindAll(i => i.isPhoto).Count;
            return photoCount >= maxPhotos;
        }
        else
        {
            int objectCount = collectedItems.FindAll(i => !i.isPhoto).Count;
            return objectCount >= maxObjects;
        }
    }

    public void RegisterItemObject(string itemID, Interactable obj)
    {
        if (!storedObjects.ContainsKey(itemID))
        {
            storedObjects[itemID] = obj;
        }
    }

    public Interactable GetOriginalObject(string itemID)
    {
        storedObjects.TryGetValue(itemID, out var obj);
        return obj;
    }

    public float GetSuspicion()
    {
        return suspicion;
    }

    public float GetCleanliness()
    {
        return GameManager.Instance.GetCurrentCleanliness();
    }
}
