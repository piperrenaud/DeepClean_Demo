using UnityEngine;

public class RubbishBin : MonoBehaviour
{
    public Transform rubbishFill;
    public int maxCapacity = 15;
    public Camera playerCamera;
    public float pickupRange = 3f;
    public LayerMask rubbishLayer;
    public int currentAmount = 0;
    public float fillStep = 0.025f;
    private float currentRotation = 0.0f;

    public bool AddRubbish(int weight)
    {
        if (currentAmount >= maxCapacity)
            return false;

        currentAmount += weight;
        currentRotation += 45.0f;
        Vector3 pos = rubbishFill.localPosition;
        rubbishFill.localPosition = new Vector3(pos.x, currentAmount * fillStep, pos.z);
        rubbishFill.transform.localRotation = Quaternion.Euler(0.0f, currentRotation, 0.0f);
        return true;
    }

    public bool IsFull()
    {
        return currentAmount >= maxCapacity;
    }

    public bool IsEmpty()
    {
        return currentAmount == 0;
    }

    public void EmptyBin()
    {
        currentAmount = 0;
        Vector3 pos = rubbishFill.localPosition;
        rubbishFill.localPosition = new Vector3(0.0219f,-0.1209f,0.0049f);
    }
}