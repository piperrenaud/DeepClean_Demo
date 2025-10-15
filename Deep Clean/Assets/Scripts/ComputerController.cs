using UnityEngine;

public class ComputerController : MonoBehaviour
{
    public GameObject videoPlayer;

    private Interactable interactable;
    private InteractableDialogue dialogue;
    private InteractableHighlighter highlighter;

    void Start()
    {
        videoPlayer.SetActive(false);

        interactable = gameObject.GetComponent<Interactable>();
        dialogue = gameObject.GetComponent<InteractableDialogue>();
        highlighter = gameObject.GetComponent<InteractableHighlighter>();

        interactable.enabled = false;
        dialogue.enabled = false;
        highlighter.enabled = false;
    }

    public void TurnOn()
    {
        videoPlayer.SetActive(true);
    }

    public void TurnOff()
    {
        videoPlayer.SetActive(false);
    }

    public void HandleUSBTaken()
    {
        interactable.enabled = true;
        dialogue.enabled = true;
        highlighter.enabled = true;
    }

    public void PlayAudio()
    {
        GetComponent<AudioSource>().Play();
    }
}
