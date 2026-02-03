using UnityEngine;

public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;

    [Header("Attack")]
    public float attackDamage = 15f;
    public float attackCooldown = 1.2f;

    Transform player;
    Health playerHealth;
    Health myHealth;
    Rigidbody rb;

    float attackTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<Health>();
        myHealth = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (myHealth.IsDead || playerHealth.IsDead)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            FacePlayer();

            if (distance > attackRange)
            {
                ChasePlayer();
            }
            else
            {
                Attack();
            }
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Vector3 move = direction * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + move);
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    void Attack()
    {
        if (Time.time - attackTimer < attackCooldown)
            return;

        attackTimer = Time.time;

        if (playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("Player hit by melee enemy!");
        }

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public bool CanBeFedOn()
    {
        // Feedable if below 50% health
        return myHealth != null && !myHealth.IsDead && myHealth.currentHealth <= myHealth.maxHealth * 0.5f;
    }
}

