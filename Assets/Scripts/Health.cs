using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsDead => currentHealth <= 0f;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;    

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);
    }

    public void Die() 
    {
        // Handle player death (e.g., play animation, respawn, etc.)
        Debug.Log("Player has died.");
        Destroy(gameObject, 2f);
    
    }
}

