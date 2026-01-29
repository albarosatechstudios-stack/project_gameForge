using UnityEngine;
using TMPro; // Serve per il testo

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance; // Singleton per accessibilità facile

    [Header("Riferimenti UI")]
    public GameObject tutorialPanel;   // Il pannello intero
    public TextMeshProUGUI textDisplay; // Dove scriviamo il testo

    private void Awake()
    {
        // Setup del Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Assicuriamoci che sia spento all'inizio
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    // --- FUNZIONI CHIAMATE DAI TRIGGER ---

    public void ShowTutorial(string message)
    {
        // 1. Aggiorna il testo
        textDisplay.text = message;

        // 2. Attiva il pannello
        tutorialPanel.SetActive(true);

        // 3. Blocca il gioco (Pausa)
        Time.timeScale = 0f;

       
         Cursor.lockState = CursorLockMode.None;
         Cursor.visible = true;
    }

    // --- FUNZIONE CHIAMATA DAL BOTTONE "OK" ---

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);

        // Riprendi il gioco
        Time.timeScale = 1f;

        
         Cursor.lockState = CursorLockMode.Locked;
         Cursor.visible = false;
    }
    public void DistruggiManager()
    {
        Debug.Log("[MaestroManager] Reset e autodistruzione in corso...");

        // 1. Rimuoviamo il riferimento statico per evitare errori "MissingReference"
        if (instance == this)
        {
            instance = null;
        }

        // 2. Distruggiamo il GameObject che tiene lo script
        // Nota: Unity chiamerà automaticamente OnDisable(), quindi l'evento si scollegherà da solo.
        Destroy(gameObject);
    }
}