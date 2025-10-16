using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource backgroundMusic;
    public AudioSource ambience;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGameplay()
    {
        backgroundMusic.Play();
        ambience.Play();
    }

    public void StopMusicAburpt()
    {
        backgroundMusic.Stop();
        ambience.Stop();
    }

    public void FadeOutMusic()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float startVolumeBG = backgroundMusic.volume;
        float startVolumeAMB = ambience.volume;

        while (backgroundMusic.volume > 0 || ambience.volume > 0)
        {
            backgroundMusic.volume -= startVolumeBG * Time.deltaTime / 1f;
            ambience.volume -= startVolumeAMB * Time.deltaTime / 1f;
            yield return null;
        }

        backgroundMusic.Stop();
        backgroundMusic.volume = startVolumeBG;
        ambience.Stop();
        ambience.volume = startVolumeAMB;
    }
}
