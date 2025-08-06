using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Transform orientation;

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float moveX =0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;

        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;

        Vector3 move = (forward * moveZ + right * moveX).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}