using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioScena : MonoBehaviour
{
    [Header("Nome della scena standard")]
    public string nomeScenaDaCaricare; // Es. "Bottega" o "Livello2"

    [Header("Nome scena finale")]
    public string nomeScenaFinale = "EndGame"; // Modificabile da inspector se vuoi

    // Metodo 1: Trigger fisico
    void OnTriggerEnter(Collider other)
    {
        if (this.enabled == false) return;

        if (other.CompareTag("Player"))
        {
            EseguiCaricamento();
        }
    }

    // Metodo 2: Bottone UI
    public void CaricaScena()
    {
        EseguiCaricamento();
    }

    // Logica centralizzata per evitare errori
    private void EseguiCaricamento()
    {
        // 1. Controllo di sicurezza: Il Manager esiste?
        if (MaestroManager.Instance != null)
        {
            Debug.Log($"[CambioScena] Stato attuale Maestro: {MaestroManager.Instance.faseAttuale}");
            GameManager.lastScena = SceneManager.GetActiveScene().name;
            // 2. Controllo Stato: È finito il gioco?
            if (MaestroManager.Instance.faseAttuale == QuestMaestro.FineGioco)
            {
                Debug.Log("Caricamento Finale...");
                SceneManager.LoadScene(nomeScenaFinale); // Carica "EndGame"
                return; // FERMATI QUI! Non eseguire il codice sotto.
            }
        }

        // 3. Se il Manager non c'è O se il gioco non è finito, carica la scena normale
        Debug.Log("Caricamento Standard...");
        SceneManager.LoadScene(nomeScenaDaCaricare);
    }
}