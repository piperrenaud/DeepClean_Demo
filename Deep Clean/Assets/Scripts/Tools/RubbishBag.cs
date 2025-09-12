using UnityEngine;

public class RubbishBag : MonoBehaviour
{
    public int maxCapacity = 15;
    public int currentAmount = 0;
    public bool isTied = false;

    private float currentRotation = 0.0f;

    public bool AddRubbish(int weight)
    {
        if (isTied) return false; // can't add if tied
        if (currentAmount >= maxCapacity) return false;

        currentAmount += weight;
        currentRotation += 45.0f;

        return true;
    }

    public bool IsFull() => currentAmount >= maxCapacity;
    public bool IsEmpty() => currentAmount == 0;

    public void TieBag() => isTied = true;
}
