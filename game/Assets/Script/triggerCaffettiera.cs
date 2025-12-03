using UnityEngine;
using System.Collections;

public class triggerCaffettiera : MonoBehaviour { 



    [Header("Impostazioni")]
    public float activationDelay = 2.0f; // Dopo quanti secondi si attiva il trigger
    public SphereCollider triggerArea;   // Il collider grande che il nemico deve sentire
    public float lifetime = 5f;    // tempo prima che l’oggetto scompaia
    private SpawnCaffettiera player;         // riferimento al player


    void Start()
    {
        // 1. Assicuriamoci che il collider di rilevamento sia spento all'inizio
        if (triggerArea != null)
        {
            triggerArea.enabled = false;
        }
        else
        {
            // Se ti sei dimenticato di collegarlo nell'inspector, prova a prenderlo da solo
            triggerArea = GetComponent<SphereCollider>();
            if (triggerArea != null) triggerArea.enabled = false;
        }

        player = FindFirstObjectByType<SpawnCaffettiera>();

        // 2. Avvia il conteggio
        StartCoroutine(ActivateItem());
        // Avvia il timer per la distruzione
        StartCoroutine(DestroyAfterTime());
    }

    IEnumerator ActivateItem()
    {
        // Aspetta i secondi definiti
        yield return new WaitForSeconds(activationDelay);

        // Attiva il collider
        if (triggerArea != null)
        {
            triggerArea.enabled = true;
            Debug.Log("Oggetto attivo! I nemici ora possono sentirlo.");
        }
    }

    IEnumerator DestroyAfterTime()
    {
        // aspetta la lifetime
        yield return new WaitForSeconds(lifetime);

        // Ripristina l'item al player
        if (player != null)
        {
            player.RestoreItem();
            Debug.Log("Item ripristinato al player!");
        }

        // Distruggi l'oggetto
        Destroy(gameObject);
    }
}