using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class Squad : MonoBehaviour
{
    public List<Transform> formationPoints = new();
    public List<Soldier> soldiers = new();

    public float formationThreshold = 0.2f;

    private NavMeshAgent squadAgent;

    void Awake()
    {
        squadAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        MaintainFormation();
    }

    private void MaintainFormation()
    {
        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] != null && soldiers[i].gameObject.activeInHierarchy)
            {
                Vector3 targetPosition = formationPoints[i].position;
                float distanceToTarget = Vector3.Distance(soldiers[i].transform.position, targetPosition);

                if (distanceToTarget > formationThreshold)
                {
                    soldiers[i].MoveTo(targetPosition);
                }
            }
        }
    }

    public void AddSoldier(Soldier soldier)
    {
        if (!soldiers.Contains(soldier))
        {
            soldiers.Add(soldier);
        }
    }

    public void RemoveSoldier(Soldier soldier)
    {
        if (soldiers.Contains(soldier))
        {
            soldiers.Remove(soldier);
        }
    }

    public void MoveTo(Vector3 destination)
    {
        squadAgent.SetDestination(destination);
    }

    public void Attack(Transform target)
    {
        squadAgent.ResetPath();
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.gameObject.activeInHierarchy)
            {
                soldier.SetTarget(target);
            }
        }
    }
}