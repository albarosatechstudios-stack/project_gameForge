using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // Fondamentale

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Riferimenti UI")]
    public GameObject pannelloDialogo;
    public TextMeshProUGUI testoDialogo;
    public TextMeshProUGUI testoIstruzioni;

    private bool isDialogoAperto = false;
    private bool puoChiudere = false;

    // Memoria per sapere chi eri prima di parlare
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
        if (!isDialogoAperto || !puoChiudere) return;

        bool inputRilevato = false;
        // Rileva tastiera o mouse (funziona anche in TimeScale 0)
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) inputRilevato = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) inputRilevato = true;

        if (inputRilevato)
        {
            ChiudiDialogo();
        }
    }

    public void MostraMessaggio(string messaggio)
    {
        if (GameManager.Instance != null)
        {
            // --- SALVATAGGIO STATO (Safety Check) ---
            // Salviamo lo stato SOLO se non siamo gi� in pausa/dialogo.
            // Questo evita il bug dove salvi "NoPause" come stato precedente e rimani bloccato.
            if (GameManager.Instance.CurrentState != GameState.NoPause)
            {
                statoPrecedente = GameManager.Instance.CurrentState;
            }

            // Imposta lo stato su NoPause (blocca interazioni, menu, etc)
            GameManager.Instance.ChangeState(GameState.NoPause);
        }

        // Blocca il tempo fisico
        Time.timeScale = 0f;

        // Attiva UI
        pannelloDialogo.SetActive(true);
        testoDialogo.text = messaggio;

        isDialogoAperto = true;
        puoChiudere = false;

        StopAllCoroutines();
        StartCoroutine(AbilitaChiusura());
    }

    public void ChiudiDialogo()
    {
        // Spegne UI
        pannelloDialogo.SetActive(false);
        testoDialogo.text = "";
        isDialogoAperto = false;
        puoChiudere = false;

        // Riavvia il tempo
        Time.timeScale = 1f;

        // --- RIPRISTINO STATO ---
        if (GameManager.Instance != null)
        {
            // Torna a essere quello che eri prima (Visitor o Thief)
            GameManager.Instance.ChangeState(statoPrecedente);
        }
    }

    IEnumerator AbilitaChiusura()
    {
        // Usa Realtime per ignorare il Time.timeScale = 0
        yield return new WaitForSecondsRealtime(0.2f);
        puoChiudere = true;
    }
}