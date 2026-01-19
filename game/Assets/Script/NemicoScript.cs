using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public enum STATE { VIGILE, CHASING, SEARCHING, SLEEPING, OFF, DISTRACTED }

public class NemicoScript : MonoBehaviour
{
    [Header("Componenti")]
    private NavMeshAgent agent;
    public Transform player;
    public FieldOfView fov;

    [Header("Grafica UI (Balloon)")]
    public EnemyStatusVisuals statusVisuals; // <--- TRASCINA QUI IL CANVAS FIGLIO
    private STATE lastState; // Serve per capire quando lo stato cambia

    [Header("Stato Interno")]
    public STATE state = STATE.VIGILE;
    private Collider currentSmokeCollider;

    [Header("Impostazioni Pattuglia")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer;

    [Header("Impostazioni Inseguimento Avanzato")]
    public float stopDistanceFromPlayer = 2.0f;
    public float searchTime = 4f;
    private float searchTimer;

    // Logica "Last Known Position"
    private Vector3 lastKnownPosition;
    private bool hasLastKnownPosition = false;

    [Header("Impostazioni Distrazione")]
    public float distractionTime = 5f;
    private float distractionTimer;
    private Vector3 distractionPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Cerca lo script FieldOfView
        if (fov == null) fov = GetComponentInChildren<FieldOfView>();
        if (fov == null) fov = GetComponent<FieldOfView>();

        // Cerca lo script EnemyStatusVisuals se non assegnato
        if (statusVisuals == null) statusVisuals = GetComponentInChildren<EnemyStatusVisuals>();
    }

    void Start()
    {
        patrolTimer = patrolWaitTime;
        if (agent != null) agent.stoppingDistance = 0.5f;

        // Inizializza l'icona corretta allo start
        if (statusVisuals != null) statusVisuals.UpdateStatus(state);
        lastState = state;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.OnStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.OnStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState newGlobalState)
    {
        if (newGlobalState == GameState.Visitor)
        {
            if (state == STATE.CHASING || state == STATE.SEARCHING || state == STATE.DISTRACTED)
            {
                state = STATE.VIGILE;
                if (agent.isActiveAndEnabled) agent.ResetPath();
                player = null;
                hasLastKnownPosition = false;
            }
        }
    }

    void Update()
    {
        // --- GESTIONE CAMBIO ICONE (NUOVO) ---
        // Se lo stato è cambiato rispetto al frame precedente, aggiorno la grafica
        if (state != lastState)
        {
            if (statusVisuals != null)
            {
                statusVisuals.UpdateStatus(state);
            }
            lastState = state; // Ricordo il nuovo stato
        }
        // -------------------------------------

        CheckVision();

        switch (state)
        {
            case STATE.VIGILE:
                PatrolLogic();
                break;

            case STATE.CHASING:
                ChaseLogic();
                break;

            case STATE.SEARCHING:
                SearchLogic();
                break;

            case STATE.DISTRACTED:
                DistractedLogic();
                break;

            case STATE.SLEEPING:
                SleepLogic();
                break;

            case STATE.OFF:
                if (agent.isActiveAndEnabled && !agent.isStopped) agent.isStopped = true;
                break;
        }
    }

    // --- LOGICA DI VISIONE ---
    void CheckVision()
    {
        if (state == STATE.SLEEPING || state == STATE.OFF) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Visitor) return;

        if (fov != null && fov.visibleTargets.Count > 0)
        {
            Transform targetVisto = fov.visibleTargets[0];
            lastKnownPosition = targetVisto.position;
            hasLastKnownPosition = true;

            if (state != STATE.CHASING && state != STATE.SLEEPING)
            {
                state = STATE.CHASING;
                player = targetVisto;
            }
        }
    }

    // --- LOGICHE DI COMPORTAMENTO ---

    void ChaseLogic()
    {
        if (!agent.isActiveAndEnabled) return;
        agent.isStopped = false;

        if (fov.visibleTargets.Count > 0 && player != null)
        {
            agent.SetDestination(player.position);
            if (agent.remainingDistance <= agent.stoppingDistance + 1f) LookAtTarget(player.position);
        }
        else
        {
            if (hasLastKnownPosition)
            {
                agent.SetDestination(lastKnownPosition);
                if (!agent.pathPending && agent.remainingDistance <= 1.5f)
                {
                    state = STATE.SEARCHING;
                    searchTimer = 0;
                }
            }
            else
            {
                state = STATE.VIGILE;
            }
        }
    }

    void SearchLogic()
    {
        if (!agent.isActiveAndEnabled) return;
        agent.isStopped = true;

        searchTimer += Time.deltaTime;
        float rotationAmount = Mathf.Sin(Time.time * 2) * 60f;
        transform.Rotate(Vector3.up * rotationAmount * Time.deltaTime);

        if (searchTimer >= searchTime)
        {
            hasLastKnownPosition = false;
            state = STATE.VIGILE;
            agent.isStopped = false;
        }
    }

    void PatrolLogic()
    {
        if (!agent.isActiveAndEnabled) return;
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

    void DistractedLogic()
    {
        if (!agent.isActiveAndEnabled) return;
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
        if (agent.isActiveAndEnabled && !agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        if (currentSmokeCollider == null || !currentSmokeCollider.enabled || !currentSmokeCollider.gameObject.activeInHierarchy)
        {
            WakeUp();
        }
    }

    // --- UTILS ---

    public void ForzaSonno(GameObject oggettoFumo)
    {
        Collider col = oggettoFumo.GetComponent<Collider>();
        if (col != null) { currentSmokeCollider = col; EntraInStatoSonno(); }
    }

    void EntraInStatoSonno()
    {
        if (state != STATE.SLEEPING)
        {
            state = STATE.SLEEPING;
            if (agent.isActiveAndEnabled) agent.ResetPath();
        }
    }

    void WakeUp()
    {
        state = STATE.VIGILE;
        patrolTimer = 0;
        currentSmokeCollider = null;
    }

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
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask)) return navHit.position;
        return origin;
    }

    // --- TRIGGER ---

    void OnTriggerStay(Collider other)
    {
        if (state == STATE.SLEEPING) return;

        if (other.CompareTag("Fumogeno"))
        {
            if (other.enabled && other.isTrigger) { currentSmokeCollider = other; EntraInStatoSonno(); }
        }

        if (other.CompareTag("Item"))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Thief)
            {
                if (state == STATE.VIGILE || state == STATE.CHASING || state == STATE.SEARCHING)
                {
                    state = STATE.DISTRACTED;
                    distractionPoint = other.transform.position;
                    distractionTimer = 0;
                    if (agent.isActiveAndEnabled) agent.ResetPath();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Fumogeno") && currentSmokeCollider == other) WakeUp();
    }
}