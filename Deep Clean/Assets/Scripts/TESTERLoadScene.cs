using UnityEngine;
using UnityEngine.SceneManagement;

public class TESTERLoadScene : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if (hit.collider.gameObject.name == "Switch Scenes")
            {
                if (Input.GetMouseButtonDown(0)) 
                {
                    SceneManager.LoadScene("Home");
                }
            }
        }
    }
}
