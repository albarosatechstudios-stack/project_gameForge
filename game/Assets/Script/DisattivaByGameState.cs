using UnityEngine;

public class DisattivaByGameState : MonoBehaviour
{
    public GameState state;

    void Update()
    {
        // Se lo stato corrente corrisponde allo stato target
        if (GameManager.Instance.CurrentState == state)
        {
            // Itera su tutti i figli immediati di questo oggetto
            foreach (Transform child in transform)
            {
                // Controllo se è già inattivo per evitare chiamate inutili (ottimizzazione)
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                // Controllo se è già inattivo per evitare chiamate inutili (ottimizzazione)
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
}