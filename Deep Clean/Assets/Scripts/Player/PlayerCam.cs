using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private bool isForcedLook = false;
    private Transform lookTarget;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    void Update()
    {
        //turn off movement if invetory open
        if (InventoryToggle.inventoryOpen) return;

        if (isForcedLook && lookTarget != null)
        {
            Vector3 dir = (lookTarget.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);

            Vector3 euler = transform.rotation.eulerAngles;
            orientation.rotation = Quaternion.Euler(0, euler.y, 0);
            return;
        }
        
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void ForceLookAt(Transform target)
    {
        isForcedLook = true;
        lookTarget = target;
    }

    public void ReleaseLook()
    {
        isForcedLook = false;
        lookTarget = null;
    }
}
