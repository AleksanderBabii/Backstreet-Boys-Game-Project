using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    [SerializeField]
    private PlayerHealthUI playerHealthUI;
    private EnemyMeleeAI enemyAI;


    void Awake()
    {
        currentHealth = maxHealth;
        IsDead = false;

        if (playerHealthUI != null)
        {
            playerHealthUI.SetMaxHealth(maxHealth);
        }
        enemyAI = GetComponent<EnemyMeleeAI>();
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"{gameObject.name} took {amount} damage");

        if (IsDead) return;

        currentHealth -= amount; //
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Current HP: {currentHealth}");

        if (playerHealthUI != null)
        {
            playerHealthUI.SetHealth(currentHealth);
        }
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
            Die();
    }
    void Update()
    {
        if (enemyAI != null && enemyAI.isAttacking)
        {
            // Reset attacking state after applying damage
            enemyAI.isAttacking = false;
        }
        
    }


    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (playerHealthUI != null)
        {
            playerHealthUI.SetHealth(currentHealth);
        }
        OnHealthChanged?.Invoke(currentHealth);
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnDeath?.Invoke();

        Debug.Log($"{gameObject.name} died.");

        // Delay destruction for player/enemy to play death animation
        Destroy(gameObject, 2f);
    }
}
