using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class StartGameManager : MonoBehaviour
{
    public PlayableDirector introScene;
    public GameObject startFadeIn;
    public GameObject[] gameplayObjects;
    public GameObject instructions;

    private Animator fadeInAnimator;

    void Start()
    {
        startFadeIn.SetActive(false);
        instructions.SetActive(false);

        foreach (GameObject obj in gameplayObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }            
        }

        fadeInAnimator = startFadeIn.GetComponent<Animator>();

        introScene.stopped += OnCutsceneEnd;
        introScene.Play();
    }

    void OnCutsceneEnd(PlayableDirector director)
    {
        startFadeIn.SetActive(true);

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
        fadeInAnimator.Play("FadeFromBlack");
        yield return new WaitForSeconds(1f);
        instructions.SetActive(true);
    }
}
