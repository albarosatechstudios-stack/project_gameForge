using UnityEngine;
using UnityEngine.InputSystem;

public class QuadroInterattivo : MonoBehaviour
{
    public Sprite immagineQuadro;
    [TextArea] public string descrizioneQuadro;
    public PannelloQuadro pannello;

    public void Update()
    {
       
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            print(immagineQuadro.name);
            pannello.MostraQuadro(immagineQuadro, descrizioneQuadro);
        }
    }
   
}
