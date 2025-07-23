using UnityEngine;

public class RubbishBin : MonoBehaviour
{
    public Transform rubbishFill;
    public int maxCapacity = 5;
    public Camera playerCamera;
    public float pickupRange = 3f;
    public LayerMask rubbishLayer;
    private int currentAmount = 0;
    public float fillStep = 0.08f;

    public bool AddRubbish()
    {
        if (currentAmount >= maxCapacity)
            return false;

        currentAmount++;
        Vector3 pos = rubbishFill.localPosition;
        rubbishFill.localPosition = new Vector3(pos.x, currentAmount * fillStep, pos.z);
        return true;
    }

    public bool IsFull()
    {
        return currentAmount >= maxCapacity;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, rubbishLayer))
            {
                if (!IsFull())
                {
                    Destroy(hit.collider.gameObject);
                    AddRubbish();
                }
                else
                {
                    Debug.Log("Bin is Full!!");
                }
            }
        }
    }
}
