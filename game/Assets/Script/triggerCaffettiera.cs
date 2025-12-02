using UnityEngine;
using System.Collections;

public class triggerCaffettiera : MonoBehaviour { 



    [Header("Impostazioni")]
    public float activationDelay = 2.0f; // Dopo quanti secondi si attiva il trigger
    public SphereCollider triggerArea;   // Il collider grande che il nemico deve sentire

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

        // 2. Avvia il conteggio
        StartCoroutine(ActivateItem());
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
}