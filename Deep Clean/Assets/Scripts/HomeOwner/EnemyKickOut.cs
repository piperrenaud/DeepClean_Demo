using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyKickOut : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeController timeController;
    [SerializeField] private Transform player;
    [SerializeField] private EnemyWander enemyWander;
    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField] private MonoBehaviour playerCamera;

    [Header("Settings")]
    [SerializeField] private float endOfDayHour = 20f;

    private bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;
        if (timeController == null || player == null || enemyWander == null) return;

        bool endOfDay = timeController.GetCurrentTime().Hour >= endOfDayHour;
        bool maxSuspicion = enemyWander.suspicion >= 100;

        if (endOfDay || maxSuspicion)
        {
            hasTriggered = true;

            string dialogue = endOfDay
                ? "Okay, you've been here long enoug. I want you to leave now."
                : "I've had enough! Stop snooping around my things and GET OUT!";

            Debug.Log("[EnemyKickOut] Kickout triggered. Reason: " + dialogue);
            StartCoroutine(StartKickOut(dialogue));
        }
    }

    private IEnumerator StartKickOut(string dialogue)
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCamera != null) playerCamera.enabled = false;

        yield return StartCoroutine(enemyWander.FacePlayer(dialogue));

        Debug.Log("[EnemyKickOutCutscene] kickout sequence complete");
    }
}
