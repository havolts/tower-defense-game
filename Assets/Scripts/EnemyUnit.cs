using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnit : MonoBehaviour
{
    public UnitStats unitStats;
    private NavMeshAgent agent;
    private Order currentOrder;
    private CombatStance stance = CombatStance.Aggressive;
    private float lastAttackTime;

    private Renderer rend;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0f;

        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        Transform closest = this.GetComponent<Vision>().GetClosestTarget();
        if (closest != null)
        {
            currentOrder = new Order(this.GetComponent<Vision>().GetClosestTarget(), false);
            rend.material.color = Color.red; // Self-given order
        }
        else
        {
            currentOrder = new Order(new Vector3(0,0,0), false); // Temporary fix, we need to get the fortress in start as we cannot set it to a variable for a prefab.
        }
        ExecuteOrder(currentOrder);
    }

    void ExecuteOrder(Order order)
    {
        switch (order.orderType)
        {
            case OrderType.Move:
                HandleMove(order);
                break;

            case OrderType.Attack:
                HandleAttack(order);
                break;
        }
    }

    // I split these out for sake of readability
    void HandleMove(Order order)
    {
        MoveTo(order.targetPosition);

        // Basically checks if the unit has reached it's destination - if so it completes order.
        // Currently slightly problematic as the unit never goes to exact position, and does not take into account other objects at position.
        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(0.1f, agent.stoppingDistance)) currentOrder = null;
    }

    void HandleAttack(Order order)
    {
        if (order.targetTransform == null)
        {
            currentOrder = null;
            return;
        }

        Transform target = order.targetTransform;
        float distance = Vector3.Distance(transform.position, target.position);

        // Move only if stance is aggressive/player order and target is out of range
        if ((stance == CombatStance.Aggressive || order.isPlayerOrder) && distance > unitStats.attackRange)
            MoveTo(target.position);

        // Attacks only if in range
        if (!(stance == CombatStance.Passive))
        {
            if (distance <= unitStats.attackRange && Time.time >= lastAttackTime + unitStats.attackCooldown)
            {
                Health health = target.GetComponent<Health>();
                if (health != null)
                    health.TakeDamage(unitStats.attackDamage);

                lastAttackTime = Time.time;
            }
        }
    }

    void MoveTo(Vector3 position)
    {
        Vector3 destination = position;
        float distance = Vector3.Distance(transform.position, position);

        if (distance > unitStats.attackRange)
        {
            Vector3 directionToTarget = (position - transform.position).normalized;
            Vector3 offset = directionToTarget * unitStats.attackRange;
            destination = position - offset;

        }
        agent.SetDestination(destination);
        RotateTowards(position);
    }

    void RotateTowards(Vector3 position)
    {
        Vector3 directionToTarget = (position - transform.position).normalized;
        directionToTarget.y = 0;
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            transform.forward = Vector3.Slerp(transform.forward, directionToTarget, Time.deltaTime * 10f);
        }
    }
}
