using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Cosa scrivere?")]
    [TextArea(3, 10)] // Crea una casella di testo grande nell'Inspector
    public string instructionText = "Scrivi qui le istruzioni...";

    [Header("Opzioni")]
    public bool destroyAfterView = true; // Se true, sparisce dopo l'uso (consigliato)

    private void OnTriggerEnter(Collider other)
    {
        // Controlla che sia il Player a entrare (assicurati che il Player abbia il Tag "Player")
        if (other.CompareTag("Player"))
        {
            // Chiama il manager e passagli il testo
            if (TutorialManager.instance != null)
            {
                TutorialManager.instance.ShowTutorial(instructionText);
            }

            // Distrugge o disattiva QUESTO trigger per non attivarlo 
            // di nuovo appena il giocatore chiude il pannello
            if (destroyAfterView)
            {
                // Disattiviamo l'oggetto o lo distruggiamo
                gameObject.SetActive(false);
                // oppure Destroy(gameObject);
            }
        }
    }
}