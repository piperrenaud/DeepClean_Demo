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

    [Header("Pickup Settings")]
    public float holdDistance = 2.5f;
    public float rotationSpeed = 100f;
    
    [Header("UniqueID")]
    public string objectID;

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
    }

    void Update()
    {
        if (isHeld)
        {
            RotateObject();
        }

        if ((isHeld || IsHighlighted()) && Input.GetKeyDown(KeyCode.Return))
        {
            InventoryManager.Instance.AddItem(objectID);
            gameObject.SetActive(false);
        }
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
        isHeld = true;

        //safe og parent and position/rotation
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        //parent to player and pos in front
        transform.SetParent(playerTransform);
        transform.localPosition = new Vector3(0, -0.5f, holdDistance);
        transform.localRotation = Quaternion.identity;
    }

    public void DropObject()
    {
        isHeld = false;
        
        //return to og parent and transform
        transform.SetParent(originalParent);
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }

    void RotateObject()
    {
        //rotate left/right with shift
        if (Input.GetKey(KeyCode.LeftShift)) {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
