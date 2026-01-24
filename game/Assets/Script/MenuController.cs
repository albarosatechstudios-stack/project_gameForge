using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    [Header("Gestione UI Menu")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    // --- NUOVO: DATI SETTINGS ---
    // Queste variabili saranno accessibili da TUTTI gli script del gioco
    public float mouseSensitivity  = 1.0f;
    public int microphoneIndex  = 0;

    public event Action<float> OnSensitivityChanged; // NUOVO
    public event Action<int> OnMicrophoneChanged;

    private void Awake()
    {
        // Se esiste già un'altra istanza (quella che arriva dal gioco)...
        if (instance != null)
        {
            // ... DISTRUGGILA! (Distruggiamo la vecchia versione che ha i link UI rotti)
            Destroy(instance.gameObject);
        }

        // Ora IO (la nuova versione nata in questa scena) divento il capo
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Carico i dati (così recupero sensibilità e mic salvati)
        LoadSettings();
    }

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // --- LOGICA GESTIONE DATI ---

    public void UpdateSensitivity(float val)
    {
        mouseSensitivity = val;
        PlayerPrefs.SetFloat("MouseSens", val); // Salva su disco
        PlayerPrefs.Save();
        OnSensitivityChanged?.Invoke(val);
        Debug.Log("MenuController: Sensibilità aggiornata a " + val);
    }

    public void UpdateMicrophoneIndex(int index)
    {
        microphoneIndex = index;
        PlayerPrefs.SetInt("MicIndex", index); // Salva su disco
        PlayerPrefs.Save();
        OnMicrophoneChanged?.Invoke(index);
        Debug.Log("MenuController: Microfono index aggiornato a " + index);
    }

    private void LoadSettings()
    {
        // Carica i dati (oppure usa i default 1.0f e 0)
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSens", 1.0f);
        microphoneIndex = PlayerPrefs.GetInt("MicIndex", 0);
    }

    // --- GESTIONE UI ---

    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // --- GESTIONE SCENE ---

    public void PlayGame() { SceneManager.LoadScene(1); }
    public void QuitGame() { Application.Quit(); }
}