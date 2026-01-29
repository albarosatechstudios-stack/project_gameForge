using UnityEngine;
using UnityEngine.InputSystem; // Fondamentale per il New Input System

public class NPCMaestro : MonoBehaviour
{
    [Header("Riferimenti")]
    public MaestroVisuals visuals;

    [Header("Obiettivo Attuale")]
    public string objectiveName = "Ragazza con Perle"; // qui inserire il nome del quadro da vedere

    [Header("Dialoghi")]
    [TextArea] public string dialogoThief = "AL LADRO! GUARDIE!";
    [TextArea] public string dialogoPrimaVolta = "Benvenuto viaggiatore! Piacere di conoscerti.";
    [TextArea] public string dialogoDopoQuadro = "Quel quadro nasconde un terribile segreto...";
    [TextArea] public string dialogoStandard = "Bella giornata, vero?";

    // Variabile privata per sapere se il giocatore � vicino
    private bool giocatoreInZona = false;


    void Start(){
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
                if (MaestroManager.Instance.isRealised){
                    DialogueManager.Instance.MostraMessaggio("Vai e sostituisci i quadri, non farti <b>ARRESTARE</b>!\nRicordati di tornare qui dopo lo scambio.");                }
                else{
                    DialogueManager.Instance.MostraMessaggio("quanto ci vuole a realizzare questo falso? non cincischiare!");
                }
            // Qui potresti controllare se il giocatore ha finito il lavoro
            break;

        case QuestMaestro.FalsoPronto:
            // FINALE
            if (manager.falsoDiAltaQualita) {
                DialogueManager.Instance.MostraMessaggio("Ottimo lavoro, non si accorgeranno di nulla. Fine gioco.");
            } else {
                DialogueManager.Instance.MostraMessaggio("Che schifo di falso! Ci scopriranno tutti! Fine gioco.");
            }
            GameManager.Instance.LoadLastScena();
            break;
    }
}
}