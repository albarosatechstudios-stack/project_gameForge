using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PannelloQuadro : MonoBehaviour
{
    public Image immagine;
    public TextMeshProUGUI testo;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void MostraQuadro(Sprite img, string desc)
    {
        immagine.sprite = img;
        testo.text = desc;
        gameObject.SetActive(true);
    }

    public void ChiudiPannello()
    {
       
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ChiudiPannello();
        }
    }
}
