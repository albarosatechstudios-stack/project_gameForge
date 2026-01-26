using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class LoadDrawingOnQuadro : MonoBehaviour
{
    private string folderName = "Disegni";
    private string fileName = "saved_drawing.png";

    private bool isPlayerNear = false;
    private bool hasChanged = false;
    private Renderer myRenderer;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        // Forza il tag se ti sei dimenticato di settarlo, utile per debug
        if (!gameObject.CompareTag("Quadro")) Debug.LogWarning("Attenzione: Tag 'Quadro' mancante su " + gameObject.name);
    }

    void Update()
    {
        // Debug temporaneo: premi Z anche se non sei vicino per testare se carica l'immagine
        // if (Keyboard.current.zKey.wasPressedThisFrame) ApplyTextureFromFile(); 

        if (isPlayerNear && !hasChanged && Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame && GameManager.Instance.CurrentState == GameState.Thief)
        {
            ApplyTextureFromFile();
        }
    }

    void ApplyTextureFromFile()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, folderName, fileName);
        Debug.Log("Cerco file in: " + fullPath);

        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D newTexture = new Texture2D(2, 2);

            if (newTexture.LoadImage(fileData))
            {
                // Assegnazione standard
                myRenderer.material.mainTexture = newTexture;
                
                // Se usi URP/HDRP e l'immagine non si vede, togli il commento qui sotto:
                // myRenderer.material.SetTexture("_BaseMap", newTexture);

                hasChanged = true;
                Debug.Log("Immagine applicata!");
            }
        }
        else
        {
            Debug.LogError("File non trovato! Hai salvato il disegno prima?");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Assicurati che il Player abbia il tag "Player"
        if (other.CompareTag("Player")) 
        {
            isPlayerNear = true;
            Debug.Log("Il Player è vicino al quadro! Premi Z.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNear = false;
            Debug.Log("Il Player si è allontanato.");
        }
    }
}