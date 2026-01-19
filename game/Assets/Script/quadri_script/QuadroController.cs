using UnityEngine;
using UnityEngine.UI; // Se usi la UI per mostrare le info
using UnityEngine.InputSystem;


public class QuadroController : MonoBehaviour
{
    public DatiQuadro dati; // Trascina qui il file ScriptableObject creato
    
    [Space]
    public MeshRenderer telaRenderer; // Il piano/mesh dove mostrare l'immagine

    public PannelloQuadro pannello;
    private bool inTrigger = false;



    void Start()
    {
        if (dati != null)
            ConfiguraQuadro();
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused) return;
        if (inTrigger && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log(dati.titolo);
            pannello.MostraQuadro(dati.immaginePrincipale, dati.info);
        }
    }

    public void ConfiguraQuadro()
    {
        // Applica la texture principale al materiale
        // Nota: Assicurati che il materiale della tela usi uno shader che accetti texture (es. Standard o Unlit)
        // telaRenderer.material.mainTexture = dati.immaginePrincipale;
        
        Debug.Log($"Quadro caricato: {dati.titolo}");
    }

    // Esempio di funzione per scambiare le immagini (confronto)
    public void MostraConfronto(bool attiva)
    {
        // telaRenderer.material.mainTexture = attiva ? dati.immagineConfronto : dati.immaginePrincipale;
    }


        private void OnTriggerEnter(Collider other)
    {
        // Il player entra nel trigger
        if (other.CompareTag("Player"))
        {
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Il player esce dal trigger
        if (other.CompareTag("Player"))
        {
            inTrigger = false;
        }
    }
}