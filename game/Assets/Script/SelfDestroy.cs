using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour
{
    [Header("Impostazioni Tempo")]
    [Tooltip("Dopo quanti secondi l'oggetto deve distruggersi?")]
    public float lifetime = 30f; // Default: 1 minuto

    private float currentCooldownTimer = 0f;
    private SpawnFumogeno player;         // riferimento al player


    void Start()
    {
        player = FindFirstObjectByType<SpawnFumogeno>();

        // Pianifica la distruzione di questo GameObject dopo 'lifeTime' secondi
       currentCooldownTimer = lifetime;
       StartCoroutine(DestroyAfterTime());
    }

    void Update()
    {
        currentCooldownTimer -= Time.deltaTime;
    }

    IEnumerator DestroyAfterTime()
    {
        // aspetta la lifetime
        yield return new WaitForSeconds(lifetime);

        // Ripristina l'item al player
        if (player != null)
        {
            player.RestoreItem();
            Debug.Log("fumogeno ripristinato al player!");
        }

        // Distruggi l'oggetto
        Destroy(gameObject);
    }

        public float GetTimeRemaining()
    {
        return Mathf.Max(currentCooldownTimer, 0f);
    }
}