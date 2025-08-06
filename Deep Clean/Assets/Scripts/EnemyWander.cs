using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyWander : MonoBehaviour
{
    public List<Transform> wanderPoints;
    public float waitTime = 2f;
    public Animator animator;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            animator.SetBool("Walking", false);
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                SetNewDestination();
                timer = 0;
            }
        } else {
            animator.SetBool("Walking", true);
        }
    }

    void SetNewDestination()
    {
        if (wanderPoints.Count == 0) return;
        currentTarget = wanderPoints[Random.Range(0, wanderPoints.Count)];
        agent.SetDestination(currentTarget.position);
    }
}
