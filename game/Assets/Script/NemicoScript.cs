using UnityEngine;
using UnityEngine.AI;

public enum STATE { VIGILE, CHASING, SLEEPING, OFF, DISTRACTED }

public class NemicoScript : MonoBehaviour
{
    [Header("Componenti")]
    private NavMeshAgent agent;
    public Transform player;

    [Header("Stato Attuale")]
    public STATE state = STATE.VIGILE;

    // --- NUOVA VARIABILE ---
    // Memorizziamo quale oggetto fumogeno ci sta facendo dormire
    private GameObject currentSmokeObject;

    [Header("Impostazioni Pattuglia")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer;

    [Header("Impostazioni Inseguimento")]
    public float stopDistanceFromPlayer = 2.0f;

    [Header("Impostazioni Distrazione")]
    public float distractionTime = 5f;
    private float distractionTimer;
    private Vector3 distractionPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolTimer = patrolWaitTime;
        if (agent != null) agent.stoppingDistance = 0.5f;
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
                SleepLogic(); // Qui c'è la modifica importante
                break;

            case STATE.OFF:
                if (!agent.isStopped) agent.isStopped = true;
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
            Vector3 directionFromPlayer = (transform.position - player.position).normalized;
            Vector3 safeDestination = player.position + (directionFromPlayer * stopDistanceFromPlayer);
            agent.SetDestination(safeDestination);

            if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                LookAtTarget(player.position);
            }
        }
    }

    void DistractedLogic()
    {
        agent.isStopped = false;
        agent.SetDestination(distractionPoint);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            distractionTimer += Time.deltaTime;
            if (distractionTimer >= distractionTime)
            {
                state = STATE.VIGILE;
                distractionTimer = 0;
            }
        }
    }

    void SleepLogic()
    {
        // 1. Assicuriamoci che stia fermo
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 2. CONTROLLO DI SICUREZZA ("BULLETPROOF")
        // Se l'oggetto fumo che mi ha addormentato non esiste più (è diventato null perché distrutto)
        // O se non ho un riferimento al fumo
        if (currentSmokeObject == null)
        {
            Debug.Log("Il fumo è svanito (Destroy), mi sveglio!");
            WakeUp();
        }
    }

    // Funzione helper per svegliarsi (usata sia in SleepLogic che OnTriggerExit)
    void WakeUp()
    {
        state = STATE.VIGILE;
        patrolTimer = 0;
        currentSmokeObject = null; // Reset del riferimento
        // animator.SetBool("IsSleeping", false);
    }

    // --- UTILITIES ---

    void LookAtTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    // --- SENSORI (TRIGGER) ---

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (state == STATE.VIGILE || state == STATE.DISTRACTED)
            {
                state = STATE.CHASING;
                player = other.transform;
            }
        }

        // Se entro nel fumo o ci sto dentro
        if (other.CompareTag("Fumogeno"))
        {
            // Salvo il riferimento all'oggetto fumo specifico!
            currentSmokeObject = other.gameObject;

            if (state != STATE.SLEEPING)
            {
                Debug.Log("Nemico investito dal fumo! Zzz...");
                state = STATE.SLEEPING;
                agent.ResetPath();
            }
        }

        if (other.CompareTag("Item"))
        {
            if (state == STATE.VIGILE || state == STATE.CHASING)
            {
                state = STATE.DISTRACTED;
                distractionPoint = other.transform.position;
                distractionTimer = 0;
                agent.ResetPath();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && state != STATE.SLEEPING)
        {
            state = STATE.CHASING;
            player = other.transform;
        }

        if (other.CompareTag("Fumogeno"))
        {
            currentSmokeObject = other.gameObject; // MEMORIZZO IL FUMO
            state = STATE.SLEEPING;
            agent.ResetPath();
        }

        if (other.CompareTag("Item") && (state == STATE.VIGILE || state == STATE.CHASING))
        {
            state = STATE.DISTRACTED;
            distractionPoint = other.transform.position;
            distractionTimer = 0;
            agent.ResetPath();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && state == STATE.CHASING)
        {
            state = STATE.VIGILE;
            agent.ResetPath();
        }

        // Manteniamo questo per quando il nemico ESCE dal fumo fisicamente (ad esempio spinto fuori)
        // Ma non facciamo più affidamento solo su questo per il Destroy
        if (other.CompareTag("Fumogeno"))
        {
            // Verifichiamo se l'oggetto da cui usciamo è quello che ci faceva dormire
            if (currentSmokeObject == other.gameObject)
            {
                Debug.Log("Sono uscito dall'area del fumo!");
                WakeUp();
            }
        }
    }
}