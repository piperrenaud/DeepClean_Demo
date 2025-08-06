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
    public float holdDistance = 1.5f;
    public float rotationSpeed = 100f;
    
    [Header("UniqueID")]
    public string objectID;

    private bool isHeld = false;
    private Camera mainCamera;
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
            HoldObject();
            RotateObject();

            if (Input.GetMouseButtonUp(1))
                DropObject();
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

    public void PickUpObject()
    {
        isHeld = true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void DropObject()
    {
        isHeld = false;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    void HoldObject()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = holdDistance;

        Vector3 targetPos = mainCamera.ScreenToWorldPoint(mousePos);
        transform.position = targetPos;
    }

    void RotateObject()
    {
        float rotateX = 0f;
        float rotateY = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) rotateX = 1f;
        if (Input.GetKey(KeyCode.DownArrow)) rotateX = -1f;
        if (Input.GetKey(KeyCode.LeftArrow)) rotateY = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) rotateY = 1f;

        transform.Rotate(Vector3.right * rotateX * rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.up * rotateY * rotationSpeed * Time.deltaTime, Space.World);
    }
}
