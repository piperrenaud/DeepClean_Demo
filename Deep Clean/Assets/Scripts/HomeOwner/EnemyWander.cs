using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.FullSerializer;

public class EnemyWander : MonoBehaviour
{
    [System.Serializable]
    public class TaskPoint
    {
        public Transform transform;
        public string taskName;
        public int roomID;
    }

    [System.Serializable]
    public class ExpectedItem
    {
        public GameObject item;
        public Vector3 originalPos;
        public int roomID;
        public string missingDialogue;
        public bool isMissing = false;
    }
    public List<ExpectedItem> expectedItems;

    [Header("Waypoints/Tasks")]
    public List<TaskPoint> taskPoints;
    public float minTaskTime = 3f;
    public float maxTaskTime = 24f;

    [Header("Movement")]
    public float turnSpeed = 120f; //degrees/sec
    public float walkSpeed = 2f;

    [Header("References")]
    public Animator animator;
    public int playerRoomID;
    public PlayerRoomTracker playerTracker;
    public Transform player;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkingSound;

    [Header("Suspicion/Detection")]
    public float suspicion = 0f;
    public float viewRadius = 20f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask playerMask; //player
    public LayerMask obstructionMask; //walls/funiture

    [Header("UI")]
    public Slider suspicionSlider;
    public TMP_Text suspicionText;
    public GameObject notificationCanvas;
    public GameObject dialogueCanvas;
    public TMP_Text dialogueText;
    public float dialogueDisplayTime = 5f;

