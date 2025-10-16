using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FolderSlotUI : MonoBehaviour
{
    [HideInInspector] public InventoryManager.CollectedEntry linkedItem;
    [HideInInspector] public UIDropFolder parentFolder;
    [HideInInspector] public Computer sourceComputer;

    [Header("UI Reference")]
    [SerializeField] private Button removeButton;
    [SerializeField] private Image itemIcon;

    public Action<FolderSlotUI> onSelected;

    private void Start()
    {
        if (removeButton != null)
        {
            removeButton.gameObject.SetActive(false);
            removeButton.onClick.AddListener(RemoveFromFolder);
        }

        Button selfButton = GetComponent<Button>();
        if (selfButton != null)
        {
            selfButton.onClick.AddListener(() =>
            {
                onSelected?.Invoke(this);
            });
        }
    }

    public void InitializeSlot(InventoryManager.CollectedEntry entry, UIDropFolder folder, Computer computer)
    {
        linkedItem = entry;
        parentFolder = folder;
        sourceComputer = computer;

        //remove this
        if (linkedItem.isEvidence)
        {
            itemIcon.color = Color.yellow;
        }
        else
        {
            itemIcon.color = Color.white;
        }
        //remove above

        if (itemIcon != null && linkedItem != null)
        {
            itemIcon.sprite = linkedItem.itemIcon;
            itemIcon.enabled = true;
        }
    }

    public void ShowRemoveButton()
    {
        if (removeButton != null)
        {
            removeButton.gameObject.SetActive(true);
        }
    }

    public void HideRemoveButton()
    {
        if (removeButton != null)
        {
            removeButton.gameObject.SetActive(false);
        }
    }

    public void RemoveFromFolder()
    {
        if (linkedItem == null || parentFolder == null) return;

        parentFolder.folderItems.Remove(linkedItem);
        Destroy(gameObject);
        sourceComputer.AddItemToScrollGrid(linkedItem);
    }
}
