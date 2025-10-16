using UnityEngine; 
using UnityEngine.UI; 
using TMPro; 
using System.Collections.Generic; 
using System.Collections; 
using System.Linq; 

public class GameManager : MonoBehaviour 
{ 
    public static GameManager Instance { get; private set; } 
    
    [Header("Game Settings")] 
    public TMP_Text progressText; 
    public Slider overallProgressBar;
    public Animator notificationAnimator;
    public TMP_Text notificationText; 
    
    [Header("References")] 
    public Transform enemy; 

    private List<DirtSpot> allDirtSpots = new List<DirtSpot>(); 
    private int cleanedSpots = 0; 
    private float totalDirtAmount = 0f; 
    private float totalDirtCleaned = 0f; 
    private float maxRubbishAmount = 0f;
    private float totalRubbishCollected = 0f;
    private PlayerRoomTracker playerRoomTracker; 
    
    void Awake() 
    { 
        Instance = this;
    } 
    
    void Start() 
    { 
        DirtSpot[] spots = FindObjectsByType<DirtSpot>(FindObjectsSortMode.None); 
        allDirtSpots.AddRange(spots); 

        int rubbishLayer = LayerMask.NameToLayer("Rubbish");
        GameObject[] allRubbishObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        maxRubbishAmount = allRubbishObjects.Count(obj => obj.layer == rubbishLayer);
                
        totalDirtAmount = allDirtSpots.Count(); 
        if (overallProgressBar != null) 
        { 
            overallProgressBar.minValue = 0f; 
            overallProgressBar.maxValue = totalDirtAmount + maxRubbishAmount; 
            overallProgressBar.value = 0f; 
        } 
        
        UpdateUI(); 
    } 
    
    public void OnDirtSpotCleaned(DirtSpot dirtSpot) 
    { 
        cleanedSpots++; 
        totalDirtCleaned++;
        
        UpdateUI(); 
    } 
    
    public void UpdateUI() 
    { 
        totalDirtCleaned = allDirtSpots.Sum(s => s.GetAmountCleaned()); 

        float totalProgress = totalDirtCleaned + totalRubbishCollected;

        float progressPercent = (totalDirtAmount + maxRubbishAmount > 0) ? (totalProgress / (totalDirtAmount + maxRubbishAmount)) * 100f : 0f; 
        progressText.text = $"{progressPercent:0}%"; 
        
        if (overallProgressBar != null) 
        { 
            overallProgressBar.maxValue = totalDirtAmount + maxRubbishAmount; 
            overallProgressBar.value = totalProgress;
        } 
    } 
    
    public float GetCurrentCleanliness() 
    {   
        return 100 * ((totalDirtCleaned + totalRubbishCollected) / (totalDirtAmount + maxRubbishAmount)); 
    } 
    
    private void CheckGameComplete() 
    { 
        if (cleanedSpots >= (totalDirtAmount + maxRubbishAmount)) 
        { 
            OnGameComplete(); 
        } 
    } 
    
    private void OnGameComplete() 
    { 
        Debug.Log("All dirt cleaned! Game complete!"); 
    } 
    
    public bool IsCleaningComplete() 
    { 
        return (totalDirtCleaned + totalRubbishCollected) == (totalDirtAmount + maxRubbishAmount); 
    } 

    public void Notify(string text)
    {
        notificationText.text = text;
        notificationAnimator.SetTrigger("Notify");
    }

    public void RubbishAdded()
    {
        totalRubbishCollected++;;
        Debug.Log("total rubbish collected: " + totalRubbishCollected);
        UpdateUI();
    }
}