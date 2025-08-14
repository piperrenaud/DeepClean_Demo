using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyWander : MonoBehaviour
{
    [System.Serializable]
    public class TaskPoint
    {
        public Transform transform;
        public string taskName;
        public int roomID;
    }


    [Header("Waypoints/Tasks")]
    public List<TaskPoint> taskPoints;
    public float minTaskTime = 3f;
    public float maxTaskTime = 24f;

    [Header("Movement")]
    public float turnSpeed = 120f; //degrees/sec

    [Header("References")]
    public Animator animator;
    public float suspicion = 0f;
    public int playerRoomID;

    private NavMeshAgent agent;
    private TaskPoint currentTargetPoint;
    private float timer;
    private bool isTurning = false;
    private Quaternion targetRotation;
    private string turnAnimParam = "";
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private DoorController targetDoor;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PickNextTask();
    }

    void Update()
    {
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

        //patrol randomly if suspicion is 0
        if (suspicion == 0f)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f && currentTargetPoint != null)
            {
                animator.SetBool("Walking", false);
                
                StartCoroutine(DoTaskRoutine(currentTargetPoint.taskName));
                currentTargetPoint = null;
            }
        }
    }

    IEnumerator DoTaskRoutine(string taskName)
    {
        //use turning anims to face task points forward dir
        Vector3 forwardDir = currentTargetPoint.transform.forward;
        FaceDirection(forwardDir);

        //wait for tunring to finish
        while (isTurning)
            yield return null;

        //start task anim
        animator.SetBool(taskName, true);
        float waitTime = Random.Range(minTaskTime, maxTaskTime);
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

    void PickNextTask(bool preferNearPlayer = false)
    {
        if (taskPoints.Count == 0) return;

        float biasChance = Mathf.Clamp01(suspicion);
        List<TaskPoint> candidates;

        if (Random.value < biasChance)
        {
            candidates = taskPoints.Where(p => p.roomID == playerRoomID).ToList();
            if (candidates.Count == 0) candidates = taskPoints;
        }
        else 
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

        door.OpenDoor();
        yield return new WaitForSeconds(1f); // match your "Opening" animation length

        // Resume moving through the door
        agent.isStopped = false;
        agent.SetDestination(currentTargetPoint.transform.position);
        animator.SetBool("Walking", true);

        // Wait until the enemy has moved fully past the door before closing
        while (Vector3.Distance(transform.position, door.transform.position) < waitDistance)
        {
            yield return null;
        }

        door.CloseDoor();
        yield return new WaitForSeconds(1f);
    }

    DoorController FindDoorAlongPath(NavMeshPath path)
    {
        DoorController nearestDoor = null;
        float nearestDist = Mathf.Infinity;

        foreach (Vector3 corner in path.corners)
        {
            foreach (var door in FindObjectsOfType<DoorController>())
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
    }

    public void PauseMovement(float seconds)
    {
        isPaused = true;
        pauseTimer = seconds;
        agent.isStopped = true;
        animator.SetBool("Walking", false);
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
    }
}