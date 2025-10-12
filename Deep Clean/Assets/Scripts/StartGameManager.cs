using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class StartGameManager : MonoBehaviour
{
    public PlayableDirector introScene;
    public GameObject fadeFromBlack;
    public GameObject[] gameplayObjects;
    public GameObject instructions;
    public GameObject cutsceneParent;

    void Start()
    {
        cutsceneParent.SetActive(true);
        fadeFromBlack.SetActive(false);
        instructions.SetActive(false);

        foreach (GameObject obj in gameplayObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }            
        }

        introScene.stopped += OnCutsceneEnd;
        introScene.Play();
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
    }
}
