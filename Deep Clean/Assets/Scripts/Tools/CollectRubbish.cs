using UnityEngine; 

public class CollectRubbish : MonoBehaviour 
{ 
    public Camera playerCamera; 
    public float pickupRange = 3f; 
    public LayerMask rubbishLayer; 
    public LayerMask binLayer; 
    public PlayerRubbishTool tool; // reference to tool script 
    
    [Header("Audio")] 
    public AudioSource audioSource; 
    public AudioClip pickupSound; 

    void Update() 
    { 
        if (Input.GetMouseButtonDown(0)) 
        { 
            if (!tool.HasOpenBag()) return; // only collect if open bag active 
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition); 
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, rubbishLayer)) 
            { 
                GameObject target = hit.collider.gameObject; 
                if (tool.CurrentBag.AddRubbish(1)) 
                { 
                    Destroy(target); 
                    audioSource.PlayOneShot(pickupSound); 
                    
                    // auto-tie if full 
                    if (tool.CurrentBag.IsFull()) 
                    { 
                        tool.StartCoroutine(tool.TieRoutine()); 
                    } 
                }
                else 
                { 
                    GameManager.Instance.Notify("Bag is full!");
                    Debug.Log("Bag is full or tied!"); 
                } 
            } 
        } 

        // Empty bag into bin 
        if (Input.GetKeyDown(KeyCode.E)) 
        { 
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition); 
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, binLayer)) 
            { 
                tool.EmptyBagAtBin(); 
            } 
        } 
    } 
}