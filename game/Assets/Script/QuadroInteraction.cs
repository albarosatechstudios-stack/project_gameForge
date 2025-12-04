using UnityEngine;
using UnityEngine.InputSystem; // Per il tasto Z
using System.IO; // Necessario per leggere i file dal computer

public class LoadDrawingOnQuadro : MonoBehaviour
{
    // --- IMPOSTAZIONI PATH ---
    // Usiamo Path.Combine per costruire il percorso in modo sicuro
    private string folderName = "Disegni";
    private string fileName = "saved_drawing.png";

    // --- VARIABILI INTERNE ---
    private bool isPlayerNear = false;
    private bool hasChanged = false; // Non reversibile
    private Renderer myRenderer;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();

        // Controllo tag
        if (!gameObject.CompareTag("Quadro"))
        {
            Debug.LogWarning($"L'oggetto '{gameObject.name}' non ha il tag 'Quadro'!");
        }
    }

    void Update()
    {
        // Se il player è vicino, non è ancora cambiato e preme Z
        if (isPlayerNear && !hasChanged && Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
        {
            ApplyTextureFromFile();
        }
    }

    void ApplyTextureFromFile()
    {
        // 1. Costruiamo il percorso dinamico
        // Application.persistentDataPath punta automaticamente a:
        // C:/Users/TuoNomeUtente/AppData/LocalLow/DefaultCompany/GameForge25
        string fullPath = Path.Combine(Application.persistentDataPath, folderName, fileName);

        Debug.Log("Tentativo di caricamento da: " + fullPath);

        // 2. Controlliamo se il file esiste
        if (File.Exists(fullPath))
        {
            // 3. Leggiamo i byte del file PNG
            byte[] fileData = File.ReadAllBytes(fullPath);

            // 4. Creiamo una Texture vuota (le dimensioni verranno sovrascritte automaticamente)
            Texture2D newTexture = new Texture2D(2, 2);

            // 5. Carichiamo l'immagine nella texture
            if (newTexture.LoadImage(fileData))
            {
                // 6. Applichiamo la texture al materiale del quadro
                // Nota: Se usi URP/HDRP potrebbe servire myRenderer.material.SetTexture("_BaseMap", newTexture);
                myRenderer.material.mainTexture = newTexture;

                hasChanged = true; // Blocca ulteriori modifiche
                Debug.Log("Immagine applicata con successo!");
            }
            else
            {
                Debug.LogError("Impossibile convertire il file in Texture.");
            }
        }
        else
        {
            Debug.LogError($"File non trovato! Assicurati che il disegno esista qui: {fullPath}");
        }
    }

    // --- RILEVAMENTO PLAYER ---
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}