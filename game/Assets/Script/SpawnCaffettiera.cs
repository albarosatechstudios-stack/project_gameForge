using UnityEngine;
using UnityEngine.InputSystem; 

public class SpawnCaffettiera : MonoBehaviour
{
    [Header("Cosa lanciare")]
    public GameObject itemPrefab;
    public Transform spawnPoint;

    [Header("Fisica")]
    public float throwForce = 5f;

    private int numberOfItem = 1;

 

    void Update()
    {
       
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && GameManager.Instance.CurrentState == GameState.Thief)
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