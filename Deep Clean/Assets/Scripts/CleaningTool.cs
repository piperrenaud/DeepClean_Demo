using UnityEngine;

public class CleaningTool : MonoBehaviour
{
    [Header("Tool Settings")]
    public float effectiveness = 1f; 
    public float detectionRadius = 1.5f;
    public LayerMask dirtLayerMask = 1; 
    
    [Header("Visual Feedback")]
    public Transform toolTip; 
    public ParticleSystem toolParticles; 
    public Animator toolAnimator; 
    
    private Camera playerCamera;
    private DirtSpot currentDirtSpot;
    private bool isCleaning = false;
    private Vector3 lastMousePosition;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
    }
    
    void Update()
    {
        HandleInput();
        UpdateToolPosition();
        CheckForDirt();
    }
    
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCleaning();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopCleaning();
        }
    }
    
    private void UpdateToolPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; 
        Vector3 worldPos = playerCamera.ScreenToWorldPoint(mousePos);
        
        transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 8f);
    }
    
    private void CheckForDirt()
    {
        //find nearby dirt spots
        Collider2D[] nearbyDirt = Physics2D.OverlapCircleAll(toolTip.position, detectionRadius, dirtLayerMask);
        
        DirtSpot closestDirt = null;
        float closestDistance = float.MaxValue;
        
        foreach (var collider in nearbyDirt)
        {
            DirtSpot dirt = collider.GetComponent<DirtSpot>();
            if (dirt != null && !dirt.IsFullyCleaned())
            {
                float distance = Vector2.Distance(toolTip.position, dirt.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDirt = dirt;
                }
            }
        }
        
        if (closestDirt != currentDirtSpot)
        {
            if (currentDirtSpot != null && isCleaning)
            {
                currentDirtSpot.StopCleaning();
            }
            
            currentDirtSpot = closestDirt;
            
            if (currentDirtSpot != null && isCleaning)
            {
                currentDirtSpot.StartCleaning(this);
            }
        }
    }
    
    private void StartCleaning()
    {
        isCleaning = true;
        
        if (currentDirtSpot != null)
        {
            currentDirtSpot.StartCleaning(this);
        }
        
        if (toolParticles != null)
        {
            toolParticles.Play();
        }
    }
    
    private void StopCleaning()
    {
        isCleaning = false;
        
        if (currentDirtSpot != null)
        {
            currentDirtSpot.StopCleaning();
        }
        
        if (toolParticles != null)
        {
            toolParticles.Stop();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (toolTip != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(toolTip.position, detectionRadius);
        }
    }
}