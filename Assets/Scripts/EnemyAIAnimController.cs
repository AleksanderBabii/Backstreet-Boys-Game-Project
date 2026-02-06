using UnityEngine;

public class EnemyAIAnimController : MonoBehaviour
{
    [Header("Animation")]
    public Animator enemyAnimator;
    [Header("Attack Timing")]
    public float attackDamageTime = 0.5f; // When in animation (0-1) to apply damage
    
    EnemyMeleeAI enemyMeleeAI;
    Rigidbody rb;
    private bool hasDealtDamageThisAttack = false;
    private float previousNormalizedTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (enemyAnimator == null)
        {
            // Priority: Check children first (common for imported models where mesh is a child)
            Animator[] anims = GetComponentsInChildren<Animator>();
            foreach (var anim in anims)
            {
                if (anim.gameObject != gameObject)
                {
                    enemyAnimator = anim;
                    break;
                }
            }
            // Fallback to self if no child animator found
            if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
        }
        enemyMeleeAI = GetComponent<EnemyMeleeAI>();
        rb = GetComponent<Rigidbody>();
        
        // Initialize animator state
        Idle();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if attack animation should deal damage
        CheckAttackAnimationDamage();
    }
    
    void CheckAttackAnimationDamage()
    {
        AnimatorStateInfo stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);
        
        // Check if currently in attack state
        if (stateInfo.shortNameHash == Animator.StringToHash("Attack"))
        {
            // Get normalized time (0 to 1) of the current animation
            float normalizedTime = stateInfo.normalizedTime % 1f; // % 1 handles looping
            
            // Detect when animation loops (normalizedTime went from high to low)
            if (normalizedTime < previousNormalizedTime)
            {
                hasDealtDamageThisAttack = false; // Reset for new animation cycle
            }
            
            // Deal damage once per animation cycle at the trigger time
            if (normalizedTime >= attackDamageTime && !hasDealtDamageThisAttack)
            {
                enemyMeleeAI.DealDamage();
                hasDealtDamageThisAttack = true;
            }
            
            previousNormalizedTime = normalizedTime;
        }
        else
        {
            // Reset damage flag when not in attack state
            hasDealtDamageThisAttack = false;
            previousNormalizedTime = 0f;
        }
    }

    public void Idle()
    {
        enemyAnimator.SetBool("isIdle", true);
        enemyAnimator.SetBool("isChasing", false);
        enemyAnimator.SetBool("isAttacking", false);
    }

    public void Attack()
    {
        enemyAnimator.SetBool("isAttacking", true);
        enemyAnimator.SetBool("isChasing", false);
        enemyAnimator.SetBool("isIdle", false);
    }

    public void Chase()
    {
        enemyAnimator.SetBool("isIdle", false);
        enemyAnimator.SetBool("isChasing", true);
        enemyAnimator.SetBool("isAttacking", false);
    }

    public void Die()
    {
        enemyAnimator.SetTrigger("Die");
    }
}
