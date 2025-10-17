using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

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
    [SerializeField] private MusicManager audioManager;


    [Header("Cutscenes + Dialogue")]
    [SerializeField] private GameObject cutsceneParent;
    [SerializeField] private GameObject cutsceneCam;
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
        cutsceneCam.SetActive(false);

        enemyAnimator = enemyParent.GetComponentInChildren<Animator>();
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

            StartKickOut(dialogue);
        }
    }

    private void StartKickOut(string dialogue)
    {
        playerCamera.ForceLookAt(enemyHead);
        playerCamera.SetCanLook(false);
        if (playerMovement != null) playerMovement.enabled = false;

        StartCoroutine(enemyWander.FacePlayer(dialogue, true));
    }

    public void KickedOut()
    {
        enemyAnimator.Play("Breathing_idle");

        PlayableDirector currentCutscene = null;
        cutsceneParent.SetActive(true);
        cutsceneCam.SetActive(true);
        enemyParent.SetActive(false);
        playerParent.SetActive(false);

        if (houseCleaned)
        {
            Debug.Log("[EnemyKickOut] Leaving cause house is cleaned.");
            currentCutscene = leavingNice;
        }
        else if (maxSuspicion)
        {
            Debug.Log("[EnemyKickOut] Leaving cause suspicion too high.");
            currentCutscene = kickedOut;
        }
        else if (endOfDay)
        {
            Debug.Log("[EnemyKickOut] Leaving cause the days over.");
            currentCutscene = leavingBad;
        }

        if (currentCutscene != null)
        {
            currentCutscene.stopped += OnCutsceneFinished;
            StartCoroutine(audioManager.FadeOut());
            currentCutscene.Play();
        }
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        director.stopped -= OnCutsceneFinished;
        SceneManager.LoadScene("Home");
    }
}
