using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnCaffettiera : MonoBehaviour
{
    [Header("Cosa lanciare")]
    public GameObject itemPrefab;
    public Transform spawnPoint;

    [Header("Fisica")]
    public float throwForce = 5f;

    [Header("Ricarica (Cooldown)")]
    public float cooldownTime = 5f; // Tempo di attesa in secondi
    private float currentCooldownTimer = 0f;
    private int numberOfItem = 1;

    void Update()
    {
        // GESTIONE TIMER
        // Se non abbiamo l'oggetto, il timer scorre
        if (numberOfItem == 0)
        {
            currentCooldownTimer -= Time.deltaTime;

            // Se il timer finisce, ricarichiamo
            if (currentCooldownTimer <= 0f)
            {
                RestoreItem();
            }
        }

        // INPUT (Lancia solo se numberOfItem > 0)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && GameManager.Instance.CurrentState == GameState.Thief)
        {
            SpawnItem();
        }
    }

    void SpawnItem()
    {
        if (itemPrefab != null && spawnPoint != null && numberOfItem > 0)
        {
            GameObject newItem = Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);

            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(spawnPoint.forward * throwForce, ForceMode.Impulse);
            }

            numberOfItem--; // Consumiamo l'oggetto
            currentCooldownTimer = cooldownTime; // Facciamo partire il timer
        }
    }

    public void RestoreItem()
    {
        numberOfItem = 1;
        currentCooldownTimer = 0f;
    }

    // --- Metodo pubblico per l'HUD ---
    // Restituisce il tempo mancante (0 se pronto)
    public float GetTimeRemaining()
    {
        return Mathf.Max(currentCooldownTimer, 0f);
    }

    // Dice se è pronto (utile per cambiare colore al testo)
    public bool IsReady()
    {
        return numberOfItem > 0;
    }
}