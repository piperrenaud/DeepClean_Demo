using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyWander : MonoBehaviour
{
    public List<Transform> wanderPoints;
    public float waitTime = 2f;
    public float turnSpeed = 120f; //degrees/sec
    public Animator animator;

    private NavMeshAgent agent;
    private Transform currentTarget;
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

        //movement state
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            animator.SetBool("Walking", false);
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                PrepareTurnToNewDestination();
                timer = 0;
            }
        }
    }

    void PrepareTurnToNewDestination()
    {
        if (wanderPoints.Count == 0) return;

        currentTarget = wanderPoints[Random.Range(0, wanderPoints.Count)];

        //get navmesh path
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(currentTarget.position, path);

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
            FaceDirection(currentTarget.position - transform.position);
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
        agent.SetDestination(currentTarget.position);
        animator.SetBool("Walking", true);

        // Wait until the enemy has moved fully past the door before closing
        while (Vector3.Distance(transform.position, door.transform.position) < waitDistance + 1f)
        {
            yield return null;
        }

        door.CloseDoor();
        yield return new WaitForSeconds(1f);
    }

    DoorController FindDoorAlongPath(NavMeshPath path)
    {
        foreach (var door in FindObjectsOfType<DoorController>())
        {
            if (Vector3.Distance(transform.position, door.transform.position) < 5f)
            {
                return door;
            }
        }
        return null;
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

        if (signedAngle > 0)
            turnAnimParam = "TurningLeft";
        else
            turnAnimParam = "TurningRight";

        animator.SetBool(turnAnimParam, true);
        isTurning = true;
    }

    void StartWalking()
    {
        agent.SetDestination(currentTarget.position);
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