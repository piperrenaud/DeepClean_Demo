using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIDropFolder : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform folderGridParent;
    [SerializeField] private GameObject itemSlotPrefab;

    public List<InventoryManager.CollectedEntry> folderItems = new List<InventoryManager.CollectedEntry>();

    private FolderSlotUI currentSelectedSlot;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<UIDragItem>();
        if (dragged == null || dragged.linkedItem == null) return;

        folderItems.Add(dragged.linkedItem);
        
        string folderName = gameObject.name; // Make sure the folder GameObjects are named "Important" and "Rubbish"
        EvidenceScoringManager.Instance.RegisterFolderPlacement(dragged.linkedItem, folderName);
        
        GameObject newSlot = Instantiate(itemSlotPrefab, folderGridParent);
        newSlot.name = dragged.linkedItem.itemID + "_InFolder";

        var slotUI = newSlot.GetComponent<FolderSlotUI>();
        if (slotUI != null)
        {
            slotUI.InitializeSlot(dragged.linkedItem, this, dragged.sourceComputer);
            slotUI.onSelected += OnSlotSelected;
        }

        dragged.RemoveFromSource();
        Destroy(dragged.gameObject);

        LayoutRebuilder.ForceRebuildLayoutImmediate(folderGridParent as RectTransform);
    }

    private void OnSlotSelected(FolderSlotUI selected)
    {
        if (currentSelectedSlot != null && currentSelectedSlot != selected)
        {
            currentSelectedSlot.HideRemoveButton();
        }

        currentSelectedSlot = selected;
        currentSelectedSlot.ShowRemoveButton();
    }

}
