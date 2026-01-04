using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Suspend : MonoBehaviour
{
    private float aoeRadius;
    public float suspendHeight = 10f;
    public float duration = 3f; // Default duration

    [SerializeField] private LayerMask enemyLayer;

    void Awake()
    {
        // Initialize radius from Database
        aoeRadius = SpellDatabase.Instance.suspendSpell.radius;

        var ps = GetComponent<ParticleSystem>();
        var shape = ps.shape;
        shape.radius = aoeRadius;
    }

    void Start()
    {
        Explode();
        // Destroy the spell object after the duration is over
        Destroy(gameObject, duration + 2f);
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aoeRadius, enemyLayer);

        foreach (Collider hit in hits)
        {
            NavMeshAgent agent = hit.GetComponent<NavMeshAgent>();

            if (agent != null && agent.enabled)
            {
                StartCoroutine(SuspendRoutine(hit.transform, agent));
            }
        }
    }

    IEnumerator SuspendRoutine(Transform unitTransform, NavMeshAgent agent)
    {
        // 1. Shut down AI
        agent.enabled = false;

        Vector3 startPos = unitTransform.position;
        Vector3 targetPos = startPos + Vector3.up * suspendHeight;

        // 2. Lift Up (Lerp)
        float elapsed = 0;
        float transitionTime = 0.5f;
        while (elapsed < transitionTime)
        {
            unitTransform.position = Vector3.Lerp(startPos, targetPos, elapsed / transitionTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        unitTransform.position = targetPos;

        // 3. Stay in air for the spell duration
        yield return new WaitForSeconds(duration);

        // 4. Drop Down
        elapsed = 0;
        while (elapsed < transitionTime)
        {
            unitTransform.position = Vector3.Lerp(targetPos, startPos, elapsed / transitionTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        unitTransform.position = startPos;

        // 5. Restore AI
        if (agent != null)
        {
            agent.enabled = true;
        }
    }
}
