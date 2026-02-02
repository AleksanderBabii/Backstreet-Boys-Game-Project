using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFeeding : MonoBehaviour
{
    public float feedRange = 2f;
    public float feedDamage = 40f;
    public float healAmount = 30f;
    public LayerMask enemyLayer;

    Health playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<Health>();
    }

    public void OnFeed(InputValue value)
    {
        if (!value.isPressed) return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            feedRange,
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            Health enemyHealth = hit.GetComponent<Health>();

            if (enemy != null && enemyHealth != null && enemy.CanBeFedOn())
            {
                // Drain enemy
                enemyHealth.TakeDamage(feedDamage);

                // Heal player
                playerHealth.Heal(healAmount);

                // Optional: effects
                Debug.Log("FEEDING!");

                break;
            }
        }
    }
}

