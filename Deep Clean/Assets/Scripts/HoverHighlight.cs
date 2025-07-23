using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;

    private Interactable currentInteractable;


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