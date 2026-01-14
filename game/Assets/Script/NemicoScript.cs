using UnityEngine;
using UnityEngine.AI;

// Manteniamo il tuo enum originale per gli stati interni della guardia
public enum STATE { VIGILE, CHASING, SLEEPING, OFF, DISTRACTED }

public class NemicoScript : MonoBehaviour
{
    [Header("Componenti")]
    private NavMeshAgent agent;
    public Transform player;

    [Header("Stato Interno")]
    public STATE state = STATE.VIGILE;

    // Riferimento al collider specifico che ci sta facendo dormire
    private Collider currentSmokeCollider;

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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        patrolTimer = patrolWaitTime;
        if (agent != null) agent.stoppingDistance = 0.5f;
    }

    // --- NUOVA PARTE: INTEGRAZIONE CON GAMEMANAGER ---

    private void OnEnable()
    {
        // Ci iscriviamo all'evento: Se il GameManager cambia stato, avvisami!
        if (GameManager.Instance != null)
        {
            GameManager.OnStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDisable()
    {
        // Ci disiscriviamo per evitare errori quando l'oggetto viene distrutto
        if (GameManager.Instance != null)
        {
            GameManager.OnStateChanged -= HandleGameStateChanged;
        }
    }

    // Questa funzione viene chiamata automaticamente dal GameManager
    private void HandleGameStateChanged(GameState newGlobalState)
    {
        // Se il gioco torna in modalità VISITATORE (es. reset o debug)
        if (newGlobalState == GameState.Visitor)
        {
            // Se stavo inseguendo, smetto subito e torno a pattugliare
            if (state == STATE.CHASING || state == STATE.DISTRACTED)
            {
                Debug.Log("Torno in modalità pacifica (Visitatore).");
                state = STATE.VIGILE;
                agent.ResetPath();
                player = null; // Dimentico il player
            }
        }
        
        // Se il gioco passa a LADRO
        else if (newGlobalState == GameState.Thief)
        {
            // Opzionale: Se vuoi che le guardie diventino subito aggressive se vedono il player
            // Potresti forzare un controllo qui, ma OnTriggerStay lo farà al prossimo frame.
            Debug.Log("Allerta massima! Cerco ladri.");
        }
    }

    // --------------------------------------------------

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
                if (agent.isActiveAndEnabled && !agent.isStopped) agent.isStopped = true;
                break;
        }
    }

    // --- LOGICHE DI COMPORTAMENTO (Invariate) ---

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

    void ChaseLogic()
    {
        // SICUREZZA EXTRA: Se per caso siamo tornati Visitor mentre inseguivo
        if (GameManager.Instance.CurrentState == GameState.Visitor)
        {
            state = STATE.VIGILE;
            return;
        }

        if (player != null)
        {
            if (agent.isActiveAndEnabled) agent.isStopped = false;
            
            Vector3 directionFromPlayer = (transform.position - player.position).normalized;
            Vector3 safeDestination = player.position + (directionFromPlayer * stopDistanceFromPlayer);
            
            if(agent.isActiveAndEnabled) agent.SetDestination(safeDestination);

            if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                LookAtTarget(player.position);
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
            Debug.Log("Il fumo si è diradato, mi sveglio!");
            WakeUp();
        }
    }

    public void ForzaSonno(GameObject oggettoFumo)
    {
        Collider col = oggettoFumo.GetComponent<Collider>();
        if (col != null)
        {
            currentSmokeCollider = col;
            EntraInStatoSonno();
        }
    }

    void EntraInStatoSonno()
    {
        if (state != STATE.SLEEPING)
        {
            Debug.Log("Zzz... addormentato dal fumo.");
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
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
        {
            return navHit.position;
        }
        return origin;
    }

    // --- GESTIONE COLLISIONI (AGGIORNATA CON GAMEMANAGER) ---

    void OnTriggerStay(Collider other)
    {
        // Se sto dormendo, ignoro tutto finché non mi sveglio
        if (state == STATE.SLEEPING) return;

        // Gestione PLAYER
        if (other.CompareTag("Player"))
        {
            // MODIFICA: Reagisco al player SOLO se siamo nella fase THIEF (Ladro)
            if (GameManager.Instance.CurrentState == GameState.Thief)
            {
                if (state == STATE.VIGILE || state == STATE.DISTRACTED)
                {
                    Debug.Log("Ti ho visto! AL LADRO!");
                    state = STATE.CHASING;
                    player = other.transform;
                }
            }
            else
            {
                // Se sono in fase VISITOR, ignoro il player (o potrei salutarlo)
                // Debug.Log("Buongiorno visitatore, buona permanenza.");
            }
        }

        // Gestione FUMO (Funziona sempre, anche se sono visitatore, il fumo mi addormenta)
        if (other.CompareTag("Fumogeno"))
        {
            if (other.enabled && other.isTrigger)
            {
                currentSmokeCollider = other;
                EntraInStatoSonno();
            }
        }

        // Gestione ITEM (Distrazione)
        if (other.CompareTag("Item"))
        {
            // MODIFICA: Mi distraggo solo se sono in fase THIEF (o se decidi che anche da visitatore ti distrai)
            // Di solito in fase Visitatore il player non può lanciare item.
            if (GameManager.Instance.CurrentState == GameState.Thief)
            {
                if (state == STATE.VIGILE || state == STATE.CHASING)
                {
                    state = STATE.DISTRACTED;
                    distractionPoint = other.transform.position;
                    distractionTimer = 0;
                    if(agent.isActiveAndEnabled) agent.ResetPath();
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && state == STATE.CHASING)
        {
            // Se il player scappa, torno vigile (o potrei andare all'ultima posizione nota)
            state = STATE.VIGILE;
            if(agent.isActiveAndEnabled) agent.ResetPath();
            player = null;
        }

        if (other.CompareTag("Fumogeno"))
        {
            if (currentSmokeCollider == other)
            {
                WakeUp();
            }
        }
    }
}