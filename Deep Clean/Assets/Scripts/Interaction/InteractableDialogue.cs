using UnityEngine;
using TMPro;
using System.Collections;

public class InteractableDialogue : MonoBehaviour
{
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public float typingSpeed = 0.01f;
    public float dialogueDuration = 3f;

    private Coroutine dialogueRoutine;
    private bool isTyping = false;
    private string currentFullText;
    
    void Update()
    {
        if (dialogueBox.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                FinishTypingInstantly();
            }
            else
            {
                dialogueBox.SetActive(false);
            }
        }
    }

    public void ShowDialogue(string text)
    {
        if (dialogueBox == null || dialogueText == null) return;

        dialogueBox.SetActive(true);
        currentFullText = text;

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(TypeText(text));
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

    public void FinishTypingInstantly()
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
    }

    public bool IsTyping() { return isTyping; }
}
