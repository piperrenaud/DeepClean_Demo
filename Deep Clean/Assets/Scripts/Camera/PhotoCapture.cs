using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;

[System.Serializable]
public class PhotoData
{
    public string fileName;
    public bool isEvidence;
}

public class PhotoCapture : MonoBehaviour
{
    [Header("Photo Taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] public GameObject photoFrame;
    [SerializeField] private GameObject cameraUI;

    [Header("Camera UI Elements")]
    [SerializeField] private Image innerFrame;
    [SerializeField] private Color defaultFrameColor = Color.white;
    [SerializeField] private Color validFrameColor = Color.green;
    [SerializeField] private TMP_Text photosLeftText;

    [Header("Flash Effect")]
    [SerializeField] private GameObject cameraFlash;
    [SerializeField] private float flashTime;

    [Header("Photo Fade Effect")]
    [SerializeField] private Animator fadingAnimator;

    [Header("Audio")]
    [SerializeField] private AudioSource cameraAudio;

    [Header("References")]
    public EnemyWander enemyWander;

    private Texture2D screenCapture;
    public bool viewingPhoto;

    private static int photoCounter = 0;

    void Start()
    {
        photoCounter = 0;
        //delete old photos when play mode starts
        string[] files = Directory.GetFiles(Application.persistentDataPath, "photo_*.png");
        foreach (string file in files)
        {
            File.Delete(file);
        }
        Debug.Log("Cleared old photos.");

        screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        photoFrame.SetActive(false);

        if (innerFrame != null)
        {
            innerFrame.color = defaultFrameColor;
        }
    }

    void Update()
    {
        if (!viewingPhoto)
        {
            CheckForInteractable();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!viewingPhoto)
            {
                StartCoroutine(CapturePhoto());
            }
            else 
            {
                RemovePhoto();
            }
        }
    }

    void CheckForInteractable()
    {
        Camera mainCam = Camera.main;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.GetComponent<Interactable>() != null)
            {
                innerFrame.color = validFrameColor;
                return;
            }
        }

        innerFrame.color = defaultFrameColor;
    }

    IEnumerator CapturePhoto()
    {
        if (InventoryManager.Instance.IsFull(true))
        {
            GameManager.Instance.Notify("Photo Inventory Full!");
            yield break;
        }

        cameraUI.SetActive(false);
        viewingPhoto = true;

        yield return new WaitForEndOfFrame();

        Rect regionToRead = new Rect(0, 0, Screen.width, Screen.height);

        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply();

        //check if object in center is evidence
        bool isEvidence = false;
        Camera mainCam = Camera.main;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.CompareTag("Evidence"))
            {
                isEvidence = true;
                Debug.Log("Captured photo of Evidence!");
            }
        }

        int photosLeft = 5 - photoCounter;

        photosLeftText.text = photosLeft.ToString();

        SavePhoto(screenCapture, isEvidence);
        enemyWander.HandlePhotoTaken();
        ShowPhoto();
    }

    void ShowPhoto()
    {
        Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
        //photo area = photo sprite
        photoDisplayArea.sprite = photoSprite;

        photoFrame.SetActive(true);
        StartCoroutine(CameraFlashEffect());
        fadingAnimator.Play("PhotoFade");
    }

    IEnumerator CameraFlashEffect()
    {
        cameraAudio.Play();
        cameraFlash.SetActive(true);
        yield return new WaitForSeconds(flashTime);
        cameraFlash.SetActive(false);
        yield return new WaitForSeconds(2f);
        RemovePhoto();
    }

    public void RemovePhoto()
    {
        viewingPhoto = false;
        photoFrame.SetActive(false);
        cameraUI.SetActive(true);
    }

    void SavePhoto(Texture2D texture, bool isEvidence)
    {
        byte[] bytes = texture.EncodeToPNG();
        string fileName = $"photo_{photoCounter}.png";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, bytes);

        //save metadata
        PhotoData data = new PhotoData
        {
            fileName = fileName,
            isEvidence = isEvidence
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName + ".json"), json);

        //add to inventory
        Sprite photoSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        InventoryManager.Instance.AddItem(fileName, EvidenceType.Photo, isEvidence ? "Evidence photo" : "Photo", "", "", true, photoSprite);

        photoCounter++;
    }
}
