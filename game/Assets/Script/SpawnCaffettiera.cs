using UnityEngine;
using UnityEngine.InputSystem; // NECESSARIO per il nuovo sistema

public class SpawnCaffettiera : MonoBehaviour
{
    [Header("Cosa lanciare")]
    public GameObject itemPrefab;
    public Transform spawnPoint;

    [Header("Fisica")]
    public float throwForce = 5f;

    private int numberOfItem = 1;

    // Non usiamo pi� KeyCode pubblico qui, lo mettiamo nel codice
    // oppure si usano gli Input Actions, ma per ora facciamolo semplice:

    void Update()
    {
        // Controlla se la tastiera è collegata E se il tasto E è stato premuto
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        if (itemPrefab != null && spawnPoint != null  && numberOfItem == 1)
        {
            GameObject newItem = Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);

            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(spawnPoint.forward * throwForce, ForceMode.Impulse);
            }
            
            numberOfItem--;

        }
    }

    //metodo da richiamare per ripristinare l'item
        public void  RestoreItem()
    {
        numberOfItem = 1; 
    }
}