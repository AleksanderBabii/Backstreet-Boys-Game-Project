using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    [Header("Animation")]
    public Animator playerAnimator;

    [Header("Movement Parameters")]
    public float walkThreshold = 0.3f;  // Below this = Idle
    public float runThreshold = 0.7f;   // Above this = Run

    [Header("Attack Timing")]
    public float swordAttackDamageTime = 0.5f;
    public float gunAttackDamageTime = 0.4f;

    private PlayerCombat playerCombat;
    private bool hasDealtSwordDamageThisAttack = false;
    private bool hasDealtGunDamageThisAttack = false;
    private float previousSwordNormalizedTime = 0f;
    private float previousGunNormalizedTime = 0f;

    private float lastSwordAttackTime = -1f;
    private float lastGunAttackTime = -1f;
    private const float AttackInputDuration = 0.1f;

    void Start()
    {
        if (playerAnimator == null)
        {
            Animator[] anims = GetComponentsInChildren<Animator>();
            foreach (var anim in anims)
            {
                if (anim.gameObject != gameObject)
                {
                    playerAnimator = anim;
                    break;
                }
            }
            if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        }
        // Ensure animator doesn't move the transform via root motion (we drive movement via Rigidbody)
        if (playerAnimator != null)
            playerAnimator.applyRootMotion = false;
        playerCombat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        CheckSwordAttackDamage();
        CheckGunAttackDamage();

        if (Time.time >= lastSwordAttackTime + AttackInputDuration)
            playerAnimator.SetBool("isSwordAttacking", false);
        if (Time.time >= lastGunAttackTime + AttackInputDuration)
            playerAnimator.SetBool("isGunShooting", false);
    }

    void CheckSwordAttackDamage()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        // Check if in sword attack state
        if (stateInfo.shortNameHash == Animator.StringToHash("SwordAttack"))
        {
            float normalizedTime = stateInfo.normalizedTime % 1f;

            // Detect when animation loops
            if (normalizedTime < previousSwordNormalizedTime)
            {
                hasDealtSwordDamageThisAttack = false;
            }

            // Deal damage at specific point in animation
            if (normalizedTime >= swordAttackDamageTime && !hasDealtSwordDamageThisAttack)
            {
                playerCombat.DealSwordDamage();
                hasDealtSwordDamageThisAttack = true;
            }

            previousSwordNormalizedTime = normalizedTime;
        }
        else
        {
            hasDealtSwordDamageThisAttack = false;
            previousSwordNormalizedTime = 0f;
        }
    }

    void CheckGunAttackDamage()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        // Check if in gun shoot state
        if (stateInfo.shortNameHash == Animator.StringToHash("GunShoot"))
        {
            float normalizedTime = stateInfo.normalizedTime % 1f;

            // Detect when animation loops
            if (normalizedTime < previousGunNormalizedTime)
            {
                hasDealtGunDamageThisAttack = false;
            }

            // Deal damage at specific point in animation
            if (normalizedTime >= gunAttackDamageTime && !hasDealtGunDamageThisAttack)
            {
                playerCombat.DealGunDamage();
                hasDealtGunDamageThisAttack = true;
            }

            previousGunNormalizedTime = normalizedTime;
        }
        else
        {
            hasDealtGunDamageThisAttack = false;
            previousGunNormalizedTime = 0f;
        }
    }

    /// <summary>
    /// Set movement speed (0-1 normalized). Controls transitions between Idle, Walk, and Run.
    /// </summary>
    public void SetMovementSpeed(float speed)
    {
        playerAnimator.SetFloat("moveSpeed", speed, 0.1f, Time.deltaTime);
    }

    /// <summary>
    /// Set horizontal movement input for 2D blend tree (-1 to 1)
    /// </summary>
    public void SetMovementX(float moveX)
    {
        playerAnimator.SetFloat("moveX", moveX, 0.1f, Time.deltaTime);
    }

    /// <summary>
    /// Set vertical movement input for 2D blend tree (-1 to 1)
    /// </summary>
    public void SetMovementY(float moveY)
    {
        playerAnimator.SetFloat("moveY", moveY, 0.1f, Time.deltaTime);
    }

    /// <summary>
    /// Trigger sword attack animation
    /// </summary>
    public void SwordAttack()
    {
        playerAnimator.SetBool("isSwordAttacking", true);
        playerAnimator.SetBool("isGunShooting", false);
        lastSwordAttackTime = Time.time;
    }

    /// <summary>
    /// Trigger gun shoot animation
    /// </summary>
    public void GunShoot()
    {
        playerAnimator.SetBool("isGunShooting", true);
        playerAnimator.SetBool("isSwordAttacking", false);
        lastGunAttackTime = Time.time;
    }

    /// <summary>
    /// Reset attack animations (call from Attack state on exit)
    /// </summary>
    public void ResetAttacks()
    {
        playerAnimator.SetBool("isSwordAttacking", false);
        playerAnimator.SetBool("isGunShooting", false);
    }

    public void Jump()
    {
        playerAnimator.SetTrigger("Jump");
    }

    public void SetGrounded(bool isGrounded)
    {
        playerAnimator.SetBool("isGrounded", isGrounded);
    }

    public void Die()
    {
        playerAnimator.SetTrigger("Die");
    }
}
