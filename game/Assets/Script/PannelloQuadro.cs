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
   
    public void attiva(bool b)
    {
        gameObject.SetActive(b); 
        
    }

    public void MostraQuadro(Sprite img, string desc)
    {
        // Questo ti dirà esattamente cosa sta succedendo in Console
        Debug.Log("Stato attuale: " + GameManager.Instance.CurrentState);

        if (GameManager.Instance.CurrentState != GameState.Thief)
        {
            Debug.Log("L'IF è passato! stato:" );
            GameManager.Instance.ChangeState(GameState.NoPause);
            immagine.sprite = img;
            testo.text = desc;
            gameObject.SetActive(true); // Assicurati che sia attivo!
        }
        else
        {
            Debug.Log("L'IF è fallito: lo stato era Thief.");
        }
    }

    public void ChiudiPannello()
    {
        GameManager.Instance.ChangeState(GameState.Visitor);
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
