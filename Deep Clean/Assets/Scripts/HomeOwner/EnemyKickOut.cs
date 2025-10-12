using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Playables;

public class EnemyKickOut : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeController timeController;
    [SerializeField] private GameObject playerParent;
    [SerializeField] private Transform player;
    [SerializeField] private EnemyWander enemyWander;
    [SerializeField] private GameObject enemyParent;
    [SerializeField] private Transform enemyHead;
    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField] private PlayerCam playerCamera;


    [Header("Cutscenes + Dialogue")]
    [SerializeField] private GameObject cutsceneParent;
    [SerializeField] private PlayableDirector leavingNice;
    [SerializeField] private string leavingNiceDialogue;
    [SerializeField] private PlayableDirector leavingBad;
    [SerializeField] private string leavingBadDialogue;
    [SerializeField] private PlayableDirector kickedOut;
    [SerializeField] private string kickedOutDialogue;

    [Header("Settings")]
    [SerializeField] private float endOfDayHour = 20f;

    private bool hasTriggered = false;
    private bool endOfDay;
    private bool maxSuspicion;
    private bool houseCleaned;
    private string dialogue;
    private Animator enemyAnimator;

    void Start()
    {
        cutsceneParent.SetActive(false);

        enemyAnimator = enemyParent.GetComponent<Animator>();
    }

    void Update()
    {
        if (hasTriggered) return;
        if (timeController == null || player == null || enemyWander == null) return;

        endOfDay = timeController.GetCurrentTime().Hour >= endOfDayHour;
        maxSuspicion = enemyWander.suspicion >= 100;
        houseCleaned = GameManager.Instance.IsCleaningComplete();

        if (endOfDay || maxSuspicion || houseCleaned)
        {
            hasTriggered = true;

            if (endOfDay) 
            {
                dialogue = leavingBadDialogue;
            }
            if (maxSuspicion) 
            {
                dialogue = kickedOutDialogue;
            }
            if (houseCleaned) 
            {
                dialogue = leavingNiceDialogue;
            }

            StartCoroutine(StartKickOut(dialogue));
        }
    }

    private IEnumerator StartKickOut(string dialogue)
    {
        playerCamera.ForceLookAt(enemyHead);
        yield return new WaitForSeconds(1f);

        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCamera != null) playerCamera.enabled = false;

        yield return StartCoroutine(enemyWander.FacePlayer(dialogue));
        enemyAnimator.Play("Breathing_idle");

        if (houseCleaned)
        {
            yield return new WaitForSeconds(8f);
            cutsceneParent.SetActive(true);
            enemyParent.SetActive(false);
            playerParent.SetActive(false);
            Debug.Log("[EnemyKickOut] Leaving cause house is cleaned.");
            leavingNice.Play();
        }
        else if (maxSuspicion)
        {
            yield return new WaitForSeconds(8f);
            cutsceneParent.SetActive(true);
            enemyParent.SetActive(false);
            playerParent.SetActive(false);
            Debug.Log("[EnemyKickOut] Leaving cause suspicion too high.");
            kickedOut.Play();
        }
        else if (endOfDay)
        {
            yield return new WaitForSeconds(8f);
            cutsceneParent.SetActive(true);
            enemyParent.SetActive(false);
            playerParent.SetActive(false);
            Debug.Log("[EnemyKickOut] Leaving cause the days over.");
            leavingBad.Play();
        }
    }
}
