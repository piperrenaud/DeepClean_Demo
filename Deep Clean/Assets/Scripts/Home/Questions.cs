using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class Questions : MonoBehaviour
{
    public GameObject[] questions;
    public GameObject[] endings;
    public GameObject clickAnywhere;
    public GameObject finalScoreText;
    public AudioSource audio;

    private int index = 0;
    private float finalScore = 0;
    private bool ended = false;
    private TMP_Text scoreText;

    void Start()
    {
        foreach (GameObject question in questions)
        {
            question.SetActive(false);
        }

        clickAnywhere.SetActive(false);
        finalScoreText.SetActive(false);

        foreach (GameObject ending in endings)
        {
            ending.SetActive(false);
        }

        questions[0].SetActive(true);

        scoreText = finalScoreText.GetComponent<TMP_Text>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (ended)
            {
                SceneManager.LoadScene("Menu");
            }
        }
    }

    public void AddPoint()
    {
        EvidenceScoringManager.Instance.AddPoint();
    }

    public void NextQuestion()
    {
        StartCoroutine(NextQuestionSequence());
    }

    IEnumerator NextQuestionSequence()
    {
        yield return new WaitForSeconds(1f);

        questions[index].SetActive(false);

        index++;

        if (index < 3)
        {
            questions[index].SetActive(true);
        }
    }

    public void ShowScore()
    {
        //evidence points
        float evidencePoints = EvidenceScoringManager.Instance.GetScore();
        float maxEvidencePoints = 13f;
        float evidenceScore = (evidencePoints / maxEvidencePoints) * 100f;

        //suspicion points
        float suspicion = InventoryManager.Instance.GetSuspicion();
        float suspicionScore = 100f - suspicion;

        //cleanliness score
        float cleanliness = InventoryManager.Instance.GetCleanliness();

        //combination
        finalScore = (evidenceScore * 0.5f) + (suspicionScore * 0.25f) + (cleanliness * 0.25f);

        if (finalScore >= 80f)
        {
            StartCoroutine(EndingScene(0));
        }
        else if (finalScore >= 50f)
        {
            StartCoroutine(EndingScene(1));
        }
        else
        {
            StartCoroutine(EndingScene(2));
        }

        Debug.Log($"Evidence Score: {evidenceScore:0}, Suspicion Score: {suspicionScore:0}, Cleanliness: {cleanliness:0}");
        Debug.Log("Final Score: " + finalScore);
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audio.volume;

        while (audio.volume > 0)
        {
            audio.volume -= startVolume * Time.deltaTime / 1f;
            yield return null;
        }

        audio.Stop();
        audio.volume = startVolume;
    }

    public IEnumerator EndingScene(int index)
    {
        foreach (GameObject question in questions)
        {
            question.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(FadeOut());

        yield return new WaitForSeconds(1f);

        endings[index].SetActive(true);
        scoreText.text = ($"Score: {finalScore:0}");
        finalScoreText.SetActive(true);

        yield return new WaitForSeconds(5f);

        clickAnywhere.SetActive(true);
        ended = true;
    }
}
