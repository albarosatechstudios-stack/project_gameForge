using UnityEngine;
using System.Collections.Generic;

public class ScriptDominator : MonoBehaviour
{
    [Header("Configurazione")]
    [Tooltip("Aggiungi qui TUTTI gli stati in cui gli script devono essere ATTIVI (funziona come una 'whitelist').")]
    public QuestMaestro[] statiDiAttivazione; // ORA È UN ARRAY (Lista)

    [Header("Eccezioni")]
    [Tooltip("Trascina qui gli script che NON devono mai essere toccati (rimarranno sempre attivi)")]
    public List<MonoBehaviour> scriptDaSalvare;

    // Lista privata per ricordarci quali script dobbiamo gestire
    private List<MonoBehaviour> scriptDaControllare = new List<MonoBehaviour>();

    void Start()
    {
        // 1. Al lancio del gioco, facciamo l'elenco di tutti gli script presenti sull'oggetto
        MonoBehaviour[] tuttiGliScript = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in tuttiGliScript)
        {
            // Non controlliamo noi stessi
            if (script == this) continue;

            // Non controlliamo quelli nella lista delle eccezioni
            if (scriptDaSalvare.Contains(script)) continue;

            // Aggiungiamo lo script alla lista di quelli che subiranno il comando
            scriptDaControllare.Add(script);
        }
    }

    void Update()
    {
        // 2. Controlliamo se lo stato attuale è PERMESSO
        QuestMaestro faseCorrente = MaestroManager.Instance.faseAttuale;
        bool deveEssereAttivo = false;

        // Scansioniamo l'array: se troviamo la fase corrente nella lista, attiviamo tutto
        foreach (QuestMaestro statoPermesso in statiDiAttivazione)
        {
            if (faseCorrente == statoPermesso)
            {
                deveEssereAttivo = true;
                break; // Trovato! Non serve controllare oltre
            }
        }

        // Applichiamo il risultato (True o False) a tutti gli script
        ImpostaStatoScript(deveEssereAttivo);
    }

    // Funzione helper per accendere/spegnere la lista
    void ImpostaStatoScript(bool statoAttivo)
    {
        foreach (MonoBehaviour script in scriptDaControllare)
        {
            // Facciamo il cambio solo se necessario (piccola ottimizzazione)
            if (script.enabled != statoAttivo)
            {
                script.enabled = statoAttivo;
            }
        }
    }
}