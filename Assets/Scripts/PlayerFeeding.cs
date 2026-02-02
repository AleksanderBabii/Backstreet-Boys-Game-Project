using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFeeding : MonoBehaviour
{
    [Header("Feeding")]
    public float feedRange = 2f;
    public float feedDamage = 40f;
    public float healAmount = 30f;
    public LayerMask enemyLayer;

    [Header("UI")]
    public FeedPromptUI feedPrompt;

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
    void Update()
    {
        bool canFeed = false;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            feedRange,
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy != null && enemy.CanBeFedOn())
            {
                canFeed = true;
                break;
            }
        }

        if (canFeed)
            feedPrompt.Show();
        else
            feedPrompt.Hide();
    }
}

