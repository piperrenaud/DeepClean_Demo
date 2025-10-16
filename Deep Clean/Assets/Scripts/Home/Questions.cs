using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class Questions : MonoBehaviour
{
    public GameObject[] questions;
    public GameObject[] endings;
    public GameObject[] clickAnywheres;

    private int index = 0;
    private bool ended = false;

    void Start()
    {
        foreach (GameObject question in questions)
        {
            question.SetActive(false);
        }

        foreach (GameObject clickAnywhere in clickAnywheres)
        {
            clickAnywhere.SetActive(false);
        }

        foreach (GameObject ending in endings)
        {
            ending.SetActive(false);
        }

        questions[0].SetActive(true);

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
        float finalScore = (evidenceScore * 0.5f) + (suspicionScore * 0.25f) + (cleanliness * 0.25f);

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

    public IEnumerator EndingScene(int index)
    {
        foreach (GameObject question in questions)
        {
            question.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(1f);

        endings[index].SetActive(true);

        yield return new WaitForSeconds(5f);

        clickAnywheres[index].SetActive(true);
        ended = true;
    }
}
