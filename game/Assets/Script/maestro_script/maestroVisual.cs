using UnityEngine;
using UnityEngine.UI;

public class MaestroVisuals : MonoBehaviour
{
    [Header("Riferimenti")]
    public Canvas worldCanvas;
    public Image iconImage;

    [Header("Icone")]
    public Sprite visitorSprite; // "..." (Default/Primo incontro)
    public Sprite thiefSprite;   // "!" (Sei un ladro)
    public Sprite questSprite;   // "?" (Hai completato un obiettivo e devi parlare con lui)

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (MaestroManager.Instance == null) return;

        // 1. Controllo se devo mostrare l'icona (Leggo dal Manager)
        bool deveMostrare = MaestroManager.Instance.iconaDaMostrare;

        if (deveMostrare)
        {
            AggiornaGrafica();
        }
        else
        {
            // Se non devo mostrarla, spengo l'immagine
            if (iconImage.enabled) iconImage.enabled = false;
        }
    }

    void LateUpdate()
    {
        // Billboard effect: l'icona guarda sempre la telecamera
        if (worldCanvas != null && mainCamera != null)
        {
            worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - mainCamera.transform.position);
        }
    }

    void AggiornaGrafica()
    {
        GameState statoMentale = MaestroManager.Instance.statoMentaleMaestro;
        QuestMaestro faseQuest = MaestroManager.Instance.faseAttuale;
        Sprite spriteFinale = null;

        // --- PRIORITÀ GRAFICA ---

        // 1. Se il Maestro ti percepisce come LADRO (Massima priorità)
        if (statoMentale == GameState.Thief)
        {
            spriteFinale = thiefSprite;
        }
        // 2. Se hai completato uno step della quest e devi "consegnare" l'info (Punto Interrogativo)
        // Mostriamo il "?" quando:
        // - Hai visto il quadro e devi tornare da lui (QuadroVisto)
        // - Hai finito il falso e devi farglielo vedere (FalsoPronto)
        else if (faseQuest == QuestMaestro.QuadroVisto || faseQuest == QuestMaestro.FalsoPronto)
        {
            spriteFinale = questSprite;
        }
        // 3. Default: Nuvoletta "..."
        // Si usa per il primo incontro o per i dialoghi intermedi (Inizio, DeveVedereQuadro, CreazioneFalso)
        else
        {
            spriteFinale = visitorSprite;
        }

        // Applico lo sprite solo se è cambiato o se l'icona era spenta
        if (iconImage.sprite != spriteFinale || iconImage.enabled == false)
        {
            iconImage.sprite = spriteFinale;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }
    }
}