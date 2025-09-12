using UnityEngine;
using UnityEngine.UI;

public class DirtSpot : MonoBehaviour
{
    [Header("Room Assignment")]
    public int roomID;

    [Header("Dirt Settings")]
    public float maxDirtiness = 100f;
    public float cleaningRate = 20f; 
    public float requiredToolDistance = 3f; 
    
    [Header("Visual Components")]
    public SpriteRenderer dirtRenderer;
    public Slider progressBar; 

    private float currentDirtiness;
    private bool isBeingCleaned = false;
    private CleaningTool currentTool;
    private Transform toolWorldTransform;
    private bool isSprayed = false;
    
    void Start()
    {
        currentDirtiness = maxDirtiness;
        UpdateVisuals();
    }
    
    void Update()
    {
        if (isBeingCleaned && currentTool != null)
        {
            CleanDirt();
        }
    }
    
    public void StartCleaning(CleaningTool tool, Transform toolTransform)
    {
        if (currentDirtiness <= 0) return;
        
        currentTool = tool;
        toolWorldTransform = toolTransform; // store transform for distance checks
        isBeingCleaned = true;
        

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
        }

        AudioManager.Instance?.PlayCleaningSound();
    }
    
    public void StopCleaning()
    {
        isBeingCleaned = false;
        currentTool = null;
        
        AudioManager.Instance?.StopCleaningSound();
    }
    
    private void CleanDirt()
    {
        if (currentTool == null) return;
        
        float distance = Vector3.Distance(transform.position, toolWorldTransform.position);
        if (distance > requiredToolDistance)
        {
            StopCleaning();
            return;
        }
        
        //cleaning based on effectiveness and time
        float cleaningAmount = cleaningRate * currentTool.effectiveness * Time.deltaTime;
        currentDirtiness = Mathf.Max(0, currentDirtiness - cleaningAmount);
        
        UpdateVisuals();
        
        if (currentDirtiness <= 0)
        {
            OnDirtCleaned();
        }
    }
    
    private void UpdateVisuals()
    {
        if (dirtRenderer != null)
        {
            //fade out dirt as it gets cleaner
            float alpha = currentDirtiness / maxDirtiness;
            Color color = dirtRenderer.color;
            color.a = alpha;
            dirtRenderer.color = color;
        }
        
        if (progressBar != null)
        {
            progressBar.value = 1f - (currentDirtiness / maxDirtiness);
        }
    }

    public void Spray()
    {
        if (currentDirtiness <= 0) return;
        isSprayed = true;
    }

    public bool CanBeWiped()
    {
        return isSprayed && currentDirtiness > 0;
    }
    
    private void OnDirtCleaned()
    {
        StopCleaning();
        
        AudioManager.Instance?.PlayCleanCompleteSound();
        
        //notify game manager
        GameManager.Instance?.OnDirtSpotCleaned(this);
        
        //disable or destroy the dirt spot
        gameObject.SetActive(false);
    }
    
    public float GetCleaningProgress() { return 1f - (currentDirtiness / maxDirtiness); }
    public bool IsFullyCleaned() { return currentDirtiness <= 0; }
    public float GetAmountCleaned() { return maxDirtiness - currentDirtiness; }
    public float GetRemainingDirt() { return currentDirtiness; }
    public float GetMaxDirt() { return maxDirtiness; }
}