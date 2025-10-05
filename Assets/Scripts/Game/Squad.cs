using UnityEngine;
using System.Collections.Generic;

public class Squad : MonoBehaviour
{
    public List<Transform> formationPoints = new();

    public List<Soldier> soldiers = new();

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
        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] != null && soldiers[i].gameObject.activeInHierarchy)
            {
                Vector3 offset = formationPoints[i].position - transform.position;
                soldiers[i].MoveTo(destination + offset);
            }
        }
    }

    public void Attack(Transform target)
    {
        foreach (var soldier in soldiers)
        {
            if (soldier != null && soldier.gameObject.activeInHierarchy)
            {
                soldier.SetTarget(target);
            }
        }
    }
}