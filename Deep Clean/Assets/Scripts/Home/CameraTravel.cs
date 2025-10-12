using UnityEngine;
using System.Collections;

public class CameraTravel : MonoBehaviour
{
    [Header("Travel Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform viewpoint;

    private bool isTravelling = false;
    private PlayerMovement playerMovement;
    private PlayerCam playerCam;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerCam = gameObject.GetComponent<PlayerCam>();
    }

    public void TravelToViewPoint()
    {
        if (!isTravelling)
        {
            StartCoroutine(MoveCamera());
        }
    }

    private IEnumerator MoveCamera()
    {
        isTravelling = true;

        playerMovement.enabled = false;
        playerCam.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Animator anim = player.GetComponent<Animator>();
        anim.enabled = false;

        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        Vector3 targetPos = viewpoint.position;
        Quaternion targetRot = viewpoint.rotation;

        float journey = 0f;

        while (journey < 1f)
        {
            journey += Time.deltaTime * moveSpeed;

            player.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, journey));
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, journey));

            yield return null;
        }

        player.transform.position = targetPos;
        player.transform.rotation = targetRot;

        isTravelling = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return new WaitForSeconds(0.5f);
        
        Computer.Instance.TurnOn();
    }
}
