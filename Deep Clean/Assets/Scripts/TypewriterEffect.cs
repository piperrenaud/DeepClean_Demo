using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic; 

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text textMeshPro; 
    public float typingSpeed = 0.03f;

    private string fullText;

    public void Start()
    {
        fullText = textMeshPro.text; 
        textMeshPro.text = string.Empty; 
        StartCoroutine(TypeText()); 
    }

    IEnumerator TypeText()
    {
        foreach (char letter in fullText)
        {
            textMeshPro.text += letter; 
        
            yield return new WaitForSeconds(typingSpeed); 
        }
    }
}