using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance;

    [Header("UI References")]
    public Transform itemGridParent;
    public Transform photoGridParent;
    public GameObject slotPrefab;
    public GameObject photoSlotPrefab;
    public TMP_Text descriptionText;
    public Image selectedItemImage;

    [Header("X Button")]
    public GameObject dropButton;
    private InventoryManager.CollectedEntry selectedItem;
    public Transform playerTransform;

    [System.Serializable]
    public class ItemPrefab
    {
        public string itemID;
        public Interactable prefab;
    }
    public List<ItemPrefab> itemPrefabs;

    private List<GameObject> itemSlots = new List<GameObject>();
    private List<GameObject> photoSlots = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (InventoryManager.Instance == null) return;

        //clear old slots
        foreach (var slot in itemSlots) Destroy(slot);
        itemSlots.Clear();
        foreach (var slot in photoSlots) Destroy(slot);
        photoSlots.Clear();

        //rebuild grid
        List<InventoryManager.CollectedEntry> allEntries = InventoryManager.Instance.GetAllItems();

        //poplate regular items
        foreach (var item in allEntries)
        {
            if (item.isPhoto) continue;

            GameObject slot = Instantiate(slotPrefab, itemGridParent);
            itemSlots.Add(slot);
            SetupSlot(slot, item);
        }

        //poplate photo grid
        foreach (var photo in allEntries)
        {
            if (!photo.isPhoto) continue;

            GameObject slot = Instantiate(photoSlotPrefab, photoGridParent);
            photoSlots.Add(slot);
            SetupSlot(slot, photo);
        }
    }

    private void SetupSlot(GameObject slot, InventoryManager.CollectedEntry entry)
    {
        //set icon
        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null) icon.sprite = entry.itemIcon;

        //set label
        TMP_Text label = slot.transform.Find("ItemName")?.GetComponent<TMP_Text>();
        if (label != null) label.text = "";

        //button click
        Button button = slot.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            ShowDescription(entry.itemDescription, entry);
            ShowSelectedImage(entry.itemIcon);
        });
    }

    private void ShowDescription(string desc, InventoryManager.CollectedEntry item)
    {
        selectedItem = item;

        if (descriptionText != null)
        {
            descriptionText.text = desc;
        }

        //show drop button
        if (dropButton != null)
            dropButton.SetActive(true);
    }

    private void ShowSelectedImage(Sprite icon)
    {
        if (selectedItemImage != null)
        {
            selectedItemImage.sprite = icon;
            selectedItemImage.enabled = (icon != null);
        }
    }

    public void DropSelectedItem()
    {
        if (selectedItem == null) return;

        //remove from inventory
        InventoryManager.Instance.RemoveItem(selectedItem.itemID);

        //spawn object at players feet
        Interactable objPrefab = FindInteractablePrefab(selectedItem.itemID);
        if (objPrefab != null && playerTransform != null)
        {
            Vector3 dropPos = playerTransform.position + playerTransform.forward * 1f;
            
            //instantiate the object
            Interactable droppedObj = Instantiate(objPrefab, dropPos, Quaternion.identity);
            droppedObj.gameObject.SetActive(true);

            //give rigidbody
            Rigidbody rb = droppedObj.gameObject.AddComponent<Rigidbody>();
            if (rb == null)
            {
                rb = droppedObj.gameObject.AddComponent<Rigidbody>();
            }

            //enable gravity and reset velocity
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            //copy inventory data onto dropped object
            droppedObj.itemDescription = selectedItem.itemDescription;
            droppedObj.playerDialogue = selectedItem.playerDialogue;
            droppedObj.explanation = selectedItem.explanation;
            droppedObj.evidenceType = selectedItem.type;
            droppedObj.itemIcon = selectedItem.itemIcon;
            droppedObj.gameObject.tag = "DroppedItem";
        }

        //clear selection
        selectedItem = null;
         if (dropButton != null) dropButton.SetActive(false);

        //clear ui
        if (descriptionText != null) descriptionText.text = "";
        if (selectedItemImage != null)
        {
            selectedItemImage.sprite = null;
            selectedItemImage.enabled = false;
        }

        RefreshUI();
    }

    private Interactable FindInteractablePrefab(string itemID)
    {
        foreach (var entry in itemPrefabs)
        {
            if (entry.itemID == itemID) return entry.prefab;
        }
        return null;
    }
}