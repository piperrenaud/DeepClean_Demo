using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

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
    public GameObject gameUI;

    [Header("UI Settings")]
    public Transform buttonContainer;
    public GameObject questionButtonPrefab;
    public GameObject scrollPanel;

    [Header("Dialogue References")]
    public GameObject enemyDialogueBox;
    public TMPro.TMP_Text enemyDialogueText;
    public float typingSpeed = 0.02f;
    public float dialogueDuration = 4f;

    private Coroutine typingRoutine;
    private bool isTyping;
    private string currentFullText;

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

        if (enemyDialogueBox != null && enemyDialogueBox.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                FinishTypingInstantly();
            }
            else
            {
                enemyDialogueBox.SetActive(false);
                if (scrollPanel != null)
                    scrollPanel.SetActive(true);
            }
        }
    }

    void StartInteraction()
    {
        inConversation = true;

        //disable gameUI
        gameUI.SetActive(false);

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

        //show question scroll panel
        if (scrollPanel != null)
            scrollPanel.SetActive(true);

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
        //hide scroll panel while dialogue players
        if (scrollPanel != null)
            scrollPanel.SetActive(false);
        
        //show enemy response 
        ShowEnemyDialogue(obj.GetExplanation());

        //update interactable
        if (!string.IsNullOrEmpty(obj.explanation))
        {
            if (!obj.itemDescription.Contains("Explanation:"))
            {
                obj.itemDescription += "\n\nExplanation: "+ obj.explanation;
            }
        }

        //update inventory 
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UpdateItemWithExplanation(obj.GetObjectID(), obj.explanation);
        }
    }

    private void ShowEnemyDialogue(string text)
    {
        if (enemyDialogueBox == null || enemyDialogueText == null) return;

        enemyDialogueBox.SetActive(true);
        currentFullText = text;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        enemyDialogueText.text = string.Empty;

        foreach (char letter in fullText)
        {
            enemyDialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);

            if (!isTyping) yield break; //break early if skipping
        }

        isTyping = false;

        yield return new WaitForSeconds(dialogueDuration);

        enemyDialogueBox.SetActive(false);
        typingRoutine = null;

        if (scrollPanel != null)
            scrollPanel.SetActive(true); //renable quetsions
    }

    private void FinishTypingInstantly()
    {
        isTyping = false;
        enemyDialogueText.text = currentFullText;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDuration);
        enemyDialogueBox.SetActive(false);
        typingRoutine = null;

        if (scrollPanel != null)
            scrollPanel.SetActive(true);
    }

    private IEnumerator ReEnableScrollPanelAfterDelay()
    {
        yield return new WaitForSeconds(4f);
        if (scrollPanel != null)
            scrollPanel.SetActive(true);
    }

    public void EndInteraction()
    {
        //hide scroll pane
        if (scrollPanel != null)
            scrollPanel.SetActive(false);

        ShowEnemyDialogue("Okay, let me know if you have more questions.");

        //start coroutine that waits for dialogue to finish
        StartCoroutine(WaitForClosingDialogue());
    }

    private IEnumerator WaitForClosingDialogue()
    {
        //wait while text is typing
        while (isTyping)
            yield return null;

        //wait for autoclose or until player clicks
        float timer = 0f;
        bool skipped = false;

        while (timer < dialogueDuration && !skipped)
        {
            if (Input.GetMouseButtonDown(0))
            {
                skipped = true;
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        //hide dialogue box
        if (enemyDialogueBox != null)
            enemyDialogueBox.SetActive(false);

        //finish
        FinalizeEndInteraction();
    }

    private void FinalizeEndInteraction()
    {
        inConversation = false;

        //hide dialogue ui
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        //clear question buttons
        foreach (Transform child in buttonContainer)
        {
            if (!child.CompareTag("PersistentButton"))
                Destroy(child.gameObject);
        }

        //enable gameUI
        gameUI.SetActive(true);

        //renable player movement
        playermovement.enabled = true;
        playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //resume enemy AI
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
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
