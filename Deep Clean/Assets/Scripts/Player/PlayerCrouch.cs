using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public Animator animator;

    [Header("Crouch Settings")]
    public float crouchSpeed = 1.5f;
    
    private float normalSpeed;

    void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }
        normalSpeed = playerMovement.moveSpeed;
    }

    void Update()
    {
        HandleCrouch();
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerMovement.currentSpeed = crouchSpeed;
            animator.SetBool("isCrouching", true);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerMovement.currentSpeed = normalSpeed;
            animator.SetBool("isCrouching", false);
        }
    }
}
