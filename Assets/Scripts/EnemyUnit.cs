using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class EnemyUnit : MonoBehaviour
{
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public UnitStats unitStats;
    private Transform fortress;

    float viewRadius = 100f;
    float visionAngle = 90f;

    float scanInterval = 0.25f;
    float nextScanTime = 0f;

    List<Transform> visibleTargets = new List<Transform>();
    Transform currentTarget;
    NavMeshAgent agent;

    float lastAttackTime;

    public int skillPointReward = 1; 
    private bool rewardGranted;                       
    private Health health;                            

    enum UnitState
    {
        Idle,
        Searching,
        Chasing,
        Attacking
    }

    UnitState currentState = UnitState.Idle;

    void Start()
    {
        fortress = GameObject.Find("Fortress").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0f;

        health = GetComponent<Health>();
        if (health != null)
            health.Died += OnDied;

        ChangeState(UnitState.Searching);
    }


    void OnDestroy()
    {
        if (health != null)
            health.Died -= OnDied;
    }

    private void OnDied()
    {
        if (rewardGranted) return;
        rewardGranted = true;
        //Debug.Log($"[EnemyUnit:{name}] Died ? award {skillPointReward}");
        GameEvents.EnemyKilled(skillPointReward);
    }

    private void OnEnable()
    {
        health = GetComponent<Health>();
        //Debug.Log($"[EnemyUnit:{name}] OnEnable. Has Health? {health != null} enabled={enabled}");
        if (health != null)
        {
            health.Died -= OnDied; 
            health.Died += OnDied;
            //Debug.Log($"[EnemyUnit:{name}] Subscribed to Health.Died");
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= OnDied;
            //Debug.Log($"[EnemyUnit:{name}] Unsubscribed from Health.Died");
        }
    }



    void Update()
    {
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;
            ScanForTargets();
        }

        RunStateMachine();
    }

    void RunStateMachine()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                ChangeState(UnitState.Searching);
                break;

            case UnitState.Searching:
                if (currentTarget != null)
                    ChangeState(UnitState.Chasing);
                break;

            case UnitState.Chasing:
                if (currentTarget == null)
                {
                    ChangeState(UnitState.Searching);
                    return;
                }

                MoveToTarget();

                float dist = Vector3.Distance(transform.position, currentTarget.position);
                if (currentTarget != fortress && dist <= unitStats.attackRange)
                    ChangeState(UnitState.Attacking);
                break;

            case UnitState.Attacking:
                if (currentTarget == null)
                {
                    ChangeState(UnitState.Searching);
                    return;
                }

                float distToTarget = Vector3.Distance(transform.position, currentTarget.position);
                if (distToTarget > unitStats.attackRange + 1f)
                {
                    ChangeState(UnitState.Chasing);
                    return;
                }

                if (currentTarget != fortress)
                    AttackTarget();
                break;
        }
    }

    void ChangeState(UnitState newState)
    {
        currentState = newState;
    }

    void ScanForTargets()
    {
        visibleTargets.Clear();

        Collider[] targetsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        foreach (var col in targetsInView)
        {
            Transform target = col.transform;
            Vector3 dir = (target.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, target.position);

            if (Vector3.Angle(transform.forward, dir) < visionAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dir, dist, obstacleMask))
                    visibleTargets.Add(target);
            }
        }

        visibleTargets.Sort((a, b) =>
            Vector3.Distance(transform.position, a.position)
            .CompareTo(Vector3.Distance(transform.position, b.position))
        );

        if (visibleTargets.Count > 0)
            currentTarget = visibleTargets[0];
        else
            currentTarget = fortress;
    }

    void MoveToTarget()
    {
        if (currentTarget == null) return;

        // If target is fortress, move directly to it
        if (currentTarget == fortress)
        {
            agent.SetDestination(fortress.position);
            RotateTowards(fortress.position);
            return;
        }

        // Otherwise stop at attack range
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        Vector3 destination = currentTarget.position - dir * unitStats.attackRange;
        agent.SetDestination(destination);
        RotateTowards(currentTarget.position);
    }

    void RotateTowards(Vector3 position)
    {
        Vector3 dir = (position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 10f);
    }

    void AttackTarget()
    {
        RotateTowards(currentTarget.position);

        if (Time.time >= lastAttackTime + unitStats.attackCooldown)
        {
            currentTarget.GetComponent<Health>()?.TakeDamage(unitStats.attackDamage);
            lastAttackTime = Time.time;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 left = DirFromAngle(-visionAngle / 2, false);
        Vector3 right = DirFromAngle(visionAngle / 2, false);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, left * viewRadius);
        Gizmos.DrawRay(transform.position, right * viewRadius);
    }

    Vector3 DirFromAngle(float angle, bool global)
    {
        if (!global)
            angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
