using UnityEngine;
using UnityEngine.SceneManagement; // <--- FONDAMENTALE!

public class CambioScena : MonoBehaviour
{
    [Header("Nome della scena dove andare")]
    public string nomeScenaDaCaricare; // Scrivi qui il nome esatto (es. "Livello2")

    // Metodo 1: Se entri in un Trigger (es. una porta)
    void OnTriggerEnter(Collider other)
    {
        if (this.enabled == false) return;
        if (other.CompareTag("Player") )
        {
            Debug.Log("Cambio scena in corso...");
            SceneManager.LoadScene(nomeScenaDaCaricare);
        }
    }

    // Metodo 2: Se vuoi chiamarlo da un Bottone della UI
    public void CaricaScena()
    {
        SceneManager.LoadScene(nomeScenaDaCaricare);
    }
}