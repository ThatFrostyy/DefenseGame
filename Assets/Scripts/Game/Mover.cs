using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Mover : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(float moveSpeed)
    {
        agent.speed = moveSpeed;
    }

    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
    }
}
