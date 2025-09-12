using UnityEngine;
using System.Collections;

public class PlayerCleaningTool : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public GameObject sprayBottle;
    public GameObject rag;

    private bool hasTools = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!hasTools)
            {
                StartCoroutine(PlayerToolManager.Instance.SwitchTool(() => PickupRoutine()));
            }
            else
            {
                StartCoroutine(PutdownRoutine());
            }
        }
    }


    public IEnumerator PickupRoutine()
    {
        animator.SetTrigger("CleaningPickup");
        yield return new WaitForSeconds(0.05f);

        sprayBottle.SetActive(true);
        rag.SetActive(true);
        hasTools = true;
    }

    public IEnumerator PutdownRoutine()
    {
        animator.SetTrigger("CleaningPutdown");
        yield return new WaitForSeconds(0.05f);

        sprayBottle.SetActive(false);
        rag.SetActive(false);
        hasTools = false;
    }

    public bool HasTools() => hasTools;
}
