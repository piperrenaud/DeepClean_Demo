using UnityEngine;
using UnityEditor;

public class Interactable : MonoBehaviour
{
    [Header("UniqueID")]
    public string objectID;

    [Header("Evidence Settings")]
    public EvidenceType evidenceType = EvidenceType.None;
    [TextArea] public string itemDescription;
    [TextArea] public string playerDialogue;
    [TextArea] public string explanation;

    [Header("Pickup Settings")]
    public float holdDistance = 2.5f;
    public float rotationSpeed = 100f;

    [Header("Door Settings")]
    public Animator doorAnimator;

    private bool isHeld = false;
    private ToolManager toolManager;
    private int savedToolIndex = -1;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private InteractableHighlighter highlighter;
    private InteractableDialogue dialogue;
    private InteractableInspection inspection;

    void Start()
    {
        toolManager = FindFirstObjectByType<ToolManager>();

        highlighter = GetComponent<InteractableHighlighter>();
        dialogue = GetComponent<InteractableDialogue>();
        inspection = GetComponent<InteractableInspection>();

        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isHeld)
        {
            RotateObject();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                Take();
            }
        }

        if (highlighter.IsHovering())
        {
            if (Input.GetKeyDown(KeyCode.E) && doorAnimator == null)
            {
                if (dialogue.IsTyping()) dialogue.FinishTypingInstantly();
                else dialogue.ShowDialogue(playerDialogue);
            }

            if (Input.GetKeyDown(KeyCode.E) && doorAnimator != null)
            {
                ToggleDoor();
            }
        }
    }

    public void PickUpObject(Transform playerTransform)
    {
        if (toolManager != null)
        {
            savedToolIndex = toolManager.GetCurrentToolIndex();
            if (savedToolIndex != -1) toolManager.ForcePutAwayCurrentTool();
        }

        inspection.ShowUI(itemDescription);

        isHeld = true;

        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        transform.SetParent(playerTransform);
        transform.localPosition = new Vector3(0, -0.5f, holdDistance);
        transform.localRotation = Quaternion.identity;
    }

    public void DropObject()
    {
        inspection.HideUI();
        isHeld = false;

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (toolManager != null && savedToolIndex != -1)
        {
            toolManager.ForcePickUpTool(savedToolIndex);
            savedToolIndex = -1;
        }
    }

    public void RotateObject()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, -mouseX, Space.Self);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }
    }

    public void Take()
    {
        Debug.Log("Taking: " + objectID);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(
                objectID,
                evidenceType,
                itemDescription,
                playerDialogue,
                explanation,
                false
            );
        }

        gameObject.SetActive(false);
        isHeld = false;
        inspection.HideUI();
    }

    public void ToggleDoor()
    {
        bool isOpen = doorAnimator.GetBool("Opening");

        if (isOpen)
        {
            doorAnimator.SetBool("Opening", false);
            doorAnimator.SetBool("Opened", false);
            doorAnimator.SetBool("Closing", true);
            doorAnimator.SetBool("Closed", false);
        }
        else
        {
            doorAnimator.SetBool("Opening", true);
            doorAnimator.SetBool("Opened", false);
            doorAnimator.SetBool("Closing", false);
            doorAnimator.SetBool("Closed", false);
        }
    }

    public void Highlight(bool state)
    {
        if (highlighter != null)
        {
            highlighter.Highlight(state);
        }
    }
}