    private HashSet<GameObject> seenDroppedItems = new HashSet<GameObject>();
    private NavMeshAgent agent;
    private TaskPoint currentTargetPoint;
    private float timer;
    private bool isTurning = false;
    private Quaternion targetRotation;
    private string turnAnimParam = "";
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private DoorController targetDoor;
    private bool isReacting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        PickNextTask();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        HandleSuspicion();
        UpdateSuspicion();
        CheckForMissingItems();

        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                agent.isStopped = false;
            }
            return;
        }

        if (isTurning)
        {
            //rotate towards next waypoint
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            //check if done turning
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isTurning = false;
                animator.SetBool(turnAnimParam, false);
                StartWalking();
            }
            return;
        }


        if (!agent.pathPending && agent.remainingDistance < 0.5f && currentTargetPoint != null)
        {
            animator.SetBool("Walking", false);
            HandleWalkingSound(false);

            StartCoroutine(DoTaskRoutine(currentTargetPoint.taskName));
            currentTargetPoint = null;
        }

        if (playerTracker != null)
        {
            playerRoomID = playerTracker.currentRoomID;
        }
    }

    IEnumerator DoTaskRoutine(string taskName)
    {
        if (isReacting) yield break;

        //use turning anims to face task points forward dir
        Vector3 forwardDir = currentTargetPoint.transform.forward;
        FaceDirection(forwardDir);

        //wait for tunring to finish
        while (isTurning)
            yield return null;

        //start task anim
        animator.SetBool(taskName, true);
        float waitTime = Random.Range(minTaskTime, maxTaskTime);
        Debug.Log("[EnemyWander] is '" + taskName + "' for " + waitTime + "s");
        yield return new WaitForSeconds(waitTime);
        //stop anim
        animator.SetBool(taskName, false);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (!stateInfo.IsName("Breathing_idle"))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        yield return new WaitForSeconds(0.5f);
        PickNextTask();
    }

    void PickNextTask()
    {
        if (taskPoints.Count == 0)
        {
            return;
        }

        List<TaskPoint> candidates = new List<TaskPoint>();

        if (suspicion >= 100f)
        {
            //always pick players current room
            candidates = taskPoints.Where(p => p.roomID == playerRoomID).ToList();
        }
        else if (suspicion >= 71f)
        {
            //bias towards players room but still slightly random
            float chanceStayNearPlayer = Mathf.InverseLerp(41f, 70f, suspicion); //0-1 scale
            if (Random.value < chanceStayNearPlayer)
            {
                candidates = taskPoints.Where(p => p.roomID == playerRoomID).ToList();

            }
            else
            {
                candidates = taskPoints;
            }
        }
        else
        {
            //0-40 = rnadom
            candidates = taskPoints;
        }

        //if no valid points found
        if (candidates.Count == 0)
        {
            candidates = taskPoints;
        }

        currentTargetPoint = candidates[Random.Range(0, candidates.Count)];

        //get navmesh path
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(currentTargetPoint.transform.position, path);

        //check if door between here and target
        targetDoor = FindDoorAlongPath(path);
        if (targetDoor != null && !targetDoor.IsOpen)
        {
            StartCoroutine(HandleDoorInteraction(targetDoor));
            return;
        }

        if (path.corners.Length < 2)
        {
            //no path, just face target
            FaceDirection(currentTargetPoint.transform.position - transform.position);
            return;
        }

        //second corner = 1st point on path after current pos
        Vector3 dirToFirstCorner = (path.corners[1] - transform.position).normalized;
        FaceDirection(dirToFirstCorner);
    }

    IEnumerator HandleDoorInteraction(DoorController door)
    {
        float waitDistance = 3.5f; // how far from the door to stop while opening
        // Move toward door until at wait distance
        agent.SetDestination(door.transform.position);
        animator.SetBool("Walking", true);

        while (Vector3.Distance(transform.position, door.transform.position) > waitDistance)
        {
            yield return null;
        }

        // Stop and wait while door opens
        agent.isStopped = true;
        animator.SetBool("Walking", false);
        HandleWalkingSound(true);

        door.OpenDoor();
        yield return new WaitForSeconds(1f); // match your "Opening" animation length

        // Resume moving through the door
        agent.isStopped = false;
        agent.SetDestination(currentTargetPoint.transform.position);
        animator.SetBool("Walking", true);
        HandleWalkingSound(true);

        // Wait until the enemy has moved fully past the door before closing
        while (Vector3.Distance(transform.position, door.transform.position) < waitDistance)
        {
            yield return null;
        }

        //door.CloseDoor();
        yield return new WaitForSeconds(1f);
    }

    DoorController FindDoorAlongPath(NavMeshPath path)
    {
        DoorController nearestDoor = null;
        float nearestDist = Mathf.Infinity;

        foreach (Vector3 corner in path.corners)
        {
            foreach (var door in FindObjectsByType<DoorController>(FindObjectsSortMode.None))
            {
                float dist = Vector3.Distance(corner, door.transform.position);
                if (dist < 2.5f)
                {
                    if (dist < nearestDist)
                    {
                        nearestDoor = door;
                        nearestDist = dist;
                    }
                }
            }
        }
        return nearestDoor;
    }

    void FaceDirection(Vector3 direction)
    {
        targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        float signedAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

        if (Mathf.Abs(signedAngle) < 5f)
        {
            StartWalking(); //already facing right way
            return;
        }

        turnAnimParam = signedAngle > 0 ? "TurningLeft" : "TurningRight";
        animator.SetBool(turnAnimParam, true);
        isTurning = true;
    }

    void StartWalking()
    {
        if (currentTargetPoint == null) return;
        agent.SetDestination(currentTargetPoint.transform.position);
        animator.SetBool("Walking", true);
        HandleWalkingSound(true);
    }

    private IEnumerator PauseFor(float seconds, System.Action onComplete = null)
    {
        isPaused = true;
        pauseTimer = seconds;
        agent.isStopped = true;
        animator.SetBool("Walking", false);

        yield return new WaitForSeconds(seconds);

        isPaused = false;
        agent.isStopped = false;
        onComplete?.Invoke();
    }

    public void HandleWalkingSound(bool isWalking)
    {
        if (isWalking)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying && audioSource.clip == walkingSound)
            {
                audioSource.Stop();
            }
        }
    }

    void HandleSuspicion()
    {
        //check dropped objects
        Collider[] dropped = Physics.OverlapSphere(transform.position, viewRadius);
        foreach (var obj in dropped)
        {
            if (obj.CompareTag("DroppedItem") && !seenDroppedItems.Contains(obj.gameObject))
            {
                Vector3 dirToObj = (obj.transform.position - transform.position).normalized;
                float distToObj = Vector3.Distance(transform.position, obj.transform.position);

                if (!isReacting && Vector3.Angle(transform.forward, dirToObj) < viewAngle / 2 && !Physics.Raycast(transform.position, dirToObj, distToObj, obstructionMask))
                {
                    isReacting = true;
                    suspicion += 10f;
                    seenDroppedItems.Add(obj.gameObject);
                    StartCoroutine(ShowDroppedItemDialogue(obj.gameObject));
                    Debug.Log("[EnemyWander] Enemy saw dropped item");

                    //pause then continue
                    StartCoroutine(PauseFor(4f, () =>
                    {
                        isReacting = false;
                        if (currentTargetPoint != null)
                        {
                            StartWalking();
                        }
                    }));
                }
            }
        }

        suspicion = Mathf.Clamp(suspicion, 0, 100f);
    }

    private void CheckForMissingItems()
    {
        foreach (var expected in expectedItems)
        {
            if (expected.isMissing && !isReacting)
            {
                float dist = Vector3.Distance(transform.position, expected.originalPos);
                Vector3 dirToObj = (expected.originalPos - transform.position).normalized;

                if (dist < viewRadius)
                {
                    expected.isMissing = false;
                    Debug.Log("[Enemy Wander] Enemy reacting to missing item: " + expected.item.name);
                    StartCoroutine(ReactToMissingItem(expected));
                }
            }
        }
    }

    public void OnItemTaken(GameObject item)
    {
        string takenID = item.GetComponent<Interactable>().GetObjectID();
        foreach (var expected in expectedItems)
        {
            string expectedID = expected.item.GetComponent<Interactable>().GetObjectID();
            if (expectedID == takenID && !expected.isMissing)
            {
                expected.isMissing = true;
            }
        }
    }

    private IEnumerator ReactToMissingItem(ExpectedItem expected)
    {
        isReacting = true;
        suspicion += 5f;
        Debug.Log("add 5 suspicion");
        StartCoroutine(SuspicionIncreased());

        agent.isStopped = true;
        animator.SetBool("Walking", false);

        //look item direction
        Vector3 dir = expected.originalPos - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        float elapsed = 0f;
        float turnDuration = 0.5f;
        Quaternion startRot = transform.rotation;

        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, lookRotation, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = lookRotation;


        //show dialogue
        if (dialogueCanvas != null && dialogueText != null)
        {
            dialogueText.text = expected.missingDialogue;
            dialogueCanvas.SetActive(true);
            yield return new WaitForSeconds(dialogueDisplayTime);
            dialogueCanvas.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        isReacting = false;
        agent.isStopped = false;

        //resume tasks
        if (currentTargetPoint != null) StartWalking();
    }

    private IEnumerator ShowDroppedItemDialogue(GameObject droppedItem)
    {
        StartCoroutine(SuspicionIncreased());
        if (dialogueCanvas != null && dialogueText != null)
        {
            //get item name from interactable script
            Interactable itemData = droppedItem.GetComponent<Interactable>();
            string itemName = itemData != null ? itemData.itemDescription : "item";

            dialogueText.text = $"Why is my {itemName} on the ground...";
            dialogueCanvas.SetActive(true);

            yield return new WaitForSeconds(dialogueDisplayTime);
            dialogueCanvas.SetActive(false);
        }
    }

    public IEnumerator SuspicionIncreased()
    {
        notificationCanvas.SetActive(true);
        yield return new WaitForSeconds(1f);
        notificationCanvas.SetActive(false);
    }

    void UpdateSuspicion()
    {
        if (suspicionSlider != null)
        {
            suspicionSlider.value = suspicion;
        }

        if (suspicionText != null)
        {
            if (suspicion <= 0) suspicionText.text = "0%";
            else
            {
                suspicionText.text = $"{suspicion:0}%";
            }
        }
    }

    public void HandlePhotoTaken()
    {
        if (player == null) return;

        //raise suspicion in players in view/radar
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        Vector3 dirToPlayer = (player.position - eyePos).normalized;

        //check if inside radius (2.5f instead of view radius cuz idk somethings wrong)
        if (distToPlayer < 2.5f && !Physics.Raycast(eyePos, dirToPlayer, distToPlayer, obstructionMask))
        {
            //only react if in view
            suspicion += 5f;
            suspicion = Mathf.Clamp(suspicion, 0, 100f);
            StartCoroutine(SuspicionIncreased());
            OnPlayerCaught();
            Debug.Log("Enemy saw player take a photo");
        }
    }

    public void OnPlayerCaught()
    {
        StopAllCoroutines();
        agent.isStopped = true;

        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(param.name, false);
            }
        }

        isTurning = false;
        animator.SetTrigger("ForceStand");
        StartCoroutine(PhotoCaughtRoutine());
    }

    public IEnumerator PhotoCaughtRoutine()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
        float elapsed = 0f;
        float turnDuration = 0.5f;
        Quaternion startRotation = transform.rotation;

        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, lookRotation, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = lookRotation;

        //show dialgue
        if (dialogueCanvas != null && dialogueText != null)
        {
            dialogueText.text = "Are you taking photos?";
            dialogueCanvas.SetActive(true);
            yield return new WaitForSeconds(5f);
            dialogueCanvas.SetActive(false);
        }

        //resume normal behavior
        PickNextTask();
        agent.isStopped = false;
    }

    public IEnumerator FacePlayer()
    {
        if (player == null) yield break;

        agent.isStopped = true;
        isReacting = true;
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(param.name, false);
            }
        }
        animator.SetTrigger("ForceStand");

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;
        float turnDuration = 0.5f;

        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
        StartCoroutine(WalkToPlayer());
    }

    public IEnumerator WalkToPlayer(float stopDistance = 3f)
    {
        if (player == null) yield break;

        agent.isStopped = false;
        animator.SetBool("Walking", true);
        HandleWalkingSound(true);

        //target position infront of player
        Vector3 dirToEnemy = (transform.position - player.position).normalized;
        Vector3 desiredPos = player.position + dirToEnemy * stopDistance;
        desiredPos.y = player.position.y;

        //raycast from player to enemy to check for walls
        Vector3 rayDir = (desiredPos - player.position).normalized;
        float rayDist = Vector3.Distance(player.position, desiredPos);

        if (Physics.Raycast(player.position + Vector3.up * 0.5f, rayDir, out RaycastHit hit, rayDist, LayerMask.GetMask("Obstruction")))
        {
            //if blocked, move enemy 
            desiredPos = hit.point - rayDir * 0.5f;
            desiredPos.y = transform.position.y;
        }

        agent.SetDestination(desiredPos);

        while (agent.pathPending || agent.remainingDistance > 0.2f)
        {
            // rotate towards player
            Vector3 dir = (player.position - transform.position).normalized;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed / 100f);
            }

            yield return null;
        }

        agent.isStopped = true;
        animator.SetBool("Walking", false);
        HandleWalkingSound(false);

        // dialogue moment
        if (dialogueCanvas != null && dialogueText != null)
        {
            dialogueText.text = "You're moving on already? This room's still dirty. Clean it up a bit first.";
            dialogueCanvas.SetActive(true);
            yield return new WaitForSeconds(5f);
            dialogueCanvas.SetActive(false);
        }

        // unblock player
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.UnblockMovement();

        // resume task
        isReacting = false;
        PickNextTask();
        agent.isStopped = false;
    }

    void OnDrawGizmos()
    {
        if (agent == null || agent.path == null) return;

        Gizmos.color = Color.green;
        Vector3[] corners = agent.path.corners;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
            Gizmos.DrawSphere(corners[i], 0.2f);
        }
        if (corners.Length > 0)
        {
            Gizmos.DrawSphere(corners[corners.Length - 1], 0.2f);
        }

        //draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        //draw detection cone
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        //highlight dropped items in view
        Gizmos.color = Color.magenta;
        var dropped = GameObject.FindGameObjectsWithTag("DroppedItem");
        foreach (var obj in dropped)
        {
            if (Vector3.Distance(transform.position, obj.transform.position) <= viewRadius)
            {
                Gizmos.DrawWireSphere(obj.transform.position, 0.3f);
            }
        }
    }
}