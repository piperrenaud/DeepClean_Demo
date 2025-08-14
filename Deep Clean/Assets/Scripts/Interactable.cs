using UnityEngine;

public class Interactable : MonoBehaviour
{
    private Renderer rend;
    private Material material;
    private Color originalEmission;

    [Header("Highlight Settings")]
    public float highlightIntensity = 10f;

    [Header("Interaction Settings")]
    public bool showInteractionPrompt = false;
    public GameObject interactionText;
    public GameObject interactionUI;

    [Header("Pickup Settings")]
    public float holdDistance = 2.5f;
    public float rotationSpeed = 100f;
    
    [Header("UniqueID")]
    public string objectID;

    [Header("Door Settings")]
    public Animator doorAnimator;
    
    private bool isDoorOpen = false;
    private bool isHovering = false;

    private ToolManager toolManager;
    private int savedToolIndex = -1;
    private bool isHeld = false;
    private Camera mainCamera;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            material = rend.material;
            originalEmission = new Color(1f / 255f, 1f / 255f, 1f / 255f);
            material.SetColor("_EmissionColor", originalEmission);
            material.EnableKeyword("_EMISSION");
        }

        if (interactionText != null)
            interactionText.SetActive(false);

        mainCamera = Camera.main;
        toolManager = FindFirstObjectByType<ToolManager>();
    }

    void Update()
    {
        if (isHeld)
        {
            RotateObject();

            //can only take whiile inspecting
            if (Input.GetKeyDown(KeyCode.Return))
            {
                InventoryManager.Instance.AddItem(objectID);
                gameObject.SetActive(false);
            }
        }

        if (isHovering && Input.GetKeyDown(KeyCode.E) && doorAnimator != null)
        {
            ToggleDoor();
        }
    }

    public void ToggleDoor()
    {
        if (isDoorOpen)
        {
            //close the door
            doorAnimator.SetBool("Opening", false);
            doorAnimator.SetBool("Opened", false);
            doorAnimator.SetBool("Closing", true);
            doorAnimator.SetBool("Closed", false);
        }
        else 
        {
            //open the door
            doorAnimator.SetBool("Opening", true);
            doorAnimator.SetBool("Opened", false);
            doorAnimator.SetBool("Closing", false);
            doorAnimator.SetBool("Closed", false);
        }
        isDoorOpen = !isDoorOpen;
    }

    void OnMouseOver()
    {
        isHovering = true;
        Highlight(true);
    }

    void OnMouseExit()
    {
        isHovering = false;
        Highlight(false);
    }

    public bool IsHighlighted()
    {
        return material.GetColor("_EmissionColor") != originalEmission;
    }

    public void Highlight(bool state)
    {
        if (material == null) return;

        if (state)
        {
            Color boosted = originalEmission * highlightIntensity;
            material.SetColor("_EmissionColor", boosted);
            material.EnableKeyword("_EMISSION");

            if (showInteractionPrompt && interactionText != null)
                interactionText.SetActive(true);
        }
        else
        {
            material.SetColor("_EmissionColor", originalEmission);
            if (originalEmission.maxColorComponent <= 0f)
                material.DisableKeyword("_EMISSION");

            if (interactionText != null)
                interactionText.SetActive(false);
        }
    }

    public void PickUpObject(Transform playerTransform)
    {
        //if tool is active put it away first
        if (toolManager != null)
        {
            savedToolIndex = toolManager.GetCurrentToolIndex();
            if (savedToolIndex != -1) //tool is active
            {
                toolManager.ForcePutAwayCurrentTool();
            }
        }

        interactionUI.SetActive(true);

        isHeld = true;

        //safe og parent and position/rotation
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        //parent to player and pos in front
        transform.SetParent(playerTransform);
        transform.localPosition = new Vector3(0, -0.5f, holdDistance);
        transform.localRotation = Quaternion.identity;
    }

    public void DropObject()
    {
        interactionUI.SetActive(false);

        isHeld = false;
        
        //return to og parent and transform
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        //get tool back if active
        if (toolManager != null && savedToolIndex != -1)
        {
            toolManager.ForcePickUpTool(savedToolIndex);
            savedToolIndex = -1;
        }
    }

    public void RotateObject()
    {
        //hold left mouse button to rotate object
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            //rotate around axis
            transform.Rotate(Vector3.up, -mouseX, Space.Self);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }
    }
}
