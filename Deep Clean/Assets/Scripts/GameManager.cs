using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public TMP_Text progressText;
    public Slider overallProgressBar;
    
    private List<DirtSpot> allDirtSpots = new List<DirtSpot>();
    private int cleanedSpots = 0;
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        DirtSpot[] spots = FindObjectsOfType<DirtSpot>();
        allDirtSpots.AddRange(spots);
        
        UpdateUI();
    }
    
    public void OnDirtSpotCleaned(DirtSpot dirtSpot)
    {
        cleanedSpots++;        
        UpdateUI();
        CheckGameComplete();
    }
    
    private void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"Progress: {cleanedSpots}/{allDirtSpots.Count}";
        
        if (overallProgressBar != null && allDirtSpots.Count > 0)
        {
            overallProgressBar.value = (float)cleanedSpots / allDirtSpots.Count;
        }
    }
    
    private void CheckGameComplete()
    {
        if (cleanedSpots >= allDirtSpots.Count)
        {
            OnGameComplete();
        }
    }
    
    private void OnGameComplete()
    {
        Debug.Log("All dirt cleaned! Game complete!");
    }
}