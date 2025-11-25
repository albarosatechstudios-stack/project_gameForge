using UnityEngine;
using UnityEngine.AI;

public enum STATE { VIGILE,SLEEPING,OFF }

public class NemicoScript : MonoBehaviour

{
    private NavMeshAgent agent;
    public Transform player;
    public bool isChasing = false;
    private bool inTrigger;
    public STATE state = STATE.VIGILE  ;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && state == STATE.VIGILE)
        {
            isChasing = true;
            player = other.transform;
        }
        if(other.CompareTag("nemico") &&  state == STATE.VIGILE)
        {

        }
    }

    void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    isChasing = false;
        //    agent.ResetPath();
        //}
    }

    void Update()
    {
        if (isChasing && player != null && state==STATE.VIGILE)
        {
            agent.SetDestination(player.position);
        }
    }
}
