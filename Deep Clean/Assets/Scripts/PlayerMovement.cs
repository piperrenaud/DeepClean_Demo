using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Transform orientation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        //calc movement direction relative
        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;

        //movmenet based on camera orientation
        Vector3 move = (forward * moveZ + right * moveX).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}