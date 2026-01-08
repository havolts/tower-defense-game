using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Disperse : MonoBehaviour
{
    public float speed;
    float aoeRadius;

    private Vector3 targetPosition;

    [SerializeField] private LayerMask enemyLayer;


    void Start()
    {
        GetComponent<ParticleSystem>().Play();
    }

    public void LaunchAtPosition(Vector3 position, float radius)
    {
        targetPosition = position;
        aoeRadius = radius;
    }

    void Update()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) <= distanceThisFrame)
        {
            Explode();
            return;
        }

        transform.Translate(direction * distanceThisFrame, Space.World);
        transform.forward = direction;
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aoeRadius, enemyLayer);

        foreach (Collider hit in hits)
        {
            NavMeshAgent agent = hit.GetComponent<NavMeshAgent>();
            if (agent == null) continue;

            Vector3 dir = (agent.transform.position - transform.position).normalized;
            dir.y = 0;
            agent.enabled = false;

            agent.transform.position += dir * 5;

            agent.enabled = true;
        }

        Destroy(gameObject);
    }


}
