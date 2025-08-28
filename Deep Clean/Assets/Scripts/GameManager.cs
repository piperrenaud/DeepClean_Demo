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
    public Slider roomProgressBar;
    public TMP_Text roomProgressText;

    private List<DirtSpot> allDirtSpots = new List<DirtSpot>();
    private int cleanedSpots = 0;
    private float totalDirtAmount = 0f;
    private float totalDirtCleaned = 0f;

    private PlayerRoomTracker playerRoomTracker;
    
    void Awake()
    {
        Instance = this;
    }
    
    IEnumerator Start()
    {
        DirtSpot[] spots = FindObjectsOfType<DirtSpot>();
        allDirtSpots.AddRange(spots);

        playerRoomTracker = FindObjectOfType<PlayerRoomTracker>();
        
        totalDirtAmount = allDirtSpots.Sum(s => s.maxDirtiness);

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
    
    public void UpdateUI()
    {
        //overall
        totalDirtCleaned = allDirtSpots.Sum(s => s.GetAmountCleaned());

        if (progressText != null)
        {
            float progressPercent = (totalDirtAmount > 0) ? (totalDirtCleaned / totalDirtAmount) * 100f : 0f;
            progressText.text = $"{progressPercent:0.0}%";
        }
        
        if (overallProgressBar != null)
        {
            overallProgressBar.value = totalDirtCleaned;
        }

        //per room
        if (playerRoomTracker != null)
        {
            int currentRoom = playerRoomTracker.currentRoomID;
            var roomSpots = allDirtSpots.Where(s => s.roomID == currentRoom).ToList();

            float roomTotal = roomSpots.Sum(s => s.GetMaxDirt());
            float roomCleaned = roomSpots.Sum(s => s.GetAmountCleaned());

            if (roomProgressBar != null)
            {
                roomProgressBar.minValue = 0f;
                roomProgressBar.maxValue = roomTotal;
                roomProgressBar.value = roomCleaned;
            }

            if (roomProgressText != null)
            {
                float roomPercent = (roomTotal > 0) ? (roomCleaned / roomTotal) * 100f : 0f;
                roomProgressText.text = $"{roomPercent:0.0}%";
            }
        }
    }

    public float GetRoomCleanliness(int roomID)
    {
        var roomSpots = allDirtSpots.Where(s => s.roomID == roomID).ToList();
        float roomTotal = roomSpots.Sum(s => s.GetMaxDirt());
        float roomCleaned = roomSpots.Sum(s => s.GetAmountCleaned());

        return (roomTotal > 0f) ? (roomCleaned / roomTotal) * 100f : 100f;
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