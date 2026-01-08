using UnityEngine;

public class Firebolt : MonoBehaviour
{
    public float damage = 20f;
    public float speed = 10f; // units per second
    private Transform target;

    void Start()
    {
        GetComponent<ParticleSystem>().Play();
    }


    public void Launch(Transform targetEnemy, float damageAmount)
    {
        target = targetEnemy;
        damage = damageAmount;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction * distanceThisFrame, Space.World);
        transform.forward = direction;
    }

    void HitTarget()
    {
        Health health = target.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
