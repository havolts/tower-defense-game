using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FriendlyUnit : MonoBehaviour
{
    public UnitStats unitStats;
    private NavMeshAgent agent;

    private List<Order> orders = new List<Order>();
    private Order currentOrder;

    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0f;

        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    void Update()
    {
        if (currentOrder == null && orders.Count > 0)
        {
            currentOrder = orders[0];
            orders.RemoveAt(0);
        }

        // Change color based on having an order
        if (rend != null)
            rend.material.color = (currentOrder != null) ? Color.green : originalColor;

        if (currentOrder != null)
            ExecuteOrder(currentOrder);
    }
    private float lastAttackTime;

    void ExecuteOrder(Order order)
    {
        switch (order.orderType)
        {
            case OrderType.Move:
                MoveTo(order.targetPosition);
                if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(0.1f, agent.stoppingDistance))
                    currentOrder = null;
                break;

            case OrderType.Attack:
                if (order.targetTransform == null)
                {
                    currentOrder = null;
                    return;
                }

                Transform target = order.targetTransform;
                MoveTo(target.position);

                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= unitStats.attackRange)
                {
                    RotateTowards(target.position);

                    if (Time.time >= lastAttackTime + unitStats.attackCooldown)
                    {
                        Health health = target.GetComponent<Health>();
                        if (health != null)
                            health.TakeDamage(unitStats.attackDamage);

                        lastAttackTime = Time.time;
                    }
                }

                // If target died or destroyed
                if (target == null)
                    currentOrder = null;

                break;
        }
    }

    public void AddOrder(Order order, bool append)
    {
        if (!append)
        {
            orders.Clear();
            currentOrder = null;
        }

        orders.Add(order);

        // Log current orders
        string orderList = "";
        foreach (var o in orders)
        {
            if (o.orderType == OrderType.Move)
                orderList += $"Move to {o.targetPosition}\n";
            else if (o.orderType == OrderType.Attack)
                orderList += $"Attack {o.targetTransform?.name}\n";
        }
        Debug.Log(orderList);
    }

    void MoveTo(Vector3 position)
    {
        Vector3 destination = position;

        // Only offset if the target is farther than the attack range
        float distance = Vector3.Distance(transform.position, position);
        if (distance > unitStats.attackRange)
            destination = position - (position - transform.position).normalized * unitStats.attackRange;

        agent.SetDestination(destination);
        RotateTowards(position);
    }


    void RotateTowards(Vector3 position)
    {
        Vector3 dir = (position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = Vector3.Slerp(transform.forward, dir, Time.deltaTime * 10f);
    }
}

public enum OrderType { Move, Attack }

public class Order
{
    public OrderType orderType;
    public Vector3 targetPosition;
    public Transform targetTransform;

    public Order(OrderType type, Vector3 position)
    {
        orderType = type;
        targetPosition = position;
    }

    public Order(OrderType type, Transform target)
    {
        orderType = type;
        targetTransform = target;
    }
}
