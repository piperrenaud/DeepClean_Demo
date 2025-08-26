using UnityEngine;
using System.Collections;
using TMPro;

public enum EvidenceType
{
    None,
    Take,
    Photo,
    RedHerring,
    Nothing
}

public class Interactable : MonoBehaviour
{
    private Renderer rend;
    private Material material;
    private Color originalEmission;

    [Header("Evidence Settings")]
    public EvidenceType evidenceType = EvidenceType.None;
    [TextArea] public string evidenceDescription;
    [TextArea] public string homeownerExplanation; 

    [Header("Highlight Settings")]
    public float highlightIntensity = 10f;

    [Header("Interaction Settings")]
    public bool showInteractionPrompt = false;
    public GameObject interactionText;
    public GameObject inspectionUI;

    private TMP_Text inspectionUIText;

    [Header("Pickup Settings")]
    public float holdDistance = 2.5f;
    public float rotationSpeed = 100f;
    
    [Header("UniqueID")]
    public string objectID;

    [Header("Door Settings")]
    public Animator doorAnimator;

    [Header("Dialogue Settings")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public float typingSpeed = 0.01f;
    public float dialogueDuration = 5f;

    private Coroutine dialogueRoutine;
    private bool isTyping = false;
    private string currentFullText;
    
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

        if (inspectionUI != null)
            inspectionUIText = inspectionUI.GetComponentInChildren<TMP_Text>();

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
                Take();
            }
        }

        if (isHovering && Input.GetKeyDown(KeyCode.E) && doorAnimator == null)
        {
            if (isTyping)
            {
                FinishTypingInstantly();
            }
            else
            {
                ShowDialogue();
            }
        }

        if (isHovering && Input.GetKeyDown(KeyCode.E) && doorAnimator != null)
        {
            ToggleDoor();
        }
    }

    private void ShowDialogueOnUI()
    {
        if (inspectionUI == null || inspectionUIText == null) return;

        inspectionUI.SetActive(true);
        currentFullText = evidenceDescription;

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(TypeTextOnUI(currentFullText));
    }

    private IEnumerator TypeTextOnUI(string fullText)
    {
        isTyping = true;
        inspectionUIText.text = string.Empty;

        foreach (char letter in fullText)
        {
            inspectionUIText.text += letter;
            yield return new WaitForSeconds(typingSpeed);

            if (!isTyping) yield break;
        }

        isTyping = false;
        yield return new WaitForSeconds(dialogueDuration);
        inspectionUI.SetActive(false);
        dialogueRoutine = null;
    }

    private void ShowDialogue()
    {
        if (dialogueBox == null || dialogueText == null) return;
        
        dialogueBox.SetActive(true);

        string fullText = evidenceDescription;

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(TypeText(fullText));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char letter in fullText)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);

            if (!isTyping) yield break;
        }

        isTyping = false;

        yield return new WaitForSeconds(dialogueDuration);

        dialogueBox.SetActive(false);
        dialogueRoutine = null;
    }

    private void FinishTypingInstantly()
    {
        isTyping = false;
        dialogueText.text = currentFullText;

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }
        dialogueRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDuration);
        dialogueBox.SetActive(false);
        dialogueRoutine = null;

        if (inspectionUI != null) inspectionUI.SetActive(false);
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

        inspectionUI.SetActive(true);
        ShowDialogueOnUI();

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
        inspectionUI.SetActive(false);

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

    public void Take()
    {
        Debug.Log("Taking: " + objectID);

        if (InventoryManager.Instance != null)
        {
            // Store this object’s prefab reference
            InventoryManager.Instance.AddItem(
                objectID,
                evidenceType,
                evidenceDescription,
                homeownerExplanation,
                false
            );
        }

        gameObject.SetActive(false);
        isHeld = false;
        inspectionUI.SetActive(false);
    }
}
