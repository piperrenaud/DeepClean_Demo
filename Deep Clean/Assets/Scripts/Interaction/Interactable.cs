using UnityEngine;
using UnityEditor;

public static class PlayerHeldItem
{
    public static GameObject Current;
}

public class Interactable : MonoBehaviour
{
    [Header("Inventory Settings")]
    public string objectID;
    public Sprite itemIcon;
    public string questionName;

    [Header("Evidence Settings")]
    public EvidenceType evidenceType = EvidenceType.None;
    [TextArea] public string itemDescription;
    [TextArea] public string playerDialogue;
    [TextArea] public string explanation;

    [Header("Interaction Settings")]
    public bool canBeInspected = true;

    [Header("Pickup Settings")]
    public float holdDistance = 2.5f;
    public float rotationSpeed = 100f;

    [Header("Door Settings")]
    public Animator doorAnimator;

    private bool isHeld = false;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private InteractableHighlighter highlighter;
    private InteractableDialogue dialogue;
    private InteractableInspection inspection;

    private Collider objCollider;

    private bool isDiscovered = false;

    void Start()
    {
        highlighter = GetComponent<InteractableHighlighter>();
        dialogue = GetComponent<InteractableDialogue>();
        inspection = GetComponent<InteractableInspection>();
        objCollider = GetComponent<Collider>();

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
                else
                {
                    dialogue.ShowDialogue(playerDialogue);

                    if (!isDiscovered)
                    {
                        isDiscovered = true;
                        DiscoveryManager.Instance.RegisterDiscovery(this);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.E) && doorAnimator != null)
            {
                ToggleDoor();
            }
        }
    }

    public void PickUpObject(Transform playerTransform)
    {
        if (canBeInspected && inspection != null) inspection.ShowUI(itemDescription);

        isHeld = true;

        PlayerHeldItem.Current = this.gameObject;

        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        transform.SetParent(playerTransform);
        transform.localPosition = new Vector3(0, -0.5f, holdDistance);
        transform.localRotation = Quaternion.identity;

        //diable boxcollider
        if (objCollider != null) objCollider.enabled = false;

        //disable rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DropObject()
    {
        if (inspection != null) inspection.HideUI();
        isHeld = false;
        PlayerHeldItem.Current = null;

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        //enable boxcollider
        if (objCollider != null) objCollider.enabled = true;

        //enable rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
        if (InventoryManager.Instance.IsFull(false))
        {
            GameManager.Instance.Notify("Object Inventory Full!");
            return;
        }

        //mark as discovered
        if (!isDiscovered)
        {
            isDiscovered = true;
            DiscoveryManager.Instance.RegisterDiscovery(this);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(
                objectID,
                evidenceType,
                itemDescription,
                playerDialogue,
                explanation,
                false,
                itemIcon
            );

            InventoryManager.Instance.RegisterItemObject(objectID, this);
        }

        if (inspection != null) inspection.HideUI();

        if (objCollider != null) objCollider.enabled = true;

        EnemyWander[] enemies = FindObjectsByType<EnemyWander>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.OnItemTaken(this.gameObject);
        }

        gameObject.SetActive(false);
        isHeld = false;
        PlayerHeldItem.Current = null;
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

    public string GetExplanation()
    {
        return explanation;
    }

    public string GetObjectID()
    {
        return objectID;
    }
}
