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

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Use material instance
            material = rend.material;

            originalEmission = new Color(1f / 255f, 1f / 255f, 1f / 255f);
            material.SetColor("_EmissionColor", originalEmission);
            material.EnableKeyword("_EMISSION");
        }

        if (interactionText != null)
            interactionText.SetActive(false);
    }

    public void Highlight(bool state)
    {
        if (material == null) return;

        if (state)
        {
            // Boost brightness
            Color boosted = originalEmission * highlightIntensity;
            material.SetColor("_EmissionColor", boosted);
            material.EnableKeyword("_EMISSION");

            if (showInteractionPrompt && interactionText != null)
                interactionText.SetActive(true);
        }
        else
        {
            // Reset
            material.SetColor("_EmissionColor", originalEmission);
            if (originalEmission.maxColorComponent <= 0f)
                material.DisableKeyword("_EMISSION");

            if (interactionText != null)
                interactionText.SetActive(false);
        }
    }
}
