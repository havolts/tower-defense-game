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

    //Could probably add to unitstats
    public float viewRadius = 100f;
    public float visionAngle = 110f;

    List<Transform> visibleTargets = new List<Transform>();

    public Transform GetClosestTarget()
    {
        Vector3 origin = transform.position;
        Transform closest = null;
        float closestDistSq = float.MaxValue;
        ScanForTargets();

        foreach (var target in visibleTargets)
        {
            float distSq = (target.position - origin).sqrMagnitude;
            if (distSq < closestDistSq)
            {
                closest = target;
                closestDistSq = distSq;
            }
        }
        //Debug.Log(closest.name);
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
                if (!Physics.Raycast(transform.position, dir, dist, obstacleMask)) visibleTargets.Add(target);
            }
        }
    }

    //To be removed with final build
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
