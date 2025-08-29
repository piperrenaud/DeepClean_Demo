using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryUI;
    public GameObject gameUI;
    public GameObject crossHair;

    public static bool inventoryOpen { get; private set; } = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(inventoryOpen);
        }

        if (gameUI != null)
        {
            gameUI.SetActive(!inventoryOpen);
        }
        
        if (crossHair != null)
        {
            crossHair.SetActive(!inventoryOpen);
        }

        if (inventoryOpen)
        {
            //unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            //lock cursor back
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
