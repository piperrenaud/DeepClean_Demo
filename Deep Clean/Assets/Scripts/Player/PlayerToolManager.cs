using UnityEngine;
using System;
using System.Collections;

public class PlayerToolManager : MonoBehaviour
{
    public static PlayerToolManager Instance { get; private set; }

    [Header("References")]
    public PlayerCleaningTool cleaningTool;
    public PlayerRubbishTool rubbishTool;
    public PlayerCameraTool cameraTool;

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator SwitchTool(System.Func<IEnumerator> pickupRoutineFunc)
    {
        // Putdown cleaning if active
        if (cleaningTool.HasTools())
        {
            yield return cleaningTool.StartCoroutine(cleaningTool.PutdownRoutine());
        }

        // Putdown rubbish if active
        if (rubbishTool.HasOpenBag() || rubbishTool.CurrentBag.isTied)
        {
            yield return rubbishTool.StartCoroutine(rubbishTool.PutdownRoutine());
        }

        // Put down camera if held
        if (cameraTool.IsHeld())
        {
            yield return cameraTool.StartCoroutine(cameraTool.PutdownRoutine());
        }

        // Now actually start the pickup
        yield return StartCoroutine(pickupRoutineFunc());
    }

}
