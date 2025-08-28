using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;
    public PlayerMovement playerMovement;
    public PlayerCam playerCam;

    private Interactable currentInteractable;
    private Interactable heldInteractable;


    void Update()
    {
        if (heldInteractable == null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Interactable interact = hit.collider.GetComponent<Interactable>();

                if (interact != currentInteractable)
                {
                    if (currentInteractable != null)
                        currentInteractable.Highlight(false);

                    currentInteractable = interact;

                    if (currentInteractable != null)
                        currentInteractable.Highlight(true);
                }

                if (Input.GetMouseButtonDown(1) && interact != null) //right click pickup
                {

                    //only pick up the object if it can be inspected
                    if (interact.GetComponent<InteractableInspection>() != null)
                    {
                        heldInteractable = interact;
                        heldInteractable.PickUpObject(cam.transform);

                        //turn off movement while inspecting
                        playerMovement.enabled = false;
                        playerCam.enabled = false;
                    }
                }
            }
            else
            {
                if (currentInteractable != null)
                {
                    currentInteractable.Highlight(false);
                    currentInteractable = null;
                }
            }
        }
        else 
        {
            //already holding something
            heldInteractable.RotateObject();

            if (Input.GetMouseButtonDown(1)) //rightclick to drop
            {
                heldInteractable.DropObject();
                heldInteractable = null;

                //enalbe movement again
                playerMovement.enabled = true;
                playerCam.enabled = true;
            }
        }
    }
}