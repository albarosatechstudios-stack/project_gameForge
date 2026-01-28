using UnityEngine;

public enum QuestMaestro
{
    Inizio,             // Mai parlato
    DeveVedereQuadro,   // Parlato la prima volta, mandato al quadro
    QuadroVisto,        // Ha interagito col quadro, deve tornare dal maestro
    CreazioneFalso,     // Il maestro ha chiesto il falso, il giocatore ci lavora
    FalsoPronto,        // Il falso è stato creato/sostituito, pronto per il finale
    FineGioco           // Dialogo finale concluso
}

public class MaestroManager : MonoBehaviour
{
    public static MaestroManager Instance;

    [Header("Progressione Quest")]
    public QuestMaestro faseAttuale = QuestMaestro.Inizio;
    public bool falsoDiAltaQualita = false; // Determina se il finale sarà positivo o negativo

    [Header("Stato Interazione UI")]
    public bool iconaDaMostrare = true; // Parte true per il primo saluto

    [Header("Memoria Eventi (Legacy)")]
    public bool primoIncontroAvvenuto = false; 

    [Header("Stato Percepito (Dal GameManager)")]
    public GameState statoMentaleMaestro;

    string nameObjective ="";

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null) { Instance = this; 
        DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable() => GameManager.OnStateChanged += ReazioneAlCambioStato;
    private void OnDisable() => GameManager.OnStateChanged -= ReazioneAlCambioStato;

    // --- LOGICA DI AVANZAMENTO ---

    /// Fa avanzare la quest alla fase successiva e riaccende l'icona sulla testa del Maestro.
    public void AvanzaFase()
    {
        if (faseAttuale != QuestMaestro.FineGioco)
        {
            faseAttuale++;
            iconaDaMostrare = true; // Nuova fase = Nuova cosa da dire!
            Debug.Log("[MaestroManager] Nuova fase : " + faseAttuale);
        }
    }

    /// Chiamata specifica per quando il giocatore finisce il falso.
    public void SetFalsoPronto(bool isBuonaQualita)
    {
        falsoDiAltaQualita = isBuonaQualita;
        faseAttuale = QuestMaestro.FalsoPronto;
        iconaDaMostrare = true;
        Debug.Log("[MaestroManager] Il falso è pronto. Qualità ottima: " + isBuonaQualita);
    }

    // --- GESTIONE ICONA E STATI ---

    private void ReazioneAlCambioStato(GameState nuovoStato)
    {
        // Se il giocatore diventa un ladro, il maestro deve reagire a prescindere dalla quest
        if (statoMentaleMaestro != nuovoStato)
        {
            statoMentaleMaestro = nuovoStato;
            iconaDaMostrare = true; 
        }
    }

    // Chiamata da NPCMaestro quando il giocatore interagisce
    public void ConfermaInterazioneAvvenuta()
    {
        iconaDaMostrare = false;

        // Segna il primo incontro se siamo all'inizio
        if (faseAttuale == QuestMaestro.Inizio)
        {
            primoIncontroAvvenuto = true;
        }
    }

    // Metodo di supporto per lo script del Quadro
    public void SegnalaQuadroVisto(string openendPicture){
        if (faseAttuale == QuestMaestro.DeveVedereQuadro){
            // controllo per capire che il quadro aperto è quello che dice il maestro
            if (nameObjective == openendPicture){
                Debug.Log("[Manager] Ottimo, hai trovato il quadro giusto: " + openendPicture);
                AvanzaFase();
            }else{
                Debug.Log("[Manager] Questo è " + openendPicture + ", ma io cerco " + nameObjective);
            }
        }
    }

    public void setNameObjective(string name)
    {
        nameObjective = name;
    }
}