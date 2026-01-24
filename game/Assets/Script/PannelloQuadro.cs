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
        if(GameManager.Instance.CurrentState!=GameState.Thief){
           
            GameManager.Instance.ChangeState(GameState.NoPause);
        immagine.sprite = img;
        testo.text = desc;
       
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
