using UnityEngine;

public class GenerateRoom : MonoBehaviour
{
    [Header("Config")]
    public GameObject roomPrefab;   // Prefab della stanza
    public int numberOfRooms = 8;   // Numero totale di stanze (pubblico)
    public float radius = 20f;      // Raggio del cerchio

    [Header("Debug")]
    public GameObject[] generatedRooms; // Per avere riferimento alle stanze create

    void Start()
    {
        GenerateRing();
    }

    void GenerateRing()
    {
        if (roomPrefab == null)
        {
            Debug.LogError("Assegna un roomPrefab!");
            return;
        }

        generatedRooms = new GameObject[numberOfRooms];

        // Angolo tra una stanza e l’altra (360° / N)
        float angleStep = 360f / numberOfRooms;

        for (int i = 0; i < numberOfRooms; i++)
        {
            // Calcolo angolo
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            // Posizione sulla circonferenza
            Vector3 position = new Vector3(
                Mathf.Cos(rad) * radius,
                0f,
                Mathf.Sin(rad) * radius
            );

            // Rotazione verso il centro (opzionale)
            Quaternion rotation = Quaternion.LookRotation(-position.normalized, Vector3.up);

            // Instanzia la stanza
            GameObject room = Instantiate(roomPrefab, position, rotation, transform);
            room.name = $"Room_{i}";

            generatedRooms[i] = room;
        }

        Debug.Log("Ring di stanze generato. Ultima collegata alla prima.");
    }
}
