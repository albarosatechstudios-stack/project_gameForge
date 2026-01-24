using UnityEngine;

public class SleepWave : MonoBehaviour
{
    // Questo script va sulla sfera/onda
    public float maxRadius = 20f;
    public float speed = 15f;

    void Start()
    {
        Destroy(gameObject, 3f); // Dura 3 secondi poi sparisce

        // Setup automatico collider se ti dimentichi
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null) col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
    }

    void Update()
    {
        // Si allarga
        if (transform.localScale.x < maxRadius)
        {
            transform.localScale += Vector3.one * speed * Time.deltaTime;
        }
    }
}