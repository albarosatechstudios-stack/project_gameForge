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

    [Header("Impostazioni Pattuglia")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer;

    [Header("Impostazioni Inseguimento")]
    // Distanza di sicurezza: il nemico punterà a una coordinata a questa distanza dal centro del player
    public float stopDistanceFromPlayer = 2.0f;

    [Header("Impostazioni Distrazione")]
    public float distractionTime = 5f;
    private float distractionTimer;
    private Vector3 distractionPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolTimer = patrolWaitTime;

        // Impostiamo una stopping distance piccola di base. 
        // La distanza vera la calcoliamo noi matematicamente in ChaseLogic.
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
                SleepLogic();
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

            // 1. Calcoliamo il vettore direzione: Dal Player VERSO il Nemico
            // Questo ci serve per trovare il punto sul "bordo" del cerchio attorno al player
            Vector3 directionFromPlayer = (transform.position - player.position).normalized;

            // 2. Calcoliamo la coordinata esatta dove il nemico deve andare.
            // Formula: PosizionePlayer + (Direzione * DistanzaDesiderata)
            Vector3 safeDestination = player.position + (directionFromPlayer * stopDistanceFromPlayer);

            // 3. Impostiamo quella coordinata come destinazione
            agent.SetDestination(safeDestination);

            // 4. Se il nemico è molto vicino alla sua destinazione sicura, lo facciamo ruotare verso il player
            // altrimenti guarderebbe il punto vuoto calcolato.
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
        // Se sta dormendo, ferma l'agente e cancella il percorso
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // --- UTILITIES ---

    // Ruota il nemico verso il target (solo asse Y) per evitare che guardi il nulla quando è fermo
    void LookAtTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0; // Ignora l'altezza

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

        if (other.CompareTag("Fumogeno"))
        {
            if (state != STATE.SLEEPING)
            {
                Debug.Log("Nemico investito dal fumo! Zzz...");
                state = STATE.SLEEPING;
                agent.ResetPath();
            }
        }

        if (other.CompareTag("Item"))
        {
            Debug.Log("Sento l'odore del caffè...");
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
        if (other.CompareTag("Player"))
        {
            if (state != STATE.SLEEPING)
            {
                state = STATE.CHASING;
                player = other.transform;
            }
        }

        if (other.CompareTag("Fumogeno"))
        {
            state = STATE.SLEEPING;
            agent.ResetPath();
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

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && state == STATE.CHASING)
        {
            state = STATE.VIGILE;
            agent.ResetPath();
        }

        if (other.CompareTag("Fumogeno"))
        {
            if (state == STATE.SLEEPING)
            {
                Debug.Log("Fumo sparito, nemico sveglio!");
                state = STATE.VIGILE;
                patrolTimer = 0;
            }
        }
    }
}