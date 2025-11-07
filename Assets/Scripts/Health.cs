using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public HealthStats stats;
    private float currentHealth;

    public event Action Died;
    public event Action<float> Damaged;

    void Awake()
    {
        currentHealth = (stats != null) ? stats.maxHealth : 10f;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"[Health: {name}] {currentHealth} HP");
        Damaged?.Invoke(currentHealth);

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        Debug.Log($"[Health:{name}] Dying.");
        Died?.Invoke();
        Destroy(gameObject);
    }

    public float CurrentHealthFraction()
    {
        return Mathf.Clamp01(currentHealth / ((stats != null) ? stats.maxHealth : 10f));
    }
}
