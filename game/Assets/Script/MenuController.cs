using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Creiamo una variabile statica per poter accedere a questo script da ovunque
    public static MenuController instance;

    private void Awake()
    {
        // SINGLETON PATTERN
        // Controlliamo se esiste già un'istanza di questo Game Manager
        if (instance == null)
        {
            // Se non esiste, sono io l'istanza principale
            instance = this;
            // Questo comando impedisce a Unity di distruggere l'oggetto al cambio scena
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Se esiste già un altro GameManager (es. tornando al menu principale),
            // distruggo questo duplicato per averne sempre e solo uno.
            Destroy(gameObject);
        }
    }

    // --- LE TUE FUNZIONI PER I PULSANTI ---

    public void PlayGame()
    {
        SceneManager.LoadScene(1); // Assumendo che 1 sia la scena di gioco
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f; 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}