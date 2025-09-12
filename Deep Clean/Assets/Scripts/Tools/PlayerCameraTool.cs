using UnityEngine;
using System.Collections;

public class PlayerCameraTool : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public GameObject cameraObject;
    public GameObject cameraUI;
    public GameObject gameUI;
    public GameObject crosshair;

    private bool isHeld = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (!isHeld)
            {
                StartCoroutine(PlayerToolManager.Instance.SwitchTool(() =>  PickupRoutine()));
            }
            else
            {
                StartCoroutine(PutdownRoutine());
            }
        }
    }

    public bool IsHeld() => isHeld;

    public IEnumerator PickupRoutine()
    {
        animator.SetTrigger("PickupCam");
        cameraObject.SetActive(true);
        isHeld = true;

        yield return new WaitForSeconds(0.2f);
        cameraUI.SetActive(true);
        gameUI.SetActive(false);
        crosshair.SetActive(false);
    }

    public IEnumerator PutdownRoutine()
    {
        animator.SetTrigger("PutdownCam");
        yield return new WaitForSeconds(0.05f);

        cameraObject.SetActive(false);
        cameraUI.SetActive(false);
        gameUI.SetActive(true);
        crosshair.SetActive(true);
        isHeld = false;
    }
}
