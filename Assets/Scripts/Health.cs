using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public HealthStats stats;
    float currentHealth;
    private HealthUI healthUI; 

    void Awake() {
        if (stats != null)
            currentHealth = stats.maxHealth;
        else {
            currentHealth = 10f;
            Debug.Log("No Health Stats");
        }

        healthUI = GetComponentInChildren<HealthUI>();
        if (healthUI != null)
            healthUI.Start3DSlider(currentHealth);
    }

    public void TakeDamage(float amount) {
        currentHealth -= amount;

        if (healthUI != null)
            healthUI.Update3DSSlider(currentHealth);

        if (currentHealth <= 0f)
            Die();
    }

    void Die() {
        Debug.Log($"{gameObject.name} died.");
        Destroy(gameObject);
    }
}
