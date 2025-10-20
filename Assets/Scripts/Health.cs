using UnityEngine;

public class Health : MonoBehaviour
{
    public HealthStats stats; // assign via Inspector
    float currentHealth;

    void Awake()
    {
        if (stats != null)
            currentHealth = stats.maxHealth;
        else
        {
            currentHealth = 100f; // fallback if none assigned
            Debug.Log("No Health Stats");
        }
    }

    public void TakeDamage(float amount)
    {
        Debug.Log(currentHealth);
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        Debug.Log("Dying.");
        Destroy(this.gameObject);
    }
}
