using UnityEngine;
using UnityEngine.InputSystem; 

public class NPCMaestro : MonoBehaviour
{
    [Header("Riferimenti")]
    public MaestroVisuals visuals;

    [Header("Obiettivo Attuale")]
    public string objectiveName = "Ragazza con Perle"; 

    [Header("Valutazione Disegno")]
    public SimpleLineComparerIgnoreBG comparatore; 
    public Texture2D immagineReference;            
    [Range(0, 100)] public float sogliaVittoria = 70f; 
    
    private bool giocatoreInZona = false;

    [Header("Oggetto Risultato")]
    public Renderer oggettoDaColorare;

    // MODIFICA 2: Variabile per evitare il doppio click immediato
    private float inputCooldown = 0f;

    void Start()
    {
        MaestroManager.Instance.setNameObjective(objectiveName);
    }

    private void Update()
    {
        // 1. Se il giocatore non è in zona, non fare nulla
        if (!giocatoreInZona) return;

        // 2. CONTROLLO DIALOGO APERTO (La richiesta principale)
        // Se il dialogo è aperto, resettiamo un piccolo timer di "protezione"
        // e usciamo dalla funzione. Il Maestro è "disattivato".
        if (DialogueManager.Instance.IsDialogoAperto)
        {
            inputCooldown = 0.2f; // Mantiene il buffer "carico" finché il dialogo è aperto
            return;
        }

        // 3. GESTIONE COOLDOWN
        // Se il dialogo è stato chiuso, questo timer decresce.
        // Finché è maggiore di 0, ignoriamo qualsiasi click.
        // Questo "mangia" il click che ha chiuso il dialogo.
        if (inputCooldown > 0)
        {
            inputCooldown -= Time.deltaTime;
            return; // Esci, non leggere input
        }

        // 4. Input Normale (Ora siamo sicuri che il dialogo è chiuso da almeno 0.2 secondi)
        if (Keyboard.current.eKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
        {
            Interagisci();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            giocatoreInZona = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            giocatoreInZona = false;
        }
    }

    public void Interagisci()
    {
        MaestroManager manager = MaestroManager.Instance;
        manager.ConfermaInterazioneAvvenuta(); 
        manager.disableComic();

        switch (manager.faseAttuale)
        {
            case QuestMaestro.Inizio:
                DialogueManager.Instance.MostraMessaggio("Maestro:Benvenuto! Vai a guardare il Quadro " + objectiveName + " nel museo.");
                manager.AvanzaFase(); 
                break;

            case QuestMaestro.DeveVedereQuadro:
                DialogueManager.Instance.MostraMessaggio("Maestro:Ancora qui? Guarda il mio capolavoro lì sulla parete allora");
                break;

            case QuestMaestro.QuadroVisto:
                DialogueManager.Instance.MostraMessaggio("Maestro:Bello, vero? Ora crea un falso e sostituiscilo all'originale.");
                manager.AvanzaFase(); 
                break;

            case QuestMaestro.CreazioneFalso:
                if (MaestroManager.Instance.isRealised)
                {
                    DialogueManager.Instance.MostraMessaggio("Maestro:Vai e sostituisci i quadri, non farti <b>ARRESTARE</b>!\nRicordati di tornare qui dopo lo scambio.");
                }
                else
                {
                    DialogueManager.Instance.MostraMessaggio("Maestro:quanto ci vuole a realizzare questo falso? non cincischiare!");
                }
                break;

            case QuestMaestro.FalsoPronto:
                if (comparatore == null || immagineReference == null)
                {
                    Debug.LogError("ERRORE: Manca comparatore o reference!");
                    return;
                }

                float percentuale = comparatore.CompareWithSavedDrawing(immagineReference);
                Debug.Log($"Score: {percentuale}%");

                if (oggettoDaColorare != null && comparatore.textureRisultato != null)
                {
                    oggettoDaColorare.material.mainTexture = comparatore.textureRisultato;
                }
                
                bool isAltaQualita = percentuale >= sogliaVittoria;

                if (isAltaQualita)
                {
                    DialogueManager.Instance.MostraMessaggio($"Maestro:Ottimo lavoro (Score: {percentuale:F0}%)\nnon si accorgeranno di nulla.\nVa a vedere alla tela il confronto e poi esci di qui.");
                }
                else
                {
                    DialogueManager.Instance.MostraMessaggio($"Maestro:Che schifo di falso! (Score: {percentuale:F0}%)\nCi scopriranno tutti!\nVa a vedere alla tela il confronto  e poi esci di qui.");
                }
                MaestroManager.Instance.AvanzaFase();
                break;
        }
    }
}