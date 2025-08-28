using UnityEngine; 

public class FaceCamera : MonoBehaviour 
{ 
    private Camera mainCamera; 
    private Vector3 originalScale;
    
    void Start() 
    { 
        mainCamera = Camera.main; 
        originalScale = transform.localScale;
    } 
    
    void LateUpdate() 
    { 
        if (mainCamera != null) 
        { 
            transform.LookAt(
                transform.position + mainCamera.transform.rotation * Vector3.forward, 
                mainCamera.transform.rotation * Vector3.up
            ); 

            transform.localScale = originalScale;
        } 
    } 
}