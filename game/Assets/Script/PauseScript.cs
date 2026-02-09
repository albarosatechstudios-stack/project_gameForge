using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public  GameObject minimap; 

    [Header("UI Panels")]
    public GameObject pauseMenuUI;    // Il pannello con i bottoni Resume, Options, Quit
    public GameObject settingsPanel;  // Il pannello che contiene slider e dropdown

    void Update()
    {
        // Controllo generico (opzionale)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.NoPause) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // LOGICA INTELLIGENTE DEL TASTO ESC:

            // 1. Se le impostazioni sono aperte, ESC le chiude e torna al menu pausa
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            // 2. Se il gioco è in pausa (ma settings chiuso), ESC riprende il gioco
            else if (GameIsPaused)
            {
                Resume();
            }
            // 3. Altrimenti, metti in pausa
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false); // Chiudiamo tutto per sicurezza

        if (minimap != null) {
            minimap.SetActive(true);
        }
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        // Assicuriamoci che quando apri la pausa, le settings siano chiuse
        settingsPanel.SetActive(false);
        if (minimap != null) {
            minimap.SetActive(false);
        }

        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- FUNZIONI PER I BOTTONI UI ---

    // Collegalo al bottone "Options" nel pannello PauseMenuUI
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false); // Nascondi i bottoni principali
        settingsPanel.SetActive(true); // Mostra le opzioni
    }

    // Collegalo al bottone "Back" nel pannello SettingsPanel
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true); // Riporta i bottoni principali
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void onDestroy()
    {
        GameIsPaused = false;
        Destroy(gameObject);
    }
}