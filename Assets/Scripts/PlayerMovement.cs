using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    // ---------- Movement Variables ----------
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float sprintSpeed = 4f;
    float playerSpeed;

    // ---------- Jumping Variables ----------
    [Header("Jumping")]
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    bool isGrounded;
    bool isJumping;
    float jumpCooldown = 0.2f;
    float lastJumpTime;


    // ---------- Mouse Look Variables ----------

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraPivot;
    public float slideSwayAmount = 6f;
    public float slideSwaySpeed = 8f;
    float currentSway = 0f;
    float targetSway = 0f;

    float xRotation = 0f;   // vertical
    float yRotation = 0f;   // horizontal



    // ---------- Stamina Variables ----------

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 25f;
    public float staminaRegenRate = 15f;
    public float sprintCooldownThreshold = 10f;
    float currentStamina;
    bool canSprint = true;


    // --------- Speed Boost Variables ----------

    [Header("Speed Boost")]
    public float boostMultiplier = 1.5f;
    public float boostDuration = 3f;
    float boostTimer = 0f;
    bool boostActive = false;


    // --------- Slippery Surface Variables ----------

    [Header("Slippery State ")]
    public PhysicsMaterial normalMaterial;
    public PhysicsMaterial slipperyMaterial;
    public float slipperyControl = 0.2f;

    [Header("Slippery Recovery")]
    public float slipperyRecoverySpeed = 0.8f;

    [Header("Sliding")]
    public float slideDrag = 1.2f;

    [Header("Advanced Sliding")]
    public float maxSlideSpeed = 6f;
    public float angularInertia = 4f;
    public float highSpeedSlipMultiplier = 0.5f;
    public float iceExitFrictionBlend = 2f;

    // ---------- VFX ----------
    [Header("VFX")]
    public BloodScreenEffect bloodEffect;



    // ---------- INTERNAL STATE ----------
    CapsuleCollider capsule;
    float currentControl = 1f;
    float targetControl = 1f;
    float speedFactor;
    bool isMoving;
    bool isSprinting;
    Vector2 moveInput;
    Vector2 lookInput;
    Vector3 momentumDirection;
    Animator anim;
    Rigidbody rb;

    // ---------- LIFE CYCLE ----------
    void Start()
    {
        currentStamina = maxStamina;
        playerSpeed = walkSpeed;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        capsule = GetComponent<CapsuleCollider>();
        capsule.material = normalMaterial;
    }

    // ---------- INPUT ----------

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.magnitude > 0.1f)
        {
            momentumDirection =
                (transform.right * moveInput.x +
                 transform.forward * moveInput.y).normalized;
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
        playerSpeed = isSprinting ? sprintSpeed : walkSpeed;
    }

    public void OnSprintStop(InputValue value)
    {
        // Ensure sprinting stops when the key is released
        isSprinting = false;
        playerSpeed = walkSpeed;
    }

    [System.Obsolete]
    public void OnJump(InputValue value)
    {
        Debug.Log("Jump input received: " + value.isPressed);

        if (!value.isPressed)
            return;

        if (!isGrounded || isJumping)
            return;

        if (Time.time - lastJumpTime < jumpCooldown)
            return;

        lastJumpTime = Time.time;
        isJumping = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }


    // ---------- SPEED BOOST ----------
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        boostMultiplier = multiplier;
        boostDuration = duration;

        boostTimer = duration;
        boostActive = true;

        //Slippery effect when boosted
        capsule.material = slipperyMaterial;
        currentControl = slipperyControl;
        targetControl = slipperyControl;
    }
    // ---------- GET BOOST MULTIPLIER ----------
    float GetBoostMultiplier()
    {
        if (!boostActive)
            return 1f;

        boostTimer -= Time.deltaTime;

        if (boostTimer <= 0f)
        {
            boostActive = false;

            // Start recovering from slippery effect
            targetControl = 1f;

            //restore friction immediately
            capsule.material = normalMaterial;
        }

        return boostMultiplier;
    }

    // ---------- MOVEMENT ----------

    [System.Obsolete]
    void FixedUpdate()
{
    CheckGrounded();
    //Fallnack direction
    if (momentumDirection == Vector3.zero)
        momentumDirection = transform.forward;

    //currwent horizontal velocity
    Vector3 currentVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    float currentSpeed = currentVelocity.magnitude;

    // Stronger slide at higher speed
    speedFactor = Mathf.Lerp(1f, highSpeedSlipMultiplier, currentSpeed / maxSlideSpeed);
    
    float effectiveControl = currentControl * speedFactor;

    bool hasInput = moveInput.magnitude > 0.1f;

    // Desired movement direction
    Vector3 desiredDirection = hasInput
        ? (transform.right * moveInput.x + transform.forward * moveInput.y)
        : momentumDirection;

    // Angular inertia
    momentumDirection = Vector3.Slerp(
        momentumDirection,
        desiredDirection.normalized,
        effectiveControl * angularInertia * Time.fixedDeltaTime
    ).normalized;

    float targetSpeed = hasInput ? playerSpeed : currentSpeed;

    float newSpeed = Mathf.Lerp(
        currentSpeed,
        targetSpeed,
        effectiveControl
    );

    // Clamp to max slide speed
    newSpeed = Mathf.Clamp(newSpeed, 0f, maxSlideSpeed);

    Vector3 finalVelocity = momentumDirection * newSpeed;

    // ONLY control horizontal movement when grounded
    if (isGrounded)
    {
        rb.velocity = new Vector3(
            finalVelocity.x,
            rb.velocity.y,
            finalVelocity.z
        );
    }

    isMoving = newSpeed > 0.1f;
}

    // ---------- UPDATE ----------

    [System.Obsolete]
    void Update()
    {
        HandleCamera();
        HandleStamina();
        UpdateAnimator();

        currentControl = Mathf.MoveTowards(
            currentControl,
            targetControl,
            slipperyRecoverySpeed * Time.deltaTime);

        float speed = rb.velocity.magnitude;

        if (speed > maxSlideSpeed * 0.8f)
        {
            bloodEffect.ShowBlood(0.3f);
        }
    }

    // ---------- CAMERA ----------
    void HandleCamera()
    {
        float mouseX = lookInput.x * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * 100f * Time.deltaTime;

        // Vertical look
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        // Horizontal look
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // ---- CAMERA SWAY ----
        bool sliding = boostActive || currentControl < 1f;

        if (sliding)
        {
            float lateral = Vector3.Dot(momentumDirection, transform.right);
            targetSway = -lateral * slideSwayAmount;
        }
        else
        {
            targetSway = 0f;
        }

        currentSway = Mathf.Lerp(
            currentSway,
            targetSway,
            slideSwaySpeed * Time.deltaTime
        );

        cameraPivot.localRotation =
            Quaternion.Euler(xRotation, 0f, currentSway);
    }


    // ---------- STAMINA ----------
    void HandleStamina()
    {
        bool wantsToSprint = isSprinting && isMoving;

        if (wantsToSprint && canSprint && currentStamina > 0f)
        {
            playerSpeed = sprintSpeed * GetBoostMultiplier();
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                canSprint = false;
            }
        }
        else
        {
            playerSpeed = walkSpeed * GetBoostMultiplier();
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        // clamp stamina
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // allow sprint again after small recovery
        if (!canSprint && currentStamina >= sprintCooldownThreshold)
        {
            canSprint = true;
        }
    }

    //---------- GROUND CHECK ----------
    void CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float radius = 0.25f;

        bool wasGrounded = isGrounded;

        isGrounded = Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundLayer
        );

        if (!wasGrounded && isGrounded)
        {
            isJumping = false; // landed
        }
    }



    // ---------- ANIMATION ----------
    void UpdateAnimator()
    {
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isSprinting", isSprinting && canSprint && isMoving);
    }

    // ---------- DEBUG ----------
    void OnGUI()
    {
        GUI.Box(new Rect(20, 20, 200, 25), "");
        GUI.Box(
            new Rect(20, 20, 200 * (currentStamina / maxStamina), 25),
            "Stamina"
        );
    }



}
