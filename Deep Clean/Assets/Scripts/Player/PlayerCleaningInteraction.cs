using UnityEngine;
using System.Collections;

public class PlayerCleaningInteraction : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public LayerMask dirtLayer;
    public PlayerCleaningTool cleaningTool;

    [Header("Spray Settings")]
    public float sprayRange = 5f;

    [Header("Rag Settings")]
    public float ragDistance = 0.5f;
    public float cleaningTickRate = 0.02f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spraySound;
    public AudioClip wipeSound;

    private DirtSpot targetedDirt;
    private bool isWiping = false;

    void Update()
    {
        HandleSpraying();
        HandleWiping();
    }

    private void HandleSpraying()
    {
        if (!cleaningTool.HasTools()) return;

        if (Input.GetMouseButtonDown(0)) // left click
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, sprayRange, dirtLayer))
            {
                DirtSpot dirt = hit.collider.GetComponent<DirtSpot>();
                if (dirt != null && !dirt.IsFullyCleaned())
                {
                    dirt.Spray(); // mark as sprayed
                    audioSource.PlayOneShot(spraySound);
                    cleaningTool.animator.SetTrigger("Spraying"); // play spray animation
                }
            }
        }
    }

    private void HandleWiping()
    {
        if (!cleaningTool.HasTools()) return;

        if (Input.GetMouseButton(1)) // hold right click
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, sprayRange, dirtLayer))
            {
                DirtSpot dirt = hit.collider.GetComponent<DirtSpot>();
                if (dirt != null && dirt.CanBeWiped() && !isWiping)
                {
                    StartCoroutine(WipeDirtRoutine(dirt));
                }
            }
        }
        else if (isWiping)
        {
            StopCoroutine("WipeDirtRoutine");
            isWiping = false;
        }
    }

    private IEnumerator WipeDirtRoutine(DirtSpot dirt)
    {
        isWiping = true;
        targetedDirt = dirt;

        Transform rag = cleaningTool.rag.transform;
        Animator ragAnimator = rag.GetComponent<Animator>();
        Transform originalParent = rag.parent;

        // Store original transform
        Vector3 originalLocalPos = rag.localPosition;
        Quaternion originalLocalRot = rag.localRotation;

        // Move rag above dirt (once) and look at it
        rag.parent = dirt.transform; // detach from player hand temporarily
        Vector3 startPos = dirt.transform.position + Vector3.up * ragDistance;
        rag.position = startPos;
        rag.rotation = dirt.transform.rotation;

        // Play rag cleaning animation
        ragAnimator.SetBool("IsCleaning", true);

        audioSource.clip = wipeSound;
        audioSource.loop = true;
        audioSource.Play();

        // Get cleaning tool data
        CleaningTool toolData = cleaningTool.rag.GetComponent<CleaningTool>();

        // Clean while right-click held and dirt not fully cleaned
        while (Input.GetMouseButton(1) && !dirt.IsFullyCleaned())
        {
            dirt.StartCleaning(toolData, rag);
            yield return new WaitForSeconds(cleaningTickRate);
        }

        audioSource.Stop();
        audioSource.loop = false;

        ragAnimator.SetBool("IsCleaning", false);

        // Smoothly move back to original parent
        float moveDuration = 0.2f;
        float elapsed = 0f;
        Vector3 ragStartPos = rag.position;

        while (elapsed < moveDuration)
        {
            rag.position = Vector3.Lerp(ragStartPos, originalParent.position, elapsed / moveDuration);
            rag.rotation = Quaternion.Slerp(rag.rotation, originalParent.rotation, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Re-parent and reset local transform
        rag.parent = originalParent;
        rag.localPosition = originalLocalPos;
        rag.localRotation = originalLocalRot;

        dirt.StopCleaning();
        isWiping = false;
    }
}
