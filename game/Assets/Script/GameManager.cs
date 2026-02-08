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

    [Header("Settings Utente")]
    public float MouseSensitivity { get; private set; } = 1.0f;
    public int HeadphoneProfileIndex { get; private set; } = 0;

    public static GameManager Instance { get; private set; }

    // --- VISIBILITÀ INSPECTOR ---
    // Questo serve solo per vederlo nell'editor. 
    // La logica usa la proprietà pubblica 'CurrentState'.
    [SerializeField] private GameState _debugCurrentState;

    public GameState CurrentState
    {
        get { return _debugCurrentState; }
        private set { _debugCurrentState = value; }
    }
    // ---------------------------

    public static event Action<GameState> OnStateChanged;
    public static event Action OnSettingsChanged;

    public static string lastScena = "MainMenu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Questo succede SOLO la primissima volta che avvii il gioco.
        Debug.Log($"GameManager Avviato. Stato iniziale: {this.CurrentState}");
        ChangeState(GameState.Visitor);
    }

    private void Update()
    {
        // Debug rapido per cambiare stato con T
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ChangeState(GameState.Thief);
        }
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState; // Aggiorna la variabile visibile nell'Inspector

        Debug.Log($"Stato del gioco cambiato in: {newState}");
        if (newState == GameState.Thief) 
        {
            var cambiaMat = FindObjectOfType<CambiaMateriale>();
            if (cambiaMat != null)
            {
                cambiaMat.ApplicaNuovoMateriale();
            }
        }
            

        OnStateChanged?.Invoke(newState);
    }

    public void SetVisitor()
    {
        ChangeState(GameState.Visitor);
    }

    // --- SALVATAGGIO & CARICAMENTO SETTINGS ---
    public void UpdateSensitivity(float value)
    {
        MouseSensitivity = value;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void UpdateHeadphoneProfile(int index)
    {
        HeadphoneProfileIndex = index;
        SaveSettings();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MouseSens", MouseSensitivity);
        PlayerPrefs.SetInt("AudioProfile", HeadphoneProfileIndex);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSens", 1.0f);
        HeadphoneProfileIndex = PlayerPrefs.GetInt("AudioProfile", 0);
    }

    // --- GESTIONE SCENE ---
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Se torniamo al Menu Principale -> Reset totale (diventa Visitor)
        if (scene.name == "MainMenu")
        {
            ResetGameParameters();
        }

        // 2. Se siamo nella schermata di Game Over -> Sblocca mouse, ma MANTIENI LO STATO
        else if (scene.name == endSceneName)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // Qui NON cambiamo stato. Se eri Thief, resti Thief.
        }

        // 3. Se ricarichiamo il livello (Restart) -> NON facciamo nulla.
        // Il GameManager mantiene lo stato che aveva prima di morire.
    }

    // Funzione chiamata dal tasto "Restart"
    public void LoadLastScena()
    {
        SceneManager.LoadScene(lastScena);
    }
    public void LoadMenuScena()
    {
        if (MaestroManager.Instance != null)
        {
            MaestroManager.Instance.DistruggiManager();
        }
        SceneManager.LoadScene("MainMenu");

    }

    // Funzione chiamata dal tasto "Menu Principale"
    public void ResetGameParameters()
    {
        ChangeState(GameState.Visitor);
        lastScena = "MainMenu";
        Debug.Log("Reset parametri gioco eseguito.");
    }
}