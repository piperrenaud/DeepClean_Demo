using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyInteraction : MonoBehaviour
{
    [Header("References")]
    public EnemyWander enemyWander;
    public Animator enemyAnimator;
    public Transform player;
    public PlayerMovement playermovement;
    public PlayerCam playerCam;
    public GameObject dialogueUI;
    public Transform lookTarget;
    public EnemyDialogue enemyDialogue;

    [Header("UI Settings")]
    public Transform buttonContainer;
    public GameObject questionButtonPrefab;

    [Header("Settings")]
    public float interactRange = 2f;

    private bool inConversation = false;

    void Update()
    {
        if (!inConversation && Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= interactRange)
            {
                StartInteraction();
            }
        }

        if (inConversation)
        {
            LockCameraOnEnemy();
            FacePlayer();
            ForceIdle();
        }
    }

    void StartInteraction()
    {
        inConversation = true;

        //stop enemy movement
        enemyWander.enabled = false;
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.isStopped = true;

        //disable player movement
        playermovement.enabled = false;
        playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //show dialogue UI
        dialogueUI.SetActive(true);

        //spawn question buttons
        SpawnQuestionButtons();

        ResetAllAnimations();
        ForceIdle();
    }

    private void SpawnQuestionButtons()
    {
        //clear previous buttons
        foreach (Transform child in buttonContainer)
        {
            if (!child.CompareTag("PersistentButton"))
                Destroy(child.gameObject);
        }

        List<Interactable> discoveredObjects = enemyDialogue.GetDiscoveredObjects();

        if (discoveredObjects.Count == 0)
        {
            Debug.Log("No discovered objects yet");
            return;
        }

        foreach (var obj in discoveredObjects)
        {
            //onyl spawn if questionName not empty
            if (string.IsNullOrEmpty(obj.questionName)) continue;

            GameObject buttonObj = Instantiate(questionButtonPrefab, buttonContainer);
            TMPro.TMP_Text buttonText = buttonObj.GetComponentInChildren<TMPro.TMP_Text>();
            
            buttonText.text = obj.questionName;

            //add click event
            buttonObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                AskAboutObject(obj);
            });
        }
    }

    private void AskAboutObject(Interactable obj)
    {
        //show enemy response in same dialogue ui
        InteractableDialogue dialogueComponent = dialogueUI.GetComponent<InteractableDialogue>();
        if (dialogueComponent != null)
        {
            dialogueComponent.ShowDialogue(obj.GetExplanation());
        }
    }

    public void EndInteraction()
    {
        inConversation = false;

        //hide dialogue UI
        dialogueUI.SetActive(false);

        //clear question buttons
        foreach (Transform child in buttonContainer)
        {
            if (!child.CompareTag("PersistentButton"))
                Destroy(child.gameObject);
        }

        //re enable player movement
        playermovement.enabled = true;

        //re enable camera look
        playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        //resume enemy AI
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.isStopped = false;
        enemyWander.enabled = true;
    }

    private void LockCameraOnEnemy()
    {
        if (lookTarget == null) return;

        //make camera look at enemy
        Vector3 lookDir = lookTarget.position - playerCam.transform.position;
        Quaternion lookRot = Quaternion.LookRotation(lookDir, Vector3.up);
        playerCam.transform.rotation = Quaternion.Slerp(
            playerCam.transform.rotation,
            lookRot,
            Time.deltaTime * 5f //smoothing
        );
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
    }

    private void ResetAllAnimations()
    {
        foreach (AnimatorControllerParameter param in enemyAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                enemyAnimator.SetBool(param.name, false);
            }
            else if (param.type == AnimatorControllerParameterType.Trigger)
            {
                enemyAnimator.ResetTrigger(param.name);
            }
        }
    }

    private void ForceIdle()
    {
        enemyAnimator.SetBool("Walking", false);
        enemyAnimator.Play("Breathing_idle");
    }
}
