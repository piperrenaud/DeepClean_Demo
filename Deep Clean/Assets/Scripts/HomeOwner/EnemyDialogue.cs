using UnityEngine;
using System.Collections.Generic;

public class EnemyDialogue : MonoBehaviour
{
    public List<Interactable> GetDiscoveredObjects()
    {
        List<string> discoveredIDs = DiscoveryManager.Instance.GetAllDiscovered();
        List<Interactable> objects = new List<Interactable>();

        foreach (string id in discoveredIDs)
        {
            Interactable obj = FindObjectByID(id);
            if (obj != null) objects.Add(obj);
        }

        return objects;
    }

    private Interactable FindObjectByID(string id)
    {
        Interactable[] all = FindObjectsOfType<Interactable>(true);
        foreach (var obj in all)
        {
            if (obj.GetObjectID() == id)
                return obj;
        }
        return null;
    }
}
