using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator animator;
    public bool IsOpen { get; private set; } = false;

    public void OpenDoor()
    {
        if (!IsOpen)
        {
            animator.SetBool("Opening", true);
            animator.SetBool("Closing", false);
            IsOpen = true;
        }
    }

    public void CloseDoor()
    {
        if (IsOpen)
        {
            animator.SetBool("Closing", true);
            animator.SetBool("Opening", false);
            IsOpen = false;
        }
    }
}
