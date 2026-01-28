using UnityEngine;
using UnityEngine.AI;

public class NPCWander : MonoBehaviour
{
    [Header("Impostazioni Navigazione")]
    public float raggioMovimento = 10f; // Quanto lontano può andare
    public float tempoAttesa = 2f;      // Quanto aspetta prima di muoversi di nuovo

    [Header("Velocità NPC")]
    [Range(0, 20)] // Crea uno slider comodo nell'Inspector
    public float velocita = 3.5f;       // Nuova variabile per la velocità

    private NavMeshAgent agent;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = tempoAttesa;

        // Imposta la velocità iniziale
        agent.speed = velocita;
    }

    void Update()
    {
        // Mantiene la velocità del NavMesh sincronizzata con la variabile dello script
        // Utile se vuoi cambiarla in tempo reale mentre testi il gioco
        agent.speed = velocita;

        timer += Time.deltaTime;

        // Se è passato il tempo di attesa
        if (timer >= tempoAttesa)
        {
            Vector3 nuovaPos = RandomNavSphere(transform.position, raggioMovimento, -1);
            agent.SetDestination(nuovaPos);
            timer = 0;
        }
    }

    // Funzione per trovare un punto valido sulla NavMesh
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}