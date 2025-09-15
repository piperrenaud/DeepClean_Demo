using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance;

    [Header("UI References")]
    public Transform gridParent;
    public GameObject slotPrefab;
    public TMP_Text descriptionText;
    public Image selectedItemImage;

    [Header("Drop Button")]
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

    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        //auto refresh
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (InventoryManager.Instance == null) return;

        //clear old slots
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();

        //rebuild grid
        List<InventoryManager.CollectedEntry> items = InventoryManager.Instance.GetAllItems();
        foreach (var item in items)
        {
            GameObject slot = Instantiate(slotPrefab, gridParent);
            spawnedSlots.Add(slot);

            //set text
            TMP_Text label = slot.transform.Find("ItemName")?.GetComponent<TMP_Text>();
            if (label != null) label.text = "";

            //set icon
            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null) 
            {
                if (item.itemIcon != null) icon.sprite = item.itemIcon;
                else icon.sprite = null;
            }

            //button click shows description
            Button button = slot.GetComponent<Button>();
            button.onClick.AddListener(() => 
            {
                ShowDescription(item.itemDescription, item);
                ShowSelectedImage(item.itemIcon);
            });
        }
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