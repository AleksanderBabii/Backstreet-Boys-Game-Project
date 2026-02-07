using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // ---------- Movement Variables ----------
    [Header("Movement")]
    public float groundDrag = 5f;
    public float walkSpeed = 2f;
    public float sprintSpeed = 4f;
    float playerSpeed;

    // ---------- Jumping Variables ----------
    [Header("Jumping")]
    public float jumpDuration = 0.8f;
    public float airControl = 0.1f;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;
    bool isGrounded;
    bool isJumping;
    float jumpCooldown = 0.2f;
    float lastJumpTime;


    // ---------- Mouse Look Variables ----------

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.5f;
    public Transform cameraPivot;
    public float bodyRotationSpeed = 8f;

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
    bool isMoving;
    bool isSprinting;
    Vector2 moveInput;
    Vector2 lookInput;
    private bool m_IsOnSlipperySurface;
    Vector3 momentumDirection;
    Rigidbody rb;
    PlayerAnimController playerAnimController;

    // ---------- LIFE CYCLE ----------
    void Start()
    {
        currentStamina = maxStamina;
        playerSpeed = walkSpeed;
        rb = GetComponent<Rigidbody>();
        playerAnimController = GetComponent<PlayerAnimController>();

        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        capsule = GetComponent<CapsuleCollider>();
        capsule.material = normalMaterial;

        // Find CameraPivot - check parent first (if script is on root and CameraPivot is on Player_Vamp)
        if (cameraPivot == null && transform.parent != null)
        {
            cameraPivot = transform.parent.Find("CameraPivot");
        }
        
        // Otherwise check as direct child
        if (cameraPivot == null)
        {
            cameraPivot = transform.Find("CameraPivot");
        }

        // Find spine_01 and parent cameraPivot to it
        Transform spine = transform.Find("spine_01");
        if (spine == null)
        {
            // Try to find it through pelvis
            Transform pelvis = transform.Find("pelvis");
            if (pelvis != null)
            {
                spine = pelvis.Find("spine_01");
            }
        }

        // Parent cameraPivot to spine_01 if spine exists
        if (spine != null && cameraPivot != null)
        {
            cameraPivot.SetParent(spine);
            // Reset local position to a sensible default to avoid camera jumping
            cameraPivot.localPosition = new Vector3(0, 0.6f, 0.1f); // Adjust based on your spine size
        }

        // Initialize rotation to current facing to prevent snapping
        yRotation = transform.eulerAngles.y;

        if (groundLayer == 0)
        {
            Debug.LogWarning("PlayerMovement: Ground Layer is not set! Jumping will not work.");
        }
    }

    // ---------- INPUT ----------

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
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

    public void OnJump(InputValue value)
    {

        if (!value.isPressed)
            return;

        if (!isGrounded || isJumping)
            return;

        if (Time.time - lastJumpTime < jumpCooldown)
            return;

        lastJumpTime = Time.time;
        isJumping = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // Calculate required velocity to stay in air for jumpDuration: v = (g * t) / 2
        float gravity = Mathf.Abs(Physics.gravity.y);
        float jumpVelocity = (gravity * jumpDuration) / 2f;
        rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
    }


    // ---------- SPEED BOOST ----------
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        boostMultiplier = multiplier;
        boostDuration = duration;

        boostTimer = duration;
        boostActive = true;
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

            //restore friction immediately
            capsule.material = normalMaterial;
        }

        return boostMultiplier;
    }

    // ---------- MOVEMENT ----------

    void FixedUpdate()
    {
        CheckGrounded();
        // Fallback direction
        if (momentumDirection == Vector3.zero)
            momentumDirection = transform.forward;

        // current horizontal velocity
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentSpeed = currentHorizontalVelocity.magnitude;

        // Determine control and drag based on state
        float effectiveControl;
        float currentDrag;

        if (isGrounded)
        {
            if (m_IsOnSlipperySurface)
            {
                effectiveControl = slipperyControl;
                currentDrag = slideDrag;
            }
            else
            {
                effectiveControl = 1f; // Full control on normal ground
                currentDrag = groundDrag;

                // Fix: Apply downward force to prevent flying off slopes when moving fast (Speed Boost)
                if (!isJumping && rb.linearVelocity.y < 2f)
                {
                    rb.AddForce(Vector3.down * 40f, ForceMode.Force);
                }
            }
        }
        else
        {
            effectiveControl = airControl;
            currentDrag = 0f; // No drag in the air from this system
        }

        bool hasInput = moveInput.magnitude > 0.1f;

        // Desired movement direction
        Vector3 forward = cameraPivot.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = cameraPivot.right;
        right.y = 0;
        right.Normalize();
        Vector3 desiredDirection = hasInput
            ? (right * moveInput.x + forward * moveInput.y).normalized
            : momentumDirection;

        // Angular inertia
        momentumDirection = Vector3.Slerp(
            momentumDirection,
            desiredDirection,
            effectiveControl * angularInertia * Time.fixedDeltaTime
        ).normalized;

        // Determine target speed
        float targetSpeed;
        if (hasInput)
        {
            targetSpeed = playerSpeed; // playerSpeed already includes boost
        }
        else
        {
            // Apply drag if on the ground and no input
            targetSpeed = isGrounded ? Mathf.MoveTowards(currentSpeed, 0, currentDrag * Time.fixedDeltaTime) : currentSpeed;
        }

        // Smoothly approach target speed
        float newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 15f * Time.fixedDeltaTime);
        newSpeed = Mathf.Clamp(newSpeed, 0f, maxSlideSpeed);

        Vector3 finalVelocity = momentumDirection * newSpeed;

        rb.linearVelocity = new Vector3(finalVelocity.x, rb.linearVelocity.y, finalVelocity.z);

        isMoving = newSpeed > 0.1f;
    }

    // ---------- UPDATE ----------

    void Update()
    {
        HandleCamera();
        HandleStamina();
        UpdateAnimator();

        float speed = rb.linearVelocity.magnitude;

        if (speed > maxSlideSpeed * 0.8f)
        {
            bloodEffect.ShowBlood(0.3f);
        }
    }

    // ---------- CAMERA ----------
    void HandleCamera()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Vertical look
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        // Horizontal look
        yRotation += mouseX;
        
        // Smoothly rotate player body to follow camera Y-axis for natural turning
        Quaternion targetRotation = Quaternion.Euler(0f, yRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, bodyRotationSpeed * Time.deltaTime);

        // Compensate for body rotation so camera looks exactly where intended
        float parentY = cameraPivot.parent != null ? cameraPivot.parent.eulerAngles.y : 0f;
        float localY = Mathf.DeltaAngle(parentY, yRotation);

        cameraPivot.localRotation =
            Quaternion.Euler(xRotation, localY, 0f);
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
        if (capsule == null) capsule = GetComponent<CapsuleCollider>();

        // compute feet position from capsule
        Vector3 worldCenter = transform.TransformPoint(capsule.center);
        Vector3 feetPos = worldCenter - transform.up * (capsule.height * 0.5f);

        // Overlap sphere at feet to detect ground colliders on the specified layer
        Collider[] hits = Physics.OverlapSphere(feetPos, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);
        
        bool wasGrounded = isGrounded;
        isGrounded = hits.Length > 0;
        
        // Reset jumping state if grounded and we're past the initial jump impulse time
        if (isGrounded && isJumping && Time.time - lastJumpTime > 0.2f)
        {
            isJumping = false;
        }

        // Logic to prevent jump/fall animation while running down slopes or over bumps
        bool animatorIsGrounded = isGrounded;
        
        if (isJumping)
        {
            animatorIsGrounded = false;
        }
        else if (!isGrounded && Mathf.Abs(rb.linearVelocity.y) < 5f)
        {
            animatorIsGrounded = true;
        }
        playerAnimController.SetGrounded(animatorIsGrounded);

        m_IsOnSlipperySurface = false;
        if (isGrounded)
        {
            // Check material of the ground
            PhysicsMaterial groundMaterial = hits[0].sharedMaterial;
            if (groundMaterial != null && slipperyMaterial != null && groundMaterial.name.Split(' ')[0] == slipperyMaterial.name.Split(' ')[0])
            {
                m_IsOnSlipperySurface = true;
            }
        }

        capsule.material = m_IsOnSlipperySurface ? slipperyMaterial : normalMaterial;
    }



    // ---------- ANIMATION ----------
    void UpdateAnimator()
    {
        // Calculate movement speed for blend tree transitions (0 = idle, 1 = running)
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        float velocityBased = Mathf.Clamp01(currentSpeed / (sprintSpeed * 1.5f));

        bool hasInput = moveInput.magnitude > 0.1f;
        // If player has fresh input, drive animations from input/sprint state for snappy response
        float animatorSpeed;
        if (hasInput)
        {
            animatorSpeed = (isSprinting && canSprint) ? 1f : 0.5f; // 0.5 = walk, 1 = run
        }
        else
        {
            animatorSpeed = velocityBased;
        }

        // Update movement speed for blend trees
        playerAnimController.SetMovementSpeed(animatorSpeed);

        // Update directional movement input for 2D blend tree
        float sendMoveX = moveInput.x;
        float sendMoveY = moveInput.y;
        // If raw input is zero, use momentumDirection so animator still sees movement direction
        if (Mathf.Abs(sendMoveX) < 0.01f && Mathf.Abs(sendMoveY) < 0.01f)
        {
            Vector3 localMomentum = transform.InverseTransformDirection(momentumDirection);
            sendMoveX = Mathf.Clamp(localMomentum.x, -1f, 1f);
            sendMoveY = Mathf.Clamp(localMomentum.z, -1f, 1f);
        }

        playerAnimController.SetMovementX(sendMoveX);
        playerAnimController.SetMovementY(sendMoveY);
        playerAnimController.SetJumping(isJumping);
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
