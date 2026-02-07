using UnityEngine;
using System.Collections;

public class EnemyMeleeAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float detectionRangePositionOffsetY = 0.5f;
    public float attackRangePositionOffsetY = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;

    [Header("Attack")]
    public float attackDamage = 15f;
    public float attackCooldown = 1.2f;
    public bool isAttacking = false;

    [Header("Loot")]
    public GameObject heartPrefab;
    

    Transform player;
    Health playerHealth;
    Health myHealth;
    Rigidbody rb;

    float attackTimer;
    private EnemyAIAnimController enemyAnimController;



    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = player.GetComponent<Health>();
        }

        myHealth = GetComponent<Health>();
        if (myHealth != null)
            myHealth.OnDeath += HandleDeath;

        rb = GetComponent<Rigidbody>();
        enemyAnimController = GetComponent<EnemyAIAnimController>();
    }

    void FixedUpdate()
    {
        if (myHealth.IsDead || player == null || (playerHealth != null && playerHealth.IsDead))
            return;

        // Calculate distance using the center of the enemy (with offset) to the player
        Vector3 enemyCenter = transform.position + Vector3.up * detectionRangePositionOffsetY;
        Vector3 playerCenter = player.position + Vector3.up * detectionRangePositionOffsetY;
        float distance = Vector3.Distance(enemyCenter, playerCenter);


        if (distance <= detectionRange)
        {
            // Removed incorrect Idle() call here
            FacePlayer();

            if (distance > attackRange)
            {
                isAttacking = false;
                enemyAnimController.Chase();
                ChasePlayer();
            }
            else
            { 
                isAttacking = true;
                enemyAnimController.Attack();
                rb.linearVelocity = Vector3.zero; // Stop moving while attacking
            }
        }
        else
        {
            // Stop and Idle if player is out of range
            isAttacking = false;
            enemyAnimController.Idle();
            rb.linearVelocity = Vector3.zero;
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0;
        direction.Normalize();
        
        Vector3 targetVelocity = direction * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void DealDamage()
    {
        attackTimer = Time.time; // Reset cooldown timer

        if (playerHealth != null && !playerHealth.IsDead) // Apply damage
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

    void OnDestroy()
    {
        if (myHealth != null)
            myHealth.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        if (enemyAnimController != null)
            enemyAnimController.Die();
        
        StartCoroutine(DropHeart());
    }

    IEnumerator DropHeart()
    {
        yield return new WaitForSeconds(1.5f); // Wait for animation to finish
        if (heartPrefab != null)
            Instantiate(heartPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
    }
}
