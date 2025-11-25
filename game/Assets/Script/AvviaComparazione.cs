using UnityEngine;
using UnityEngine.InputSystem;

public class AvviaComparazione : MonoBehaviour
{
    private bool isTrigger = false;

    public SimpleLineComparerIgnoreBG confronto; 

    public Texture2D modelloReference;  // l'immagine di riferimento da passare

    
    
    void Update()
    { 
        if (isTrigger && Mouse.current.leftButton.wasPressedThisFrame)
        {
            print("Trigger bottone rosso");
            if (confronto != null)
            {
                float similarityPercent = confronto.CompareWithSavedDrawing(modelloReference);

                if (similarityPercent >= 0f)
                    Debug.Log($"Percentuale similarità: {similarityPercent:F2}%");
                else
                    Debug.LogWarning("Confronto non riuscito.");
            }
            else
            {
                Debug.LogError("Riferimento a SimpleLineComparerIgnoreBG mancante!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("trigger1");
            isTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            isTrigger = false;
        }
    }
}
