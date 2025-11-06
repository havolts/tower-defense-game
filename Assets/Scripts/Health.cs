using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public HealthStats stats;
    float currentHealth;

    public event Action Died;

    void Awake()
    {
        currentHealth = (stats != null) ? stats.maxHealth : 10f;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"[Health:{name}] {currentHealth} HP");
        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        Debug.Log($"[Health:{name}] Dying.");
        Died?.Invoke();               
        Destroy(gameObject);
    }
}
