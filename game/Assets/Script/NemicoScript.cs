using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // FONDAMENTALE per ricaricare la scena
using System.Collections.Generic;

public enum STATE { VIGILE, CHASING, SEARCHING, SLEEPING, OFF, DISTRACTED }

public class NemicoScript : MonoBehaviour
{
    [Header("Componenti")]
    private NavMeshAgent agent;
    public Transform player; // Assicurati che questo venga riempito dai sensori
    public FieldOfView fov;
    public EnemyStatusVisuals statusVisuals;

    [Header("Stato")]
    public STATE state = STATE.VIGILE;
    private STATE lastState;
    private Collider currentSmokeCollider;

    [Header("Pattuglia")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer;

    [Header("Inseguimento & Cattura")]
    public float stopDistanceFromPlayer = 2.0f;
    public float arrestDistance = 1.3f; // Distanza di cattura
    public float searchTime = 4f;
    private float searchTimer;

    [Tooltip("Predizione movimento")]
    public float predictionTime = 2.0f;
    private Vector3 previousPlayerPos;
    private Vector3 playerVelocity;

    // Logica "Ultima posizione nota"
    private Vector3 lastKnownPosition;
    private bool hasLastKnownPosition = false;

    [Header("Sensi")]
    public float hearingRadius = 2.5f;

    [Header("Distrazione")]
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
            if (statusVisuals != null && state != STATE.CHASING)
                statusVisuals.UpdateStatus(state);
            lastState = state;
        }

        CalculateTargetVelocity();
        CheckSensors();

        switch (state)
        {
            case STATE.VIGILE: PatrolLogic(); break;
            case STATE.CHASING: ChaseLogic(); break;
            case STATE.SEARCHING: SearchLogic(); break;
            case STATE.DISTRACTED: DistractedLogic(); break;
            case STATE.SLEEPING: SleepLogic(); break;
            case STATE.OFF: if (agent.isActiveAndEnabled) agent.isStopped = true; break;
        }
    }

    // --- LOGICA CATTURA ---
    private void ArrestaGiocatore()
    {
        // Controlla se siamo in modalità ladro
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Thief)
        {
            Debug.Log("GIOCATORE ARRESTATO!");
            if (MaestroManager.Instance != null)
            {
                MaestroManager.Instance.goToBackPhase();
            }
            
           

            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            int indexUltimaScena = SceneManager.sceneCountInBuildSettings - 1;
            
            GameManager.lastScena = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(indexUltimaScena);

            // Disabilita lo script per evitare chiamate multiple mentre carica
            this.enabled = false;
        }
    }

    // Fallback collisione fisica (se il player sbatte contro il nemico)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ArrestaGiocatore();
        }
    }
    // -----------------------

    void CalculateTargetVelocity()
    {
        if (player != null)
        {
            Vector3 currentMove = (player.position - previousPlayerPos) / Time.deltaTime;
            playerVelocity = Vector3.Lerp(playerVelocity, currentMove, Time.deltaTime * 5f);
            previousPlayerPos = player.position;
        }
    }

    void CheckSensors()
    {
        if (state == STATE.SLEEPING || state == STATE.OFF || state == STATE.DISTRACTED) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Visitor) return;

        bool foundTarget = false;
        bool detectedBySound = false;
        Transform targetTransform = null;

        // 1. Vista
        if (fov != null && fov.visibleTargets.Count > 0)
        {
            targetTransform = fov.visibleTargets[0];
            foundTarget = true;
            detectedBySound = false;
        }

        // 2. Udito
        if (!foundTarget && fov != null)
        {
            Collider[] hearers = Physics.OverlapSphere(transform.position, hearingRadius, fov.targetMask);
            if (hearers.Length > 0)
            {
                Vector3 dirToTarget = (hearers[0].transform.position - transform.position).normalized;
                float dstToTarget = Vector3.Distance(transform.position, hearers[0].transform.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, fov.obstacleMask))
                {
                    targetTransform = hearers[0].transform;
                    foundTarget = true;
                    detectedBySound = true;
                    LookAtTarget(targetTransform.position);
                }
            }
        }

        if (foundTarget)
        {
            if (player != targetTransform) previousPlayerPos = targetTransform.position;
            player = targetTransform;

            if (state != STATE.CHASING)
            {
                state = STATE.CHASING;
                if (statusVisuals != null) statusVisuals.TriggerDetection(detectedBySound);
            }

            lastKnownPosition = player.position;
            hasLastKnownPosition = true;
        }
        else
        {
            if (state == STATE.CHASING && player != null)
            {
                Vector3 predictedPos = lastKnownPosition + (playerVelocity * predictionTime);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(predictedPos, out hit, 5.0f, NavMesh.AllAreas))
                {
                    lastKnownPosition = hit.position;
                }
                player = null;
            }
        }
    }

    void ChaseLogic()
    {
        if (!agent.isActiveAndEnabled) return;
        agent.isStopped = false;

        if (player != null)
        {
            agent.SetDestination(player.position);

            // --- CONTROLLO DISTANZA DI CATTURA ---
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= arrestDistance)
            {
                ArrestaGiocatore();
            }
            // -------------------------------------

            if (agent.remainingDistance <= agent.stoppingDistance + 1.5f) LookAtTarget(player.position);
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
                    agent.updateRotation = false;
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

        float rotationAmount = Mathf.Sin(Time.time * 3) * 120f;
        transform.Rotate(Vector3.up * rotationAmount * Time.deltaTime);

        if (searchTimer >= searchTime)
        {
            hasLastKnownPosition = false;
            state = STATE.VIGILE;
            agent.isStopped = false;
            agent.updateRotation = true;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, arrestDistance);
    }

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