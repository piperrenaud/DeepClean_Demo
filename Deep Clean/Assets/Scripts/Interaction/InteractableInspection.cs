using UnityEngine;
using TMPro;
using System.Collections;

public class InteractableInspection : MonoBehaviour
{
    public GameObject inspectionUI;
    public TMP_Text description;
    public GameObject gameUI;
    public float typingSpeed = 0.01f;

    private Coroutine dialogueRoutine;
    private bool isTyping = false;

    public void ShowUI(string description)
    {
        if (inspectionUI == null || description == null) return;

        inspectionUI.SetActive(true);
        gameUI.SetActive(false);

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(TypeText(description));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        description.text = string.Empty;

        foreach (char letter in fullText)
        {
            description.text += letter;
            yield return new WaitForSeconds(typingSpeed);

            if (!isTyping) yield break;
        }

        isTyping = false;
        dialogueRoutine = null;
    }

    public void HideUI()
    {
        if (inspectionUI != null)
        {
            inspectionUI.SetActive(false);
            gameUI.SetActive(true);
        }
    }
}
