using UnityEngine;
using System.Collections;
using TMPro;

public class Questions : MonoBehaviour
{
    public GameObject[] questions;
    public TMP_Text totalPointText;

    private int index = 0;

    void Start()
    {
        questions[0].SetActive(true);
        questions[1].SetActive(false);
        questions[2].SetActive(false);
        questions[3].SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

        questions[index].SetActive(true);
    }

    public void ShowScore()
    {
        float score = EvidenceScoringManager.Instance.GetScore();
        totalPointText.text = score.ToString();
    }
}
