using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public TMP_Text progressText;
    public Slider overallProgressBar;
    
    private List<DirtSpot> allDirtSpots = new List<DirtSpot>();
    private int cleanedSpots = 0;
    private float totalDirtAmount = 0f;
    private float totalDirtCleaned = 0f;
    
    void Awake()
    {
        Instance = this;
    }
    
    IEnumerator Start()
    {
        DirtSpot[] spots = FindObjectsOfType<DirtSpot>();
        allDirtSpots.AddRange(spots);
        
        totalDirtAmount = 0f;
        foreach (var spot in allDirtSpots)
        {
            totalDirtAmount += spot.maxDirtiness;
        }

        if (overallProgressBar != null)
        {
            overallProgressBar.minValue = 0f;
            overallProgressBar.maxValue = totalDirtAmount;
            overallProgressBar.value = 0f;
        }

        yield return null;

        UpdateUI();
    }
    
    public void OnDirtSpotCleaned(DirtSpot dirtSpot)
    {
        cleanedSpots++;    
        totalDirtCleaned += dirtSpot.maxDirtiness;

        UpdateUI();
        CheckGameComplete();
    }
    
    private void UpdateUI()
    {
        totalDirtCleaned = 0f;

        foreach (var spot in allDirtSpots)
        {
            if (spot != null)
                totalDirtCleaned += spot.GetAmountCleaned();
        }

        if (progressText != null)
        {
            float progressPercent = (totalDirtAmount > 0) ? (totalDirtCleaned / totalDirtAmount) * 100f : 0f;
            progressText.text = $"{progressPercent:0.0}%";
        }
        
        if (overallProgressBar != null)
        {
            overallProgressBar.value = totalDirtCleaned;
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