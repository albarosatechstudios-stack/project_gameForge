using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [Header("Impostazioni Tempo")]
    [Tooltip("Dopo quanti secondi l'oggetto deve distruggersi?")]
    public float lifeTime = 60f; // Default: 1 minuto

    void Start()
    {
        // Pianifica la distruzione di questo GameObject dopo 'lifeTime' secondi
        Destroy(gameObject, lifeTime);
    }
}