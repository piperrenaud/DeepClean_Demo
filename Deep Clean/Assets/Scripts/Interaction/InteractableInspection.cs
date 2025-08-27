using UnityEngine;
using TMPro;
using System.Collections;

public class InteractableInspection : MonoBehaviour
{
    public GameObject inspectionUI;
    private TMP_Text inspectionUIText;

    public float typingSpeed = 0.01f;

    private Coroutine dialogueRoutine;
    private bool isTyping = false;

    void Start()
    {
        if (inspectionUI != null)
        {
            inspectionUIText = inspectionUI.GetComponentInChildren<TMP_Text>();
        }
    }

    public void ShowUI(string description)
    {
        if (inspectionUI == null || inspectionUIText == null) return;

        inspectionUI.SetActive(true);

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(TypeText(description));
    }

    private IEnumerator TypeText(string fullText)
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
        dialogueRoutine = null;
    }

    public void HideUI()
    {
        if (inspectionUI != null)
        {
            inspectionUI.SetActive(false);
        }
    }
}
