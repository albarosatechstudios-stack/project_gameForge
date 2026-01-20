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

    [Header("Grafica UI")]
    public EnemyStatusVisuals statusVisuals;
    private STATE lastState;

    [Header("Stato Interno")]
    public STATE state = STATE.VIGILE;
    private Collider currentSmokeCollider;

    [Header("Impostazioni Pattuglia")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer;

    [Header("Impostazioni Inseguimento & Predizione")]
    public float stopDistanceFromPlayer = 2.0f;
    public float searchTime = 4f;
    private float searchTimer;

    [Tooltip("Tempo in secondi per predire il movimento (Dead Reckoning)")]
    public float predictionTime = 2.0f;
    private Vector3 previousPlayerPos;
    private Vector3 playerVelocity;

    // Logica "Last Known Position"
    private Vector3 lastKnownPosition;
    private bool hasLastKnownPosition = false;

    [Header("Impostazioni Sensi (Udito/Prossimità)")]
    [Tooltip("Distanza entro la quale il nemico ti sente anche se sei alle spalle")]
    public float hearingRadius = 2.5f; // <--- NUOVO: Raggio udito

    [Header("Impostazioni Distrazione")]
    public float distractionTime = 5f;
    private float distractionTimer;
    private Vector3 distractionPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (fov == null) fov = GetComponentInChildren<FieldOfView>();
        if (fov == null) fov = GetComponent<FieldOfView>();

        if (statusVisuals == null) statusVisuals = GetComponentInChildren<EnemyStatusVisuals>();
    }

    void Start()
    {
        patrolTimer = patrolWaitTime;
        if (agent != null) agent.stoppingDistance = 0.5f;

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
        if (state != lastState)
        {
            if (statusVisuals != null) statusVisuals.UpdateStatus(state);
            lastState = state;
        }

        CalculateTargetVelocity();
        CheckSensors(); // <--- Nome cambiato da CheckVision a CheckSensors

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

    void CalculateTargetVelocity()
    {
        if (player != null)
        {
            Vector3 currentMove = (player.position - previousPlayerPos) / Time.deltaTime;
            playerVelocity = Vector3.Lerp(playerVelocity, currentMove, Time.deltaTime * 5f);
            previousPlayerPos = player.position;
        }
    }

    // --- NUOVA GESTIONE SENSORI (VISTA + UDITO) ---
    void CheckSensors()
    {
        if (state == STATE.SLEEPING || state == STATE.OFF) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Visitor) return;

        bool foundTarget = false;
        Transform targetTransform = null;

        // 1. CONTROLLO VISIVO (CONO)
        if (fov != null && fov.visibleTargets.Count > 0)
        {
            targetTransform = fov.visibleTargets[0];
            foundTarget = true;
        }

        // 2. CONTROLLO UDITIVO (SFERA DI PROSSIMITÀ)
        // Se non l'ho visto, controllo se è abbastanza vicino da "sentirlo" (anche alle spalle)
        if (!foundTarget && fov != null)
        {
            Collider[] hearers = Physics.OverlapSphere(transform.position, hearingRadius, fov.targetMask);
            if (hearers.Length > 0)
            {
                // Controllo che non ci siano muri tra me e il rumore (opzionale, ma realistico)
                // Se vuoi che ti senta anche attraverso un muro sottile, togli questo if del Raycast
                Vector3 dirToTarget = (hearers[0].transform.position - transform.position).normalized;
                float dstToTarget = Vector3.Distance(transform.position, hearers[0].transform.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, fov.obstacleMask))
                {
                    targetTransform = hearers[0].transform;
                    foundTarget = true;
                }
            }
        }

        // --- GESTIONE LOGICA TROVATO / PERSO ---
        if (foundTarget)
        {
            if (player != targetTransform) previousPlayerPos = targetTransform.position;

            player = targetTransform;
            state = STATE.CHASING;
            lastKnownPosition = player.position;
            hasLastKnownPosition = true;
        }
        else
        {
            // Se stavo inseguendo e l'ho perso (niente vista, niente udito)
            if (state == STATE.CHASING && player != null)
            {
                // Predizione movimento (Dead Reckoning)
                Vector3 predictedPos = lastKnownPosition + (playerVelocity * predictionTime);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(predictedPos, out hit, 5.0f, NavMesh.AllAreas))
                {
                    lastKnownPosition = hit.position;
                }
                player = null; // Smetto di "lockare" il player, vado in predizione
            }
        }
    }

    // --- LOGICHE DI COMPORTAMENTO ---

    void ChaseLogic()
    {
        if (!agent.isActiveAndEnabled) return;
        agent.isStopped = false;

        if (player != null)
        {
            // Inseguimento diretto
            agent.SetDestination(player.position);
            // Se sono molto vicino, mi giro verso di lui aggressivamente
            if (agent.remainingDistance <= agent.stoppingDistance + 1.5f) LookAtTarget(player.position);
        }
        else
        {
            // Inseguimento predittivo (Target perso)
            if (hasLastKnownPosition)
            {
                agent.SetDestination(lastKnownPosition);

                // Se arrivo alla posizione predetta
                if (!agent.pathPending && agent.remainingDistance <= 1.5f)
                {
                    state = STATE.SEARCHING;
                    searchTimer = 0;

                    // Appena arrivo dove ti ho perso, mi giro verso dove stavi andando o indietro
                    // (Opzionale: piccolo trick per renderlo più vivo)
                    agent.updateRotation = false; // Disabilito rotazione automatica per ruotare a mano in Search
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

        // Ruota destra/sinistra + controllo spalle
        // Una rotazione più ampia per coprire le spalle
        float rotationAmount = Mathf.Sin(Time.time * 3) * 120f;
        transform.Rotate(Vector3.up * rotationAmount * Time.deltaTime);

        if (searchTimer >= searchTime)
        {
            hasLastKnownPosition = false;
            state = STATE.VIGILE;
            agent.isStopped = false;
            agent.updateRotation = true; // Riabilito la rotazione automatica del NavMesh
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
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
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

    // Disegna il raggio udito nell'editor per debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
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