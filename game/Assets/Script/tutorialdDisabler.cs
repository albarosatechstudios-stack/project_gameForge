using UnityEngine;
using UnityEngine.InputSystem;


public class TutorialdDisabler : MonoBehaviour
{
    [Tooltip("Trascina qui l'oggetto del tutorial che deve sparire (es. la freccia o il testo)")]
    public GameObject oggettoDaDisattivare;
    private bool inTrigger = false;

    // Unity chiamer� OnMouseDown su TUTTI gli script attaccati a questo oggetto
    // Quindi verr� eseguito sia questo, sia il tuo che apre la UI.


    void Update()
    {

        // Interagisci solo se sei nel trigger e premi il tasto sinistro del mouse
        if (inTrigger && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Distrutto");
            oggettoDaDisattivare.SetActive(false);
            Destroy(this);
        }

        // 2. Opzionale: Distruggiamo questo script cos� non occupa pi� memoria
        // dato che il tutorial per questo step � finito.
        
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