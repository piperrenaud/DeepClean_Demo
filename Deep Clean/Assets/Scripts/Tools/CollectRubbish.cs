using UnityEngine;

public class CollectRubbish : MonoBehaviour
{
    public Camera playerCamera;
    public float pickupRange = 3f;
    public LayerMask rubbishLayer;
    public LayerMask binLayer;
    public RubbishBin bin;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!bin.gameObject.activeInHierarchy) return;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, rubbishLayer))
            {
                GameObject target = hit.collider.gameObject;
                if (target.name == "Rubbish Bag")
                {
                    if (bin.currentAmount <= (bin.maxCapacity / 2))
                    {
                        Destroy(target);
                        bin.AddRubbish(5);
                    }
                    else 
                    {
                        Debug.Log("Bin must be empty first");
                    }
                }
                else 
                {
                    if (!bin.IsFull())
                    {
                        Destroy(target);
                        bin.AddRubbish(1);
                    }
                    else
                    {
                        Debug.Log("Bin is Full!!");
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, binLayer))
            {                
                bin.EmptyBin();
                Debug.Log("Bin emptied");
            }
        }
    }
}