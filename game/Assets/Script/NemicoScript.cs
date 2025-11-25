using UnityEngine;
using UnityEngine.AI;

public class NemicoScript : MonoBehaviour

{
    private NavMeshAgent agent;
    public Transform player;
    public bool isChasing = false;
    private bool inTrigger;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = true;
            player = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            agent.ResetPath();
        }
    }

    void Update()
    {
        if (isChasing && player != null)
        {
            agent.SetDestination(player.position);
        }
    }
}
