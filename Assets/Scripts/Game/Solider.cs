using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(UnitAnimation))]
public class Soldier : Unit
{
    private NavMeshAgent agent;
    private UnitAnimation unitAnimation;
    private Transform currentTarget;

    private float nextAttackTime = 0f;

    private enum SoldierState { Idle, Moving, Attacking, Dead }
    private SoldierState currentState = SoldierState.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        unitAnimation = GetComponent<UnitAnimation>();
    }

    void Update()
    {
        switch (currentState)
        {
            case SoldierState.Idle:
                unitAnimation.PlayIdle();
                break;
            case SoldierState.Moving:
                if (agent.velocity.magnitude > 0.1f)
                {
                    unitAnimation.PlayMove();
                }
                else
                {
                    currentState = SoldierState.Idle;
                }
                break;
            case SoldierState.Attacking:
                HandleAttacking();
                break;
        }
    }

    private void HandleAttacking()
    {
        if (currentTarget == null)
        {
            currentState = SoldierState.Idle;
            return;
        }

        // Aim at the target
        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        // Check if we are moving while attacking
        if (agent.velocity.magnitude > 0.1f)
        {
            if (distanceToTarget > UnitData.attackRange)
            {
                // Chase the target if it's out of range
                agent.SetDestination(currentTarget.position);
            }

            // Determine movement direction relative to the target
            Vector3 moveDirection = agent.velocity.normalized;
            float forwardDot = Vector3.Dot(transform.forward, moveDirection);
            float rightDot = Vector3.Dot(transform.right, moveDirection);

            if (forwardDot > 0.7f)
            {
                unitAnimation.PlayMoveAttackForward();
            }
            else if (forwardDot < -0.7f)
            {
                unitAnimation.PlayMoveAttackBackward();
            }
            else if (rightDot > 0.7f)
            {
                unitAnimation.PlayMoveAttackRight();
            }
            else if (rightDot < -0.7f)
            {
                unitAnimation.PlayMoveAttackLeft();
            }
            else
            {
                // If not moving clearly in one of the 4 directions, just play the forward animation
                unitAnimation.PlayMoveAttackForward();
            }
        }
        else // Not moving, so we are either aiming or shooting
        {
            agent.ResetPath(); // Stop the agent from moving

            if (distanceToTarget <= UnitData.attackRange)
            {
                unitAnimation.PlayAttackIdle();

                if (Time.time >= nextAttackTime)
                {
                    Fire();
                    nextAttackTime = Time.time + 1f / UnitData.attackRate;
                }
            }
            else
            {
                // Target is out of range, go back to moving
                agent.SetDestination(currentTarget.position);
            }
        }
    }

    private void Fire()
    {
        unitAnimation.PlayAttackShot();

        // Instantiate a projectile, etc
        Debug.Log(gameObject.name + " fires at " + currentTarget.name);
    }

    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
        currentState = SoldierState.Moving;
        currentTarget = null;
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
        currentState = SoldierState.Attacking;
    }

    public void Die()
    {
        currentState = SoldierState.Dead;
        unitAnimation.PlayDeath();

        Destroy(gameObject, 3f); // Example delay
    }
}