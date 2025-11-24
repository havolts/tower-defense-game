using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This script is very likely incredibly inefficient. Test and fix.
*/

public class Vision : MonoBehaviour
{
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    float viewRadius = 100f;
    float visionAngle = 110f;

    float scanInterval = 0.25f;
    float nextScanTime = 0f;

    List<Transform> visibleTargets = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;
            int numOrders = this.GetComponent<FriendlyUnit>().orders.Count;
            if (numOrders <= 0)
            {
                ScanForTargets();
            }
        }
    }

    Transform GetClosestTarget(List<Transform> visibleTargets, Vector3 origin)
    {
        Transform closest = null;
        float closestDistSq = float.MaxValue;

        foreach (var target in visibleTargets)
        {
            float distSq = (target.position - origin).sqrMagnitude;
            if (distSq < closestDistSq)
            {
                closest = target;
                closestDistSq = distSq;
            }
        }

        return closest;
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

        var closest = GetClosestTarget(visibleTargets, transform.position);

        if (visibleTargets.Count > 0)
        {
            Order newOrder = new Order(OrderType.Attack, closest, false);
            this.GetComponent<FriendlyUnit>().AddOrder(newOrder, true);
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
