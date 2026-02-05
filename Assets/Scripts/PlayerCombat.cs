using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Sword Attack")]
    public GameObject swordObject;
    public float swordRange = 2f;
    public float swordDamage = 25f;
    public float swordAttackCooldown = 0.8f;
    public LayerMask enemyLayer;

    [Header("Gun Attack")]
    public GameObject gunObject;
    public float gunRange = 15f;
    public float gunDamage = 20f;
    public float gunAttackCooldown = 0.5f;

    private PlayerAnimController playerAnimController;
    private Health playerHealth;
    private float swordAttackTimer = 0f;
    private float gunAttackTimer = 0f;

    void Start()
    {
        playerAnimController = GetComponent<PlayerAnimController>();
        playerHealth = GetComponent<Health>();

        if (swordObject == null)
            Debug.LogWarning("Sword object not assigned to PlayerCombat!");
        if (gunObject == null)
            Debug.LogWarning("Gun object not assigned to PlayerCombat!");
    }

    void Update()
    {
        // Update cooldown timers
        swordAttackTimer += Time.deltaTime;
        gunAttackTimer += Time.deltaTime;
    }

    /// <summary>
    /// Input callback for sword attack
    /// </summary>
    public void OnSwordAttack(InputValue value)
    {
        if (!value.isPressed || playerHealth.IsDead)
            return;

        if (swordAttackTimer >= swordAttackCooldown)
        {
            playerAnimController.SwordAttack();
            swordAttackTimer = 0f;
            Debug.Log("Sword Attack Initiated!");
        }
    }

    /// <summary>
    /// Input callback for gun attack
    /// </summary>
    public void OnGunAttack(InputValue value)
    {
        if (!value.isPressed || playerHealth.IsDead)
            return;

        if (gunAttackTimer >= gunAttackCooldown)
        {
            playerAnimController.GunShoot();
            gunAttackTimer = 0f;
            Debug.Log("Gun Attack Initiated!");
        }
    }

    /// <summary>
    /// Deal sword damage to enemies in range
    /// Called from PlayerAnimController during attack animation
    /// </summary>
    public void DealSwordDamage()
    {
        // Check for enemies in sword range
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * swordRange * 0.5f, 
            swordRange, 
            enemyLayer
        );

        bool hitAny = false;
        foreach (Collider hit in hits)
        {
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                enemyHealth.TakeDamage(swordDamage);
                hitAny = true;
            }
        }

        if (hitAny)
            Debug.Log("Enemy hit with sword!");
    }

    /// <summary>
    /// Deal gun damage to enemies in range
    /// Called from PlayerAnimController during shoot animation
    /// </summary>
    public void DealGunDamage()
    {
        // Check for enemies in gun range
        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * gunRange * 0.5f, 
            gunRange, 
            enemyLayer
        );

        bool hitAny = false;
        foreach (Collider hit in hits)
        {
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                enemyHealth.TakeDamage(gunDamage);
                hitAny = true;
            }
        }

        if (hitAny)
            Debug.Log("Enemy hit with gun!");
    }

    void OnDrawGizmosSelected()
    {
        // Sword range visualization
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.forward * swordRange * 0.5f, swordRange);

        // Gun range visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * gunRange * 0.5f, gunRange);
    }
}
