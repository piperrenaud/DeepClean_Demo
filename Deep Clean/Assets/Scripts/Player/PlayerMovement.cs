using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float crouchSpeed = 1f;
    public Transform orientation;
    public Animator animator;
    public Animator camAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkingSound;

    [HideInInspector] public float currentSpeed; 

    void Start()
    {
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouch();

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            orientation.rotation,
            Time.deltaTime * 10f
        );
    }

    void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;

        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;

        Vector3 move = (forward * moveZ + right * moveX).normalized;

        //move player
        transform.Translate(move * currentSpeed * Time.deltaTime, Space.World);

        //walking anim
        bool isWalking = move.magnitude > 0f;
        animator.SetBool("isWalking", isWalking);

        //audio
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

    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            camAnimator.SetBool("isCrouching", true);
            currentSpeed = crouchSpeed;
        }
        else
        {
            camAnimator.SetBool("isCrouching", false);
            currentSpeed = moveSpeed;
        }
    }
}