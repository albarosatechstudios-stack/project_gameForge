using UnityEngine;
using System.Collections.Generic; // Serve per usare le Liste

public class GestoreVisibilitaPerStato : MonoBehaviour
{
    [Header("Configurazione")]
    [Tooltip("Inserisci qui gli stati in cui questo oggetto deve SPARIRE.")]
    public List<GameState> statiInCuiDisabilitare;

    // Componenti da accendere/spegnere
    private Renderer[] renderers;
    private Collider[] colliders;
    private Canvas[] canvases; // Caso extra: se fosse una UI

    void Awake()
    {
        // 1. Troviamo tutti i pezzi che compongono l'oggetto (anche nei figli)
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        canvases = GetComponentsInChildren<Canvas>();
    }

    void OnEnable()
    {
        // Ci iscriviamo all'evento
        GameManager.OnStateChanged += ControllaStato;

        // Controllo iniziale appena parte la scena (se il GM esiste già)
        if (GameManager.Instance != null)
        {
            ControllaStato(GameManager.Instance.CurrentState);
        }
    }

    void OnDisable()
    {
        // Ci disiscriviamo per evitare errori
        GameManager.OnStateChanged -= ControllaStato;
    }

    // Questa funzione viene chiamata ogni volta che lo stato cambia
    void ControllaStato(GameState nuovoStato)
    {
        // Verifichiamo se il nuovo stato è nella lista dei "Vietati"
        bool deveSparire = statiInCuiDisabilitare.Contains(nuovoStato);

        if (deveSparire)
        {
            ImpostaVisibilita(false); // Nascondi
        }
        else
        {
            ImpostaVisibilita(true); // Mostra
        }
    }

    void ImpostaVisibilita(bool visibile)
    {
        // Accende o spegne la grafica (Mesh, SkinnedMesh, etc)
        foreach (var r in renderers) r.enabled = visibile;

        // Accende o spegne la fisica (BoxCollider, MeshCollider, etc)
        foreach (var c in colliders) c.enabled = visibile;

        // Accende o spegne eventuali Canvas UI (nuvolette, testi)
        foreach (var cv in canvases) cv.enabled = visibile;
    }
}