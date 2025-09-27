using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameUI;
    public GameObject pauseMenuPanel;
    public Button resumeButtons;
    public Button quitButton;

    public Button ragIcon;
    public Button rubbishBagIcon;
    public Button cameraIcon;

    public TMP_Text descriptionText;

    [Header("Hover Descriptions")]
    public string ragDesc = "A rag used for cleaning surfaces.";
    public string vaccumDesc = "Vaccum to clean dust and debris.";
    public string rubbishBagDesc = "Bag for disposing trash.";
    public string cameraDesc = "Camera to take evidence photos.";

    private bool isPaused = false;
    
    
    void Start()
    {
        //default menu hidden
        pauseMenuPanel.SetActive(false);

        //assign button listeners
        resumeButtons.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);

        //assign hover events for icons
        AddHoverEvents(ragIcon, ragDesc);
        AddHoverEvents(rubbishBagIcon, rubbishBagDesc);
        AddHoverEvents(cameraIcon, cameraDesc);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) //CHANGE TO Escape
        {
            Debug.Log("hello");
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        gameUI.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        gameUI.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void QuitGame()
    {
        Debug.Log("Quit Game");
        //Application.Quit();
    }

    private void AddHoverEvents(Button iconButton, string description)
    {
        EventTriggerListener listener = iconButton.gameObject.AddComponent<EventTriggerListener>();
        listener.onEnter += () => { descriptionText.text = description; };
        listener.onExit += () => { descriptionText.text = ""; };
    }
}
