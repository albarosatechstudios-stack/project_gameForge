using UnityEngine;

using UnityEngine;

public class MaestroManager : MonoBehaviour
{
    public static MaestroManager Instance;

    [Header("Memoria Eventi")]
    public bool primoIncontroAvvenuto = false;
    public bool segretoQuadroSbloccato = false;

    [Header("Stato Interazione")]
    public bool iconaDaMostrare = true; // Parte true perché all'inizio deve salutarti

    [Header("Stato Percepito")]
    public GameState statoMentaleMaestro;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable() => GameManager.OnStateChanged += ReazioneAlCambioStato;
    private void OnDisable() => GameManager.OnStateChanged -= ReazioneAlCambioStato;

    // 1. EVENTO: CAMBIO STATO (Es. Diventi Ladro)
    private void ReazioneAlCambioStato(GameState nuovoStato)
    {
        // Se lo stato cambia, ho cose nuove da dire!
        if (statoMentaleMaestro != nuovoStato)
        {
            statoMentaleMaestro = nuovoStato;

            // RIACCENDI L'ICONA (perché ora ho una reazione diversa, es. paura o indifferenza)
            iconaDaMostrare = true;
        }
    }

    // 2. EVENTO: QUADRO
    public void AttivaEventoQuadro()
    {
        if (!segretoQuadroSbloccato)
        {
            segretoQuadroSbloccato = true;
            Debug.Log("[MaestroManager] Evento Quadro Sbloccato!");

            // RIACCENDI L'ICONA (perché ho un commento sul quadro da fare)
            iconaDaMostrare = true;
        }
    }

    // 3. AZIONE: CONFERMA INTERAZIONE
    // Chiamata quando il giocatore clicca sul Maestro
    public void ConfermaInterazioneAvvenuta()
    {
        // Ho detto quello che dovevo dire per questo stato/evento. Spengo la luce.
        iconaDaMostrare = false;

        // Se era il primo incontro, me lo segno
        if (!primoIncontroAvvenuto)
        {
            primoIncontroAvvenuto = true;
        }
    }
}