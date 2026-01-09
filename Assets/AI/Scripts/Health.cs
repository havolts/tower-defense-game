using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public HealthStats stats;
    public event Action Died;
    public float currentHealth;

    void Awake()
    {
        currentHealth = (stats != null) ? stats.maxHealth : 10f;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        Died?.Invoke();
        Destroy(gameObject);
    }


    public float CurrentHealthFraction()
    {
        return Mathf.Clamp01(currentHealth / ((stats != null) ? stats.maxHealth : 10f));
    }
}
