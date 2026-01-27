using UnityEngine;
using UnityEngine.InputSystem; // Fondamentale per il New Input System

public class NPCMaestro : MonoBehaviour
{
    [Header("Riferimenti")]
    public MaestroVisuals visuals;

    [Header("Dialoghi")]
    [TextArea] public string dialogoThief = "AL LADRO! GUARDIE!";
    [TextArea] public string dialogoPrimaVolta = "Benvenuto viaggiatore! Piacere di conoscerti.";
    [TextArea] public string dialogoDopoQuadro = "Quel quadro nasconde un terribile segreto...";
    [TextArea] public string dialogoStandard = "Bella giornata, vero?";

    // Variabile privata per sapere se il giocatore è vicino
    private bool giocatoreInZona = false;

    private void Update()
    {
        // Controlliamo l'input SOLO se il giocatore è nella zona trigger
        if (giocatoreInZona)
        {
            // Esempio: Tasto "E" della tastiera, oppure Tasto Sinistro del Mouse
            if (Keyboard.current.eKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
            {
                Interagisci();
            }
        }
    }

    // --- RILEVAMENTO ZONA (TRIGGER) ---

    private void OnTriggerEnter(Collider other)
    {
        // Controlliamo se chi è entrato è il Player
        // NOTA: Devi assicurarti che il tuo Player abbia il Tag "Player"
        if (other.CompareTag("Player"))
        {
            giocatoreInZona = true;
            Debug.Log("Sei vicino al Maestro. Premi 'E' o Clicca per parlare.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            giocatoreInZona = false;

            // Opzionale: Se te ne vai mentre il dialogo è aperto, lo chiudiamo?
            // DialogueManager.Instance.ChiudiDialogo();
            Debug.Log("Ti sei allontanato.");
        }
    }

    // --- LOGICA INTERAZIONE (IDENTICA A PRIMA) ---

    public void Interagisci()
    {
        MaestroManager.Instance.ConfermaInterazioneAvvenuta();

        // Se siamo in Thief
        if (MaestroManager.Instance.statoMentaleMaestro == GameState.Thief)
        {
            DialogueManager.Instance.MostraMessaggio(dialogoThief);
            return;
        }

        // Se hai visto il quadro
        if (MaestroManager.Instance.segretoQuadroSbloccato)
        {
            DialogueManager.Instance.MostraMessaggio(dialogoDopoQuadro);
            return;
        }

        // Se è la prima volta
        if (!MaestroManager.Instance.primoIncontroAvvenuto)
        {
            DialogueManager.Instance.MostraMessaggio(dialogoPrimaVolta);
            return;
        }

        // Default
        DialogueManager.Instance.MostraMessaggio(dialogoStandard);
    }
}