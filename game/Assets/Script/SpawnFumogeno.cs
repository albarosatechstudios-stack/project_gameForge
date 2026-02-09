using UnityEngine;
using UnityEngine.InputSystem;
public class SpawnFumogeno : MonoBehaviour
{
    [Header("Cosa lanciare")]
    public GameObject itemPrefab;
    public Transform spawnPoint;

    [Header("Fisica")]
    public float throwForce = 5f;

    private int numberOfItem = 1;



    void Update()
    {

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame && GameManager.Instance.CurrentState == GameState.Thief)
        {
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        if (itemPrefab != null && spawnPoint != null && numberOfItem == 1)
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

    public void RestoreItem()
    {
        numberOfItem = 1;
    }

    public bool IsReady()
    {
        return numberOfItem > 0;
    }

}
