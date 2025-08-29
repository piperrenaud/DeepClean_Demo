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

    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Start()
    {
        Instance = this;
    }

    public void RefreshUI()
    {
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
                ShowDescription(item.itemDescription);
                ShowSelectedImage(item.itemIcon);
            });
        }
    }

    private void ShowDescription(string desc)
    {
        if (descriptionText != null)
        {
            descriptionText.text = desc;
        }
    }

    private void ShowSelectedImage(Sprite icon)
    {
        if (selectedItemImage != null)
        {
            selectedItemImage.sprite = icon;
            selectedItemImage.enabled = (icon != null);
        }
    }
}