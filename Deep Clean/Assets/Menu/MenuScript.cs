using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("HoarderHouse");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
