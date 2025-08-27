using UnityEngine;

public class InteractableHighlighter : MonoBehaviour
{
    public float highlightIntensity = 10f;
    public bool showInteractionPrompt = false;
    public GameObject interactionText;

    private Renderer rend;
    private Material material;
    private Color originalEmission;
    private bool isHovering = false;

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
        {
            interactionText.SetActive(false);
        }
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

    public bool IsHovering() { return isHovering; }

    public void Highlight(bool state)
    {
        if (material == null) return;

        if (state)
        {
            Color boosted = originalEmission * highlightIntensity;
            material.SetColor("_EmissionColor", boosted);
            material.EnableKeyword("_EMISSION");

            if (showInteractionPrompt && interactionText != null)
            {
                interactionText.SetActive(true);
            }
        }
        else
        {
            material.SetColor("_EmissionColor", originalEmission);
            if (originalEmission.maxColorComponent <= 0f)
            {
                material.DisableKeyword("_EMISSION");
            }

            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
        }
    }
}
