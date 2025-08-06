using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    //use names/IDs for tracking
    public List<string> takenObjectIDs = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //keep across scenes
        } else {
            Destroy(gameObject);
        }
    }

    public void AddItem(string objectID)
    {
        if (!takenObjectIDs.Contains(objectID))
            takenObjectIDs.Add(objectID);
    }

    public bool HasItem(string objectID)
    {
        return takenObjectIDs.Contains(objectID);
    }
}
