using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Clips")]
    public AudioClip cleaningSound;
    public AudioClip cleanCompleteSound;
    public AudioClip toolPickupSound;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void PlayCleaningSound()
    {
        if (cleaningSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = cleaningSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    public void StopCleaningSound()
    {
        if (audioSource.clip == cleaningSound)
        {
            audioSource.Stop();
        }
    }
    
    public void PlayCleanCompleteSound()
    {
        if (cleanCompleteSound != null)
        {
            audioSource.PlayOneShot(cleanCompleteSound);
        }
    }
}