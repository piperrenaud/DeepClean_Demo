using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CorkBoardPhotoLoader : MonoBehaviour
{
    [Header("Photo Setup")]
    [SerializeField] private GameObject caseBoard;
    [SerializeField] private GameObject photoImagePregab;
    [SerializeField] private float spacing = 2.5f;
    [SerializeField] private int photosPerRow = 4;

    private RectTransform boardCanvas;

    void Start()
    {
        boardCanvas = caseBoard.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        LoadPhotos();
    }

    void LoadPhotos()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "photo_*.png");

        int index = 0;
        foreach (string file in files)
        {
            byte[] bytes = File.ReadAllBytes(file);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            //convert to sprite
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f,0.5f));

            //spawn image under canvas
            GameObject photoObj = Instantiate(photoImagePregab, boardCanvas);
            Image img = photoObj.GetComponent<Image>();
            img.sprite = sprite;

            //set size
            RectTransform rt = photoObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2, 2);

            //position in grid
            int row = index / photosPerRow;
            int col = index % photosPerRow;

            rt.anchoredPosition = new Vector2(col * spacing, -row * spacing);

            index++;
        }
    }
}
