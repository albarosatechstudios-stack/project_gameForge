using UnityEngine;
using UnityEngine.UI; // Se usi la UI per mostrare le info

public class QuadroController : MonoBehaviour
{
    public DatiQuadro dati; // Trascina qui il file ScriptableObject creato
    
    [Space]
    public MeshRenderer telaRenderer; // Il piano/mesh dove mostrare l'immagine

    void Start()
    {
        if (dati != null)
            ConfiguraQuadro();
    }

    public void ConfiguraQuadro()
    {
        // Applica la texture principale al materiale
        // Nota: Assicurati che il materiale della tela usi uno shader che accetti texture (es. Standard o Unlit)
        telaRenderer.material.mainTexture = dati.immaginePrincipale;
        
        Debug.Log($"Quadro caricato: {dati.titolo}");
    }

    // Esempio di funzione per scambiare le immagini (confronto)
    public void MostraConfronto(bool attiva)
    {
        telaRenderer.material.mainTexture = attiva ? dati.immagineConfronto : dati.immaginePrincipale;
    }
}