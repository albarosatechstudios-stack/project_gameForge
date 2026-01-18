using UnityEngine;

[CreateAssetMenu(fileName = "NuovoQuadro", menuName = "Sistema Galleria/Dati Quadro")]
public class DatiQuadro : ScriptableObject
{
    [Header("Informazioni Generali")]
    public string titolo;
    [TextArea(3, 10)]
    public string info;

    [Header("Asset Grafici")]
    public Texture2D immaginePrincipale;
    public Texture2D immagineConfronto; // L'immagine per il confronto
}