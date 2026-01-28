using UnityEngine;
using UnityEngine.InputSystem;

public class QuadroInterattivo : MonoBehaviour
{
    public Sprite immagineQuadro;
    [TextArea] public string descrizioneQuadro;
    public PannelloQuadro pannello;

    private bool inTrigger = false;

    private bool giaInteragito = false;

    void Update()
    {
       
        // Interagisci solo se sei nel trigger e premi il tasto sinistro del mouse
        if (inTrigger && Mouse.current.leftButton.wasPressedThisFrame && GameManager.Instance.CurrentState != GameState.Thief) 
        {
            Debug.Log(immagineQuadro.name);
            pannello.attiva(true);
            pannello.MostraQuadro(immagineQuadro, descrizioneQuadro);
            if (!giaInteragito)
            {
                giaInteragito = true;
               // Diciamo al manager che il quadro è stato visto
                //MaestroManager.Instance.SegnalaQuadroVisto();
            }
        }
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
