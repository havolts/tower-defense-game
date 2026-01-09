using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FriendlyUnit : MonoBehaviour
{
    public UnitStats unitStats;
    private NavMeshAgent agent;
    private Animator animator;
    public GameObject fireboltPrefab; // or assign in inspector

    public List<Order> orders = new List<Order>();
    private Order currentOrder;

    private CombatStance stance = CombatStance.Aggressive;

    Health healthComponent;
    public float health, maxHealth;
    public float influenceValue, influenceRange; // influenceRange is in world units (metres).

    private float lastAttackTime;

    [HideInInspector] public bool frozen = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0f;
        agent.speed = unitStats.movementSpeed;

        animator = GetComponentInChildren<Animator>();

        healthComponent = GetComponent<Health>();
        maxHealth = healthComponent.stats.maxHealth;

        influenceValue = 5.0f;
        influenceRange = unitStats.attackRange*2;
    }

    void Update()
    {
        if (frozen) return;
        health = healthComponent.currentHealth;
        animator.SetFloat("Speed", agent.velocity.magnitude);


        if (currentOrder == null)
        {
            if (orders.Count > 0) // There are player-given orders
            {
                currentOrder = orders[0];
                orders.RemoveAt(0);
            }
            else // There are no player-given orders
            {
                Transform closest = this.GetComponent<Vision>().GetClosestTarget();
                if(closest != null)
                {
                    currentOrder = new Order(closest, false);
                }
            }
        }
        if(currentOrder != null) ExecuteOrder(currentOrder);
    }

    // Casts a firebolt at a target - the firebolt tracks target and follows till it hits.
    void CastFirebolt(Transform target)
    {
        agent.enabled = false;
        GameObject bolt = Instantiate(fireboltPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        Firebolt firebolt = bolt.GetComponent<Firebolt>();
        animator.SetTrigger("Attack");
        firebolt.Launch(target, unitStats.attackDamage);
        agent.enabled = true;
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
        Collider col = target.GetComponent<Collider>();
        if (col == null) return;

        Vector3 targetPosition = target.position;
        Vector3 directionToTarget = targetPosition - transform.position;
        float distanceToTarget = directionToTarget.magnitude;
        Vector3 directionNormalized = directionToTarget / Mathf.Max(distanceToTarget, 0.001f);

        RotateTowards(targetPosition);

        // Move if out of range and aggressive/player order
        if ((stance == CombatStance.Aggressive || order.isPlayerOrder) && distanceToTarget > unitStats.attackRange)
        {
            Vector3 attackDestination = targetPosition - directionNormalized * unitStats.attackRange;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(attackDestination, out hit, 1f, NavMesh.AllAreas))
                attackDestination = hit.position;

            agent.SetDestination(attackDestination);
            RotateTowards(targetPosition);
        }

        // Attack if in range and can see the target
        if (distanceToTarget <= unitStats.attackRange && Time.time >= lastAttackTime + unitStats.attackCooldown)
        {
            Vision vision = GetComponent<Vision>();
            if (vision != null && vision.CanSee(target))
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
        agent.SetDestination(position);
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

public enum CombatStance { Defensive, Aggressive }
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
