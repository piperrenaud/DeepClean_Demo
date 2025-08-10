using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;

    private Interactable currentInteractable;
    private Interactable heldInteractable;


    void Update()
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

            if (Input.GetMouseButtonDown(1) && interact != null) //pickup when left mouse is held
            {
                if (heldInteractable == null)
                {
                    heldInteractable = interact;
                    heldInteractable.PickUpObject(cam.transform);
                }
                else 
                {
                    heldInteractable.DropObject();
                    heldInteractable = null;
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
}