using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class StartGameManager : MonoBehaviour
{
    [Header("Intro Dialogue")]
    public GameObject startDialogueCanvas;
    public GameObject[] murderDialogues;
    public GameObject[] robberyDialogues;
    public GameObject[] playerDialogues;
    public GameObject murderParent;
    public GameObject robberyParent;
    public GameObject playerParent;

    [Header("Cutscenes")]
    public PlayableDirector introScene;
    public GameObject cutsceneCam;
    public GameObject fadeFromBlack;
    public GameObject[] gameplayObjects;
    public GameObject instructions;
    public GameObject cutsceneParent;

    [Header("Audio CLips")]
    public AudioClip typing;
    public AudioClip newsMusic;

    public bool skipIntro = false;

    private string fullText;
    private TMP_Text textMeshPro; 
    private AudioSource audioSource;

    void Start()
    {
        cutsceneParent.SetActive(false);
        fadeFromBlack.SetActive(false);
        instructions.SetActive(false);
        cutsceneCam.SetActive(true);
        startDialogueCanvas.SetActive(true);

        audioSource = gameObject.GetComponent<AudioSource>();

        foreach (GameObject obj in gameplayObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }            
        }

        introScene.stopped += OnCutsceneEnd;

        for (int i = 0; i < robberyDialogues.Length; i++)
        {
            robberyDialogues[i].SetActive(false);
            Debug.Log("here");
        }
 
        for (int i = 0; i < murderDialogues.Length; i++)
        {
            murderDialogues[i].SetActive(false);
        }

        for (int i = 0; i < playerDialogues.Length; i++)
        {
            playerDialogues[i].SetActive(false);
        }

        if (!skipIntro)
        {
            StartCoroutine(StartDialogue());
        }
        else
        {
            startDialogueCanvas.SetActive(false);
            cutsceneParent.SetActive(true);
            introScene.Play();
        }
    }

    IEnumerator StartDialogue()
    {
        audioSource.clip = newsMusic;
        audioSource.Play();

        //murder dialoge
        for (int i = 0; i < murderDialogues.Length; i++)
        {
            murderDialogues[i].SetActive(true);

            textMeshPro = murderDialogues[i].GetComponent<TMP_Text>();

            fullText = textMeshPro.text; 
            textMeshPro.text = string.Empty; 
            foreach (char letter in fullText)
            {
                textMeshPro.text += letter; 
                yield return new WaitForSeconds(0.03f); 
            }

            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(2f);
        murderParent.SetActive(false);

        //robbery dialogue
        for (int i = 0; i < robberyDialogues.Length; i++)
        {
            robberyDialogues[i].SetActive(true);

            textMeshPro = robberyDialogues[i].GetComponent<TMP_Text>();

            fullText = textMeshPro.text; 
            textMeshPro.text = string.Empty; 
            foreach (char letter in fullText)
            {
                textMeshPro.text += letter; 
                yield return new WaitForSeconds(0.03f); 
            }

            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(FadeMusic());
        yield return new WaitForSeconds(2f);
        robberyParent.SetActive(false);
        yield return new WaitForSeconds(1f);


        audioSource.clip = typing;
        //player dialogues
        for (int i = 0; i < playerDialogues.Length; i++)
        {
            playerDialogues[i].SetActive(true);
            audioSource.Play();

            textMeshPro = playerDialogues[i].GetComponentInChildren<TMP_Text>();

            fullText = textMeshPro.text; 
            textMeshPro.text = string.Empty; 
            foreach (char letter in fullText)
            {
                textMeshPro.text += letter; 
                yield return new WaitForSeconds(0.03f); 
            }

            yield return new WaitForSeconds(1f);
            playerDialogues[i].SetActive(false);
            audioSource.Stop();
        }

        yield return new WaitForSeconds(2f);
        playerParent.SetActive(false);

        startDialogueCanvas.SetActive(false);

        cutsceneParent.SetActive(true);
        introScene.Play();
    }

    IEnumerator FadeMusic()
    {
        float startVol = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVol * Time.deltaTime / 2f;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVol;
    }

    void OnCutsceneEnd(PlayableDirector director)
    {
        foreach (GameObject obj in gameplayObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        StartCoroutine(FadeAndInstructions());
    }

    IEnumerator FadeAndInstructions()
    {
        fadeFromBlack.SetActive(true);
        yield return new WaitForSeconds(1f);
        instructions.SetActive(true);
        cutsceneParent.SetActive(false);
        cutsceneCam.SetActive(false);
    }
}
