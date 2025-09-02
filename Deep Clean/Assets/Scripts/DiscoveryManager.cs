using UnityEngine;
using System.Collections.Generic;

public class DiscoveryManager : MonoBehaviour
{
    public static DiscoveryManager Instance;

    private HashSet<string> discoveredObjects = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterDiscovery(Interactable obj)
    {
        if (!discoveredObjects.Contains(obj.GetObjectID()))
        {
            discoveredObjects.Add(obj.GetObjectID());
            Debug.Log("Discovered: "+ obj.GetObjectID());
        }
    }

    public bool IsDiscovered(string objectID)
    {
        return discoveredObjects.Contains(objectID);
    }

    public List<string> GetAllDiscovered()
    {
        return new List<string>(discoveredObjects);
    }
}
