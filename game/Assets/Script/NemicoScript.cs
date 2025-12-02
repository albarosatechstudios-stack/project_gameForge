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

    [Header("Impostazioni Distrazione")]
    public float distractionTime = 5f;
    private float distractionTimer;
    private Vector3 distractionPoint;

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
                agent.isStopped = true; // Assicuriamoci che stia fermo se OFF
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
        // Se sta dormendo, deve stare fermo immobile
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
            // Qui potresti far partire l'animazione del sonno:
            // animator.SetBool("IsSleeping", true);
        }
    }

    // --- SENSORI (TRIGGER) ---

    // OnTriggerStay è vitale perché il collider del fumo SI ESPANDE
    // e investe il nemico che potrebbe essere già dentro l'area ma fermo.
    void OnTriggerStay(Collider other)
    {
        // Rilevamento Player (Solo se è sveglio!)
        if (other.CompareTag("Player"))
        {
            if (state == STATE.VIGILE || state == STATE.DISTRACTED)
            {
                state = STATE.CHASING;
                player = other.transform;
            }
        }

        // Rilevamento FUMO (Priorità assoluta)
        if (other.CompareTag("Fumogeno"))
        {
            // Va a dormire indipendentemente da cosa stava facendo
            if (state != STATE.SLEEPING)
            {
                Debug.Log("Nemico investito dal fumo! Zzz...");
                state = STATE.SLEEPING;
                agent.ResetPath(); // Dimentica dove stavi andando
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Rilevamento Player
        if (other.CompareTag("Player"))
        {
            // Insegue solo se non sta dormendo
            if (state != STATE.SLEEPING)
            {
                state = STATE.CHASING;
                player = other.transform;
            }
        }

        // 2. Rilevamento FUMO
        if (other.CompareTag("Fumogeno"))
        {
            state = STATE.SLEEPING;
            agent.ResetPath();
        }

        // 3. Rilevamento ITEM (Distrazione)
        if (other.CompareTag("Item"))
        {
            // Viene distratto solo se non sta inseguendo e non sta dormendo
            if (state == STATE.VIGILE)
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
        // Se il player scappa
        if (other.CompareTag("Player") && state == STATE.CHASING)
        {
            state = STATE.VIGILE;
            agent.ResetPath();
        }

        // Se il fumo sparisce o il nemico viene spostato fuori
        if (other.CompareTag("Fumogeno"))
        {
            // Si sveglia solo se stava dormendo
            if (state == STATE.SLEEPING)
            {
                Debug.Log("Fumo sparito, nemico sveglio!");
                state = STATE.VIGILE;

                // Resetta timer pattuglia per farlo muovere subito o aspettare un po'
                patrolTimer = 0;

                // Qui fermeresti l'animazione del sonno
                // animator.SetBool("IsSleeping", false);
            }
        }
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