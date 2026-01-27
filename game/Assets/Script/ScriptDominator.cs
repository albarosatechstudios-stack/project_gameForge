using UnityEngine;
using System.Collections.Generic;

public class ScriptDominator : MonoBehaviour
{
    [Header("Configurazione")]
    [Tooltip("In quale stato questo script deve 'Dominare' (cioè disattivare gli altri)?")]
    public GameState statoDiAttivazione; // Qui selezioni Visitor, Thief o NoPause nell'Inspector

    [Header("Eccezioni")]
    [Tooltip("Trascina qui gli script che NON devono mai essere toccati")]
    public List<MonoBehaviour> scriptDaSalvare;

    // Lista privata per ricordarci quali script dobbiamo gestire (per non cercarli ogni frame)
    private List<MonoBehaviour> scriptDaControllare = new List<MonoBehaviour>();

    void Start()
    {

        // 1. Al lancio del gioco, facciamo l'elenco di tutti gli script presenti sull'oggetto
        MonoBehaviour[] tuttiGliScript = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in tuttiGliScript)
        {
            // Non controlliamo noi stessi (altrimenti lo script si suicida e smette di funzionare)
            if (script == this) continue;

            // Non controlliamo quelli nella lista delle eccezioni
            if (scriptDaSalvare.Contains(script)) continue;

            // Aggiungiamo lo script alla lista di quelli che subirono il comando
            scriptDaControllare.Add(script);
        }
    }

    void Update()
    {
        // 2. Controlliamo ogni frame in che stato siamo
        if (GameManager.Instance.CurrentState == statoDiAttivazione)
        {
            // SE siamo nello stato scelto (es. Thief): DISATTIVA gli altri script
            ImpostaStatoScript(false);
        }
        else
        {
            // SE siamo in un altro stato (es. Visitor): RIATTIVA gli altri script
            // Questo è fondamentale, altrimenti la porta resta rotta per sempre!
            ImpostaStatoScript(true);
        }
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