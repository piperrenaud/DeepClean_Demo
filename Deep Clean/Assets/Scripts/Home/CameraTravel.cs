using UnityEngine;
using System.Collections;

public class CameraTravel : MonoBehaviour
{
    [Header("Travel Settings")]
    [SerializeField] private Transform viewpoint;
    [SerializeField] private Transform playerViewPoint;

    private PlayerMovement playerMovement;
    private PlayerCam playerCam;
    private GameObject player;

    private Transform originalParent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerCam = gameObject.GetComponent<PlayerCam>();

        originalParent = transform.parent;
    }

    public void TravelToViewPoint()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerMovement.enabled = false;
        playerCam.enabled = false;

        transform.parent = viewpoint;
        gameObject.transform.localRotation = Quaternion.identity;

        player.transform.position = playerViewPoint.position;

        Computer.Instance.TurnOn();
    }

    public void ExitComputer()
    {
        Computer.Instance.TurnOff();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerMovement.enabled = true;
        playerCam.enabled = true;

        transform.parent = originalParent;
    }
}
