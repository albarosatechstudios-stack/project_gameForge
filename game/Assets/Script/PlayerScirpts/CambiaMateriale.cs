using UnityEngine;

public class CambiaMateriale : MonoBehaviour
{
    [Tooltip("Trascina qui il materiale che vuoi applicare")]
    public Material nuovoMateriale;

    private Renderer _renderer;

    void Start()
    {
        // Otteniamo il componente Renderer dell'oggetto (MeshRenderer o SkinnedMeshRenderer)
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogError("Nessun componente Renderer trovato sull'oggetto!");
        }
    }

    // Questa è la funzione pubblica da chiamare per cambiare il materiale
    public void ApplicaNuovoMateriale()
    {
        if (_renderer != null && nuovoMateriale != null)
        {
            // Assegna il nuovo materiale
            _renderer.material = nuovoMateriale;
        }
        else
        {
            Debug.LogWarning("Impossibile cambiare materiale: manca il Renderer o il Nuovo Materiale non è assegnato.");
        }
    }
}