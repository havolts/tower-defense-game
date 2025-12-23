using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FriendlyUnit : MonoBehaviour
{
    public UnitStats unitStats;
    private NavMeshAgent agent;
    public GameObject fireboltPrefab; // or assign in inspector

    public List<Order> orders = new List<Order>();
    private Order currentOrder;

    private CombatStance stance = CombatStance.Aggressive;

    public float health, maxHealth;
    public float influenceValue, influenceRange; // influenceRange is in world units (metres).

    private float lastAttackTime;

    private Renderer rend;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0f;

        maxHealth = GetComponent<Health>().stats.maxHealth;
        rend = GetComponent<Renderer>();

        influenceValue = 5.0f;
        influenceRange = unitStats.attackRange*2;
    }

    void Update()
    {
        health = GetComponent<Health>().currentHealth;
        if (currentOrder == null)
        {
            if (orders.Count > 0) // There are player-given orders
            {
                currentOrder = orders[0];
                orders.RemoveAt(0);
                rend.material.color = Color.blue; // Player-given order
            }
            else // There are no player-given orders
            {
                Transform closest = this.GetComponent<Vision>().GetClosestTarget();
                if(closest != null)
                {
                    currentOrder = new Order(closest, false);
                    rend.material.color = Color.green; // Self-given order
                }
            }
        }
        if(currentOrder != null) ExecuteOrder(currentOrder);
    }

    void CastFirebolt(Transform target)
    {
        GameObject bolt = Instantiate(fireboltPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        Firebolt firebolt = bolt.GetComponent<Firebolt>();
        firebolt.Launch(target, unitStats.attackDamage);
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
                CastFirebolt(target);
                lastAttackTime = Time.time;
            }

        }
    }

    // Adds order to the list of orders
    public void AddOrder(Order order, bool append)
    {
        if (!append)
        {
            orders.Clear();
            currentOrder = null;
        }

        orders.Add(order);
        //Removed previous debug.log which looped over list - poor efficiency
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

public enum CombatStance { Passive, Defensive, Aggressive }
public enum OrderType { Move, Attack }

public class Order
{
    public OrderType orderType;

    public Vector3 targetPosition;
    public Transform targetTransform;

    public bool isPlayerOrder;

    public Order(Vector3 position, bool isplayerorder)
    {
        orderType = OrderType.Move;
        targetPosition = position;
        isPlayerOrder = isplayerorder;
    }

    public Order(Transform target, bool isplayerorder)
    {
        orderType = OrderType.Attack;
        targetTransform = target;
        isPlayerOrder = isplayerorder;
    }
}
