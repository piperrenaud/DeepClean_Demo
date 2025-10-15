using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Computer : MonoBehaviour
{
    public static Computer Instance { get; private set; } 

    [Header("Computer UI")]
    [SerializeField] private GameObject computerScreen;

    [Header("Scroll Content Area")]
    [SerializeField] private Transform scrollContentParent;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Item display elements")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text playerText;
    [SerializeField] private TMP_Text explanationText;
    [SerializeField] private Image itemImage;

    private List<GameObject> createdSlots = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        RefreshCollectedItems();
    }

    void Start()
    {
        computerScreen.SetActive(false);
    }

    public void TurnOn()
    {
        computerScreen.SetActive(true);
    }

    public void TurnOff()
    {
        computerScreen.SetActive(false);
    }

    public void RefreshCollectedItems()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        foreach (var slot in createdSlots)
        {
            Destroy(slot);
        }
        createdSlots.Clear();

        List<InventoryManager.CollectedEntry> allItems = InventoryManager.Instance.GetAllItems();

        foreach (var item in allItems)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, scrollContentParent);
            createdSlots.Add(newSlot);

            Image icon = newSlot.transform.Find("Image")?.GetComponent<Image>();
            if (icon != null) icon.sprite = item.itemIcon;

            Button btn = newSlot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemClicked(item));
            }

            var drag = newSlot.GetComponent<UIDragItem>();
            if (drag != null)
            {
                drag.linkedItem = item;
                drag.sourceComputer = this;
            }
        }
    }

    private void OnItemClicked(InventoryManager.CollectedEntry entry)
    {
        if (descriptionText != null)
        {
            descriptionText.text = entry.itemDescription;
        }

        if (playerText != null)
        {
            playerText.text = entry.playerDialogue;
        }

        if (explanationText != null)
        {
            explanationText.text = entry.explanation;
        }

        if (itemImage != null)
        {
            itemImage.sprite = entry.itemIcon;
            itemImage.enabled = (entry.itemIcon != null);
        }
    }

    public void RemoveItemSlot(GameObject slot)
    {
        if (createdSlots.Contains(slot))
        {
            createdSlots.Remove(slot);
            Destroy(slot);
        }
    }

    public void AddItemToScrollGrid(InventoryManager.CollectedEntry item)
    {
        GameObject newSlot = Instantiate(itemSlotPrefab, scrollContentParent);
        createdSlots.Add(newSlot);

        Image icon = newSlot.transform.Find("Image")?.GetComponent<Image>();
        if (icon != null)
        {
            icon.sprite = item.itemIcon;
        }

        Button btn = newSlot.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnItemClicked(item));
        }

        var drag = newSlot.GetComponent<UIDragItem>();
        if (drag != null)
        {
            drag.linkedItem = item;
            drag.sourceComputer = this;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContentParent as RectTransform);
    }
}


