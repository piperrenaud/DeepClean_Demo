using UnityEngine;

public class TVController : MonoBehaviour
{
    public GameObject videoPlayer;

    void Start()
    {
        videoPlayer.SetActive(false);
    }

    public void TurnOn()
    {
        videoPlayer.SetActive(true);
    }

    public void TurnOff()
    {
        videoPlayer.SetActive(false);
    }
}
