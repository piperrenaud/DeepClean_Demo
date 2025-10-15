using UnityEngine;
using System.Collections.Generic;

public class EvidenceScoringManager : MonoBehaviour
{
    public static EvidenceScoringManager Instance;

    [HideInInspector] public float totalPoints = 0f;

    private HashSet<string> scoredFolderItems = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //call when player takes/photographs object
    public void RegisterInteraction(Interactable item, EvidenceType actionType)
    {
        if (item == null) return;

        float points = 0f;

        switch (item.evidenceType)
        {
            case EvidenceType.Take:
                points = actionType == EvidenceType.Take ? 1f : 0.5f;
                break;

            case EvidenceType.Photo:
                points = actionType == EvidenceType.Photo ? 1f : 0.5f;
                break;
            case EvidenceType.RedHerring:
            case EvidenceType.None:
            default:
                points = 0f;
                break;
        }

        totalPoints += points;
        Debug.Log($"Interaction: {item.objectID}, +{points} points. Total: {totalPoints}");
    }

    public void RegisterFolderPlacement(InventoryManager.CollectedEntry item, string folderName)
    {
        if (item == null || string.IsNullOrEmpty(folderName)) return;

        if (scoredFolderItems.Contains(item.itemID)) return;

        bool correct = false;

        Interactable originalObj = null;
        if (!string.IsNullOrEmpty(item.itemDescription)) // itemDescription stores original objectID
        {
            originalObj = InventoryManager.Instance.GetOriginalObject(item.itemDescription);
        }

        if (originalObj != null)
        {
            bool isEvidence = originalObj.CompareTag("Evidence");

            if (isEvidence && folderName == "Important") correct = true;
            else if (!isEvidence && folderName == "Rubbish") correct = true;
        }
        else
        {
            // Photos of nothing or None objects
            if (folderName == "Rubbish") correct = true;
        }

        if (correct)
        {
            totalPoints += 1f;
            scoredFolderItems.Add(item.itemID);
            Debug.Log($"Folder placement correct for {item.itemID}! +1 point. Total: {totalPoints}");
        }
        else
        {
            Debug.Log($"Folder placement incorrect for {item.itemID}. No points awarded.");
        }
    }

    public void ResetScoring()
    {
        totalPoints = 0f;
        scoredFolderItems.Clear();
    }

    public void AddPoint()
    {
        totalPoints += 1f;
    }

    public float GetScore()
    {
        return totalPoints;
    }
}
