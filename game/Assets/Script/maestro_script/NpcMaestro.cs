using UnityEngine;
using UnityEngine.InputSystem; // Fondamentale per il New Input System

public class NPCMaestro : MonoBehaviour
{
    [Header("Riferimenti")]
    public MaestroVisuals visuals;

    [Header("Obiettivo Attuale")]
    public string objectiveName = "Ragazza con Perle"; // qui inserire il nome del quadro da vedere

    //[Header("Dialoghi")]
    //[TextArea] public string dialogoThief = "AL LADRO! GUARDIE!";
    //[TextArea] public string dialogoPrimaVolta = "Benvenuto viaggiatore! Piacere di conoscerti.";
    //[TextArea] public string dialogoDopoQuadro = "Quel quadro nasconde un terribile segreto...";
    //[TextArea] public string dialogoStandard = "Bella giornata, vero?";
    [Header("Valutazione Disegno")]
    public SimpleLineComparerIgnoreBG comparatore; // Trascina qui il componente SimpleLineComparer
    public Texture2D immagineReference;            // Trascina qui l'immagine da copiare (es. saveImageDOnna)
    [Range(0, 100)] public float sogliaVittoria = 70f; // Percentuale minima per vincere
    // Variabile privata per sapere se il giocatore � vicino
    private bool giocatoreInZona = false;

    [Header("Oggetto Risultato")]
    public Renderer oggettoDaColorare;


    void Start()
    {
        MaestroManager.Instance.setNameObjective(objectiveName);
    }

    private void Update()
    {
        // Controlliamo l'input SOLO se il giocatore � nella zona trigger
        if (giocatoreInZona)
        {
            // Esempio: Tasto "E" della tastiera, oppure Tasto Sinistro del Mouse
            if (Keyboard.current.eKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
            {
                Interagisci();
            }
        }
    }

    // --- RILEVAMENTO ZONA (TRIGGER) ---

    private void OnTriggerEnter(Collider other)
    {
        // Controlliamo se chi � entrato � il Player
        // NOTA: Devi assicurarti che il tuo Player abbia il Tag "Player"
        if (other.CompareTag("Player"))
        {
            giocatoreInZona = true;
            Debug.Log("Sei vicino al Maestro. Premi 'E' o Clicca per parlare.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            giocatoreInZona = false;

            // Opzionale: Se te ne vai mentre il dialogo � aperto, lo chiudiamo?
            // DialogueManager.Instance.ChiudiDialogo();
            Debug.Log("Ti sei allontanato.");
        }
    }

    // --- LOGICA INTERAZIONE (IDENTICA A PRIMA) ---

    public void Interagisci()
    {
        MaestroManager manager = MaestroManager.Instance;
        manager.ConfermaInterazioneAvvenuta(); // Spegne l'icona "!"
        manager.disableComic();
        switch (manager.faseAttuale)
        {
            case QuestMaestro.Inizio:
                DialogueManager.Instance.MostraMessaggio("Benvenuto! Vai a guardare il Quadro " + objectiveName + " nel corridoio.");
                manager.AvanzaFase(); // Passa a 'DeveVedereQuadro'
                break;

            case QuestMaestro.DeveVedereQuadro:
                DialogueManager.Instance.MostraMessaggio("Ancora qui? Vai a vedere quel quadro!");
                break;

            case QuestMaestro.QuadroVisto:
                DialogueManager.Instance.MostraMessaggio("Bello, vero? Ora crea un falso e sostituiscilo all'originale.");
                manager.AvanzaFase(); // Passa a 'CreazioneFalso'
                break;

            case QuestMaestro.CreazioneFalso:
                // controllo se il quadro falso è stato realizzato per avere due dialoghi
                // falso pronto -> ora vai e sostituisci i quadri, non farti arrestare!
                // falso non pronto -> quanto ci vuole a realizzare questo falso? non cincischiare!
                if (MaestroManager.Instance.isRealised)
                {
                    DialogueManager.Instance.MostraMessaggio("Vai e sostituisci i quadri, non farti <b>ARRESTARE</b>!\nRicordati di tornare qui dopo lo scambio.");
                }
                else
                {
                    DialogueManager.Instance.MostraMessaggio("quanto ci vuole a realizzare questo falso? non cincischiare!");
                }
                // Qui potresti controllare se il giocatore ha finito il lavoro
                break;

            case QuestMaestro.FalsoPronto:
                // -----------------------------------------------------------
                // 1. ESEGUI IL CONFRONTO (Calcolo LIVE)
                // -----------------------------------------------------------
                if (comparatore == null || immagineReference == null)
                {
                    Debug.LogError("ERRORE: Manca il comparatore o l'immagine reference nello script Maestro!");
                    return;
                }

                // Chiediamo al comparatore la percentuale di somiglianza
                float percentuale = comparatore.CompareWithSavedDrawing(immagineReference);
                Debug.Log($"Il Maestro valuta il disegno... Punteggio: {percentuale}%");

                // -----------------------------------------------------------
                // 2. VALUTA LA CONDIZIONE
                // -----------------------------------------------------------
                // Qui decidiamo se il 'falsoDiAltaQualita' è vero o falso in base al numero
                if (oggettoDaColorare != null && comparatore.textureRisultato != null)
                {
                    // Cambia la texture principale del materiale dell'oggetto
                    oggettoDaColorare.material.mainTexture = comparatore.textureRisultato;

                    // Opzionale: Se l'immagine appare scura/strana, prova a cambiare lo shader in "Unlit/Texture" nell'editor
                    Debug.Log("Texture risultato applicata all'oggetto!");
                }
                else
                {
                    Debug.LogWarning("Impossibile applicare texture: Manca l'oggettoDaColorare o la texture non è stata generata.");
                }
                // -------------------------------------------------------------
                bool isAltaQualita = percentuale >= sogliaVittoria;

                // -----------------------------------------------------------
                // 3. IL TUO IF / ELSE ORIGINALE
                // -----------------------------------------------------------
                if (isAltaQualita)
                {
                    // VITTORIA
                    DialogueManager.Instance.MostraMessaggio($"Ottimo lavoro (Score: {percentuale:F0}%), non si accorgeranno di nulla. Va a vedere alla tela il confronto e poi esci di qui.");
                }
                else
                {
                    // SCONFITTA
                    DialogueManager.Instance.MostraMessaggio($"Che schifo di falso! (Score: {percentuale:F0}%) Ci scopriranno tutti! Va a vedere alla tela il confronto  e poi esci di qui.");
                }

                // Caricamento scena o fine logica
                //GameManager.Instance.LoadLastScena();
                break;
        }
    }
}