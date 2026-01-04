using UnityEngine;

public class Meteor : MonoBehaviour
{
    float damage;
    public float speed;
    float aoeRadius;

    private Vector3 targetPosition;

    [SerializeField] private LayerMask enemyLayer;


    void Start()
    {
        GetComponent<ParticleSystem>().Play();
    }

    public void LaunchAtPosition(Vector3 position, float damageAmount, float radius)
    {
        targetPosition = position;
        damage = damageAmount;
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
            Health health = hit.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
