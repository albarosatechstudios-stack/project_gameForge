using UnityEngine;
using System;
using UnityEngine.InputSystem; 
using UnityEngine.SceneManagement;


// Definiamo gli stati del gioco tramite Enum
public enum GameState
{
    Visitor, // Fase 1: Visitatore (guardie passive, niente oggetti)
    Thief    // Fase 2: Ladro (guardie attive, oggetti utilizzabili)
}

public class GameManager : MonoBehaviour
{
     [Header("Impostazioni")]
    [SerializeField] private string endSceneName = "EndGame";

    // Pattern Singleton per accedere al GameManager da qualsiasi script
    public static GameManager Instance { get; private set; }

    // Stato attuale del gioco
    public GameState CurrentState { get; private set; }

    // Evento che viene lanciato quando lo stato cambia
    public static event Action<GameState> OnStateChanged;

    private void Awake()
    {
        // Setup del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Non distruggere al cambio scena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Inizia il gioco come Visitatore
        Debug.Log($"Stato è: {this.CurrentState}");
        ChangeState(GameState.Visitor);

        
    }

    // Metodo pubblico per cambiare lo stato del gioco
    public void ChangeState(GameState newState)
    {
        // Evita di ri-chiamare lo stesso stato se ci siamo già
        if (CurrentState == newState) return;

        CurrentState = newState;
        Debug.Log($"Stato del gioco cambiato in: {newState}");

        // Notifica tutti gli script iscritti all'evento
        OnStateChanged?.Invoke(newState);
    }

        // ESEMPIO: Tasto per testare il cambio di stato durante il gameplay
    private void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            // Simula il passaggio alla fase Ladro premendo 'T'
            ChangeState(GameState.Thief); 
        }
    }

     private void OnEnable()
    {
        // Mi iscrivo all'evento di Unity che avvisa quando una scena è caricata
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Mi disiscrivo per evitare errori
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Questa funzione parte in automatico ogni volta che una scena finisce di caricare
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Controllo se la scena appena caricata è quella finale
        if (scene.name == endSceneName)
        {
            Debug.Log("Siamo nell'ultima scena! Il GameManager ha finito il suo lavoro. Addio.");
            
            // Rimuovo il riferimento statico
            if (Instance == this) Instance = null;

            // Distruggo l'oggetto (così al prossimo riavvio si riparte da zero)
            Destroy(gameObject);
        }
    }

}