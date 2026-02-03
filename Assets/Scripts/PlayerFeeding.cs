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

        Collider[] hits = Physics.OverlapSphere(transform.position, feedRange, enemyLayer);

        foreach (Collider hit in hits)
        {
            Health enemyHealth = hit.GetComponent<Health>();
            EnemyMeleeAI enemyAI = hit.GetComponent<EnemyMeleeAI>();

            if (enemyHealth == null || enemyAI == null) continue;
            if (!enemyAI.CanBeFedOn() || enemyHealth.IsDead) continue;

            // Drain enemy
            enemyHealth.TakeDamage(feedDamage);

            // Heal player
            playerHealth.Heal(healAmount);

            Debug.Log("FEEDING!");
            break;
        }
    }

    void Update()
    {
        bool canFeed = false;
        Collider[] hits = Physics.OverlapSphere(transform.position, feedRange, enemyLayer);

        foreach (Collider hit in hits)
        {
            EnemyMeleeAI enemyAI = hit.GetComponent<EnemyMeleeAI>();
            Health enemyHealth = hit.GetComponent<Health>();

            if (enemyAI != null && enemyHealth != null && !enemyHealth.IsDead && enemyAI.CanBeFedOn())
            {
                canFeed = true;
                break;
            }
        }

        if (feedPrompt != null)
        {
            if (canFeed) feedPrompt.Show();
            else feedPrompt.Hide();
        }
    }
}
