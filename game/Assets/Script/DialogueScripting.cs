using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; 

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Riferimenti UI")]
    public GameObject pannelloDialogo;
    public TextMeshProUGUI testoDialogo;
    public TextMeshProUGUI testoIstruzioni;

    // MODIFICA 1: "isDialogoAperto" ora ha un Getter pubblico
    private bool _isDialogoAperto = false;
    public bool IsDialogoAperto { get { return _isDialogoAperto; } }

    private bool puoChiudere = false;
    private GameState statoPrecedente = GameState.Visitor;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (pannelloDialogo != null) pannelloDialogo.SetActive(false);
        if (testoIstruzioni != null) testoIstruzioni.text = "Premi un tasto per continuare";
    }

    void Update()
    {
        // Se il dialogo Ã¨ chiuso o non possiamo ancora chiuderlo, esci
        if (!_isDialogoAperto || !puoChiudere) return;

        bool inputRilevato = false;
        
        // Rileva input (Input System funziona anche con TimeScale=0 se configurato su "Update Mode: Dynamic/Unscaled")
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) inputRilevato = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) inputRilevato = true;

        if (inputRilevato)
        {
            ChiudiDialogo();
        }
    }

    public void MostraMessaggio(string messaggio)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.NoPause)
        {
            statoPrecedente = GameManager.Instance.CurrentState;
            GameManager.Instance.ChangeState(GameState.NoPause);
        }

        Time.timeScale = 0f;

        pannelloDialogo.SetActive(true);
        testoDialogo.text = messaggio;

        _isDialogoAperto = true; // Impostiamo la variabile
        puoChiudere = false;

        StopAllCoroutines();
        StartCoroutine(AbilitaChiusura());
    }

    public void ChiudiDialogo()
    {
        pannelloDialogo.SetActive(false);
        testoDialogo.text = "";
        
        // --- PUNTO CRITICO ---
        // Impostiamo isDialogoAperto a false.
        // NOTA: Il tempo riparte subito, quindi nello stesso frame il Maestro potrebbe leggere il click.
        _isDialogoAperto = false; 
        puoChiudere = false;

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(statoPrecedente);
        }
    }

    IEnumerator AbilitaChiusura()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        puoChiudere = true;
    }
}