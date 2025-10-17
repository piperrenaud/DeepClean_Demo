using UnityEngine;
using TMPro;
using System.Collections;

public class StartScene : MonoBehaviour
{
    public GameObject startDialogue;
    public GameObject fadeFromBlack;
    public GameObject[] texts;
    public AudioSource audio;

    private TMP_Text textBox;
    private string fullText;
    private GameObject player;
    private PlayerMovement playerMovement;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();

        playerMovement.enabled = false;

        startDialogue.SetActive(false);
        texts[0].SetActive(false);
        texts[1].SetActive(false);
        texts[2].SetActive(false);

        StartCoroutine(PlayerDialogue());
    }

    IEnumerator PlayerDialogue()
    {
        yield return new WaitForSeconds(1.5f);
        startDialogue.SetActive(true);

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].SetActive(true);
            textBox = texts[i].GetComponent<TMP_Text>();
            
            fullText = textBox.text; 
            textBox.text = string.Empty; 

            foreach (char letter in fullText)
            {
                textBox.text += letter; 
                yield return new WaitForSeconds(0.03f); 
            }

            yield return new WaitForSeconds(1f);
            texts[i].SetActive(false);
        }

        playerMovement.enabled = true;
        startDialogue.SetActive(false);
        fadeFromBlack.SetActive(false);

        audio.Play();
    }
}
