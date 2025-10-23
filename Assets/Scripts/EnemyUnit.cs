using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnit : MonoBehaviour
{
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public UnitStats unitStats;

    float viewRadius = 100f;
    float visionAngle = 90f;

    float scanInterval = 0.25f;
    float nextScanTime = 0f;

    List<Transform> visibleTargets = new List<Transform>();
    Transform currentTarget;
    NavMeshAgent agent;

    float lastAttackTime;

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
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0f;

        ChangeState(UnitState.Searching);
    }

    void Update()
    {
        RunStateMachine();
    }

    void RunStateMachine()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                // No operation, could add patrol later
                break;

            case UnitState.Searching:
                if (Time.time >= nextScanTime)
                {
                    nextScanTime = Time.time + scanInterval;
                    ScanForTargets();
                }
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
                if (dist <= unitStats.attackRange)
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

        for (int i = 0; i < targetsInView.Length; i++)
        {
            Transform target = targetsInView[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < visionAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                    visibleTargets.Add(target);
            }
        }

        if (visibleTargets.Count > 0)
        {
            visibleTargets.Sort((a, b) =>
                Vector3.Distance(transform.position, a.position)
                .CompareTo(Vector3.Distance(transform.position, b.position))
            );

            currentTarget = visibleTargets[0];
        }
        else
        {
            currentTarget = null;
        }
    }

    void MoveToTarget()
    {
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
