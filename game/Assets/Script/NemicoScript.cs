using UnityEngine;
using UnityEngine.AI;

// Aggiunto lo stato DISTRACTED
public enum STATE { VIGILE, CHASING, SLEEPING, OFF, DISTRACTED }

public class NemicoScript : MonoBehaviour
{
    [Header("Componenti")]
    private NavMeshAgent agent;
    public Transform player;

    [Header("Stato Attuale")]
    public STATE state = STATE.VIGILE;

    [Header("Impostazioni Pattuglia")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer;

    [Header("Impostazioni Distrazione")]
    public float distractionTime = 5f; // Quanto tempo rimane distratto sull'oggetto
    private float distractionTimer;
    private Vector3 distractionPoint; // Dove si trova l'oggetto

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolTimer = patrolWaitTime;
    }

    void Update()
    {
        switch (state)
        {
            case STATE.VIGILE:
                PatrolLogic();
                break;

            case STATE.CHASING:
                ChaseLogic();
                break;

            case STATE.DISTRACTED:
                DistractedLogic();
                break;

            case STATE.SLEEPING:
                SleepLogic();
                break;

            case STATE.OFF:
                break;
        }
    }

    // --- LOGICHE DI COMPORTAMENTO ---

    void PatrolLogic()
    {
        if (agent.isStopped) agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                Vector3 newPos = RandomNavSphere(transform.position, patrolRadius, -1);
                agent.SetDestination(newPos);
                patrolTimer = 0;
            }
        }
    }

    void ChaseLogic()
    {
        if (player != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void DistractedLogic()
    {
        agent.isStopped = false;

        // 1. Vai verso l'oggetto
        agent.SetDestination(distractionPoint);

        // 2. Se è arrivato vicino all'oggetto
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // 3. Aspetta (Esamina l'oggetto)
            distractionTimer += Time.deltaTime;

            if (distractionTimer >= distractionTime)
            {
                // 4. Tempo scaduto: torna a fare la guardia
                state = STATE.VIGILE;
                distractionTimer = 0;
            }
        }
    }

    void SleepLogic()
    {
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // --- SENSORI (TRIGGER) ---
    void OnTriggerStay(Collider other)
    {
        // Se il player è ANCORA dentro l'area e il nemico è tornato VIGILE
        if (other.CompareTag("Player") && state == STATE.VIGILE)
        {
            // ...ricomincia subito a inseguirlo!
            state = STATE.CHASING;
            player = other.transform;
        }
    }

        void OnTriggerEnter(Collider other)
    {
        // LOGICA PLAYER: Se vede il player e non è già distratto o addormentato
        if (other.CompareTag("Player") && state == STATE.VIGILE)
        {
            state = STATE.CHASING;
            player = other.transform;
        }

        // LOGICA ITEM: Se entra nell'area di un oggetto (es. sasso, cibo)
        // IMPORTANTE: Assicurati che l'oggetto abbia il Tag "Item"
        if (other.CompareTag("Item"))
        {
            // Funziona se è Vigile o se sta Inseguendo (l'oggetto lo distrae dall'inseguimento?)
            // Se vuoi che l'oggetto lo distragga SOLO se non ti ha visto, aggiungi: && state != STATE.CHASING
            if (state == STATE.VIGILE || state == STATE.CHASING)
            {
                state = STATE.DISTRACTED;
                distractionPoint = other.transform.position; // Memorizza dove andare
                distractionTimer = 0; // Resetta il timer

                // Opzionale: Se vuoi che smetta subito di inseguire il player
                agent.ResetPath();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && state == STATE.CHASING)
        {
            state = STATE.VIGILE;
            agent.ResetPath();
        }

        // Nota: Non mettiamo logica per l'uscita dall'Item, 
        // perché gestiamo la fine della distrazione col timer.
    }

    // --- UTILITIES ---
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}