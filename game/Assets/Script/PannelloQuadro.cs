using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class PannelloQuadro : MonoBehaviour
{
    public Image immagine;
    public TextMeshProUGUI testo;

  
    public void attiva(bool b)
    {
        gameObject.SetActive(b); 
        
    }

    public void MostraQuadro(Sprite img, string desc)
    {
        // Questo ti dira esattamente cosa sta succedendo in Console
        Debug.Log("Stato attuale: " + GameManager.Instance.CurrentState);

        if (GameManager.Instance.CurrentState != GameState.Thief)
        {
            gameObject.SetActive(true); 
            Debug.Log("L'IF e' passato! stato:" );
            GameManager.Instance.ChangeState(GameState.NoPause);
            immagine.sprite = img;
            testo.text = desc;
        }
        else
        {
            Debug.Log("L'IF e' fallito: lo stato era Thief.");
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
