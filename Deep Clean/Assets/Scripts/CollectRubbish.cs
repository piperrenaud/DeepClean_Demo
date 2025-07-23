using UnityEngine;

public class CollectRubbish : MonoBehaviour
{
    public Camera playerCamera;
    public float pickupRange = 3f;
    public LayerMask rubbishLayer;
    public RubbishBin bin;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, rubbishLayer))
            {
                if (!bin.IsFull())
                {
                    Destroy(hit.collider.gameObject);
                    bin.AddRubbish();
                }
                else
                {
                    Debug.Log("Bin is Full!!");
                }
            }
        }
    }
}
