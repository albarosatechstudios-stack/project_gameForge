using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    Visitor,
    Thief,
    NoPause
}

public class GameManager : MonoBehaviour
{
    [Header("Impostazioni Generali")]
    [SerializeField] private string endSceneName = "EndGame";

    // --- NUOVO: Variabili Settings ---
    [Header("Settings Utente")]
    public float MouseSensitivity { get; private set; } = 1.0f; // Default
    public int HeadphoneProfileIndex { get; private set; } = 0; // Default

    // Pattern Singleton
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    public static event Action<GameState> OnStateChanged;

    // --- NUOVO: Evento per notificare il cambio impostazioni (utile per il PlayerController) ---
    public static event Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- NUOVO: Carica i dati appena il gioco si avvia ---
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log($"Stato è: {this.CurrentState}");
        ChangeState(GameState.Visitor);
    }

    // ... (Il tuo codice ChangeState e Update rimane uguale) ...
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        Debug.Log($"Stato del gioco cambiato in: {newState}");
        OnStateChanged?.Invoke(newState);
    }

    public void SetVisitor()
    {
        CurrentState=GameState.Visitor;
    }

    private void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ChangeState(GameState.Thief);
        }
    }

    // --- NUOVO: Gestione Dati Settings ---

    public void UpdateSensitivity(float value)
    {
        MouseSensitivity = value;
        SaveSettings(); // Salvataggio automatico ad ogni modifica
        OnSettingsChanged?.Invoke(); // Avvisa chi è in ascolto (es. PlayerController)
    }

    public void UpdateHeadphoneProfile(int index)
    {
        HeadphoneProfileIndex = index;
        SaveSettings();
        // Qui potresti chiamare un AudioManager per cambiare mix
        Debug.Log("Profilo cuffie cambiato a indice: " + index);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MouseSens", MouseSensitivity);
        PlayerPrefs.SetInt("AudioProfile", HeadphoneProfileIndex);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSens", 1.0f); // 1.0f è il default se non trova nulla
        HeadphoneProfileIndex = PlayerPrefs.GetInt("AudioProfile", 0);
    }

    // ... (Il resto del tuo codice OnEnable/OnDisable/OnSceneLoaded rimane uguale) ...
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == endSceneName)
        {
            if (Instance == this) Instance = null;
            Destroy(gameObject);
        }
    }
}