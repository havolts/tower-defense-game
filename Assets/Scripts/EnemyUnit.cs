using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnit : MonoBehaviour
{
    float viewRadius = 100.0f; // Radius in which a target can be detected
    float visionAngle = 90.0f; // Angle in which a target can be detected. We could probably vary by unit type (Trolls have lower vision, etc.)
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();
    Transform currentTarget;

    // Probably super inefficient. Current working solution.
    // Take a look when free to streamline.
    void findVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < visionAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }

        // Sort visible targets by distance from the unit.
        // We should add a priority weight here so that a unit will try finish attacking its current target if still close enough even if another target becomes closer.
        visibleTargets.Sort((a, b) =>
            Vector3.Distance(transform.position, a.position)
            .CompareTo(Vector3.Distance(transform.position, b.position))
        );
    }

    //Deals with the actual movement and attacking.
    NavMeshAgent agent;
    public UnitStats unitStats;

    void moveToTarget()
    {
        if (agent == null) Debug.LogError("Agent is NULL on " + gameObject.name);
        if (unitStats == null) Debug.LogError("UnitStats is NULL on " + gameObject.name);
        if (currentTarget == null) Debug.LogError("currentTarget is NULL on " + gameObject.name);


        Vector3 dir = (currentTarget.position - transform.position).normalized;
        Vector3 destination = currentTarget.position - dir * unitStats.attackRange; // 5 = desired stopping distance
        agent.SetDestination(destination);
    }

    float lastAttackTime = 0f;

    void attackTarget()
    {
        if (Time.time >= lastAttackTime + unitStats.attackCooldown)
        {
            currentTarget.GetComponent<Health>()?.TakeDamage(unitStats.attackDamage);
            lastAttackTime = Time.time;
        }
    }


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0.0f;
    }

    void Update()
    {
        findVisibleTargets();

        if (visibleTargets.Count > 0)
        {
            // Pick first or nearest
            currentTarget = visibleTargets[0];
            moveToTarget();
            attackTarget();
        }
        else
        {
            currentTarget = null;
            agent.ResetPath(); // stops movement
        }
    }

    void LateUpdate()
    {
        if (currentTarget != null)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 10f);
        }
    }

    public Vector3 dirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 left = dirFromAngle(-visionAngle / 2, false);
        Vector3 right = dirFromAngle(visionAngle / 2, false);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, left * viewRadius);
        Gizmos.DrawRay(transform.position, right * viewRadius);

        if (visibleTargets != null)
        {
            Gizmos.color = Color.red;
            foreach (var t in visibleTargets)
            {
                Gizmos.DrawLine(transform.position, t.position);
            }
        }
    }
}
