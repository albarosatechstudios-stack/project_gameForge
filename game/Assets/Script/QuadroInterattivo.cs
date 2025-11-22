using UnityEngine;
using UnityEngine.InputSystem;

public class QuadroInterattivo : MonoBehaviour
{
    public Sprite immagineQuadro;
    [TextArea] public string descrizioneQuadro;
    public PannelloQuadro pannello;

    private bool inTrigger = false;

    void Update()
    {
        if (inTrigger)
        {
            print("triggerato");
        }
        // Interagisci solo se sei nel trigger e premi il tasto sinistro del mouse
        if (inTrigger && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log(immagineQuadro.name);
            pannello.MostraQuadro(immagineQuadro, descrizioneQuadro);
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
