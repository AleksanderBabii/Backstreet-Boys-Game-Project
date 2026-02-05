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
    public float mouseSensitivity = 0.5f;
    public Transform cameraPivot;
    public float slideSwaySpeed = 8f;
    public float bodyRotationSpeed = 8f;
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
    bool isMoving;
    bool isSprinting;
    Vector2 moveInput;
    Vector2 lookInput;
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
            // Reset local position to avoid jumping
            if (cameraPivot.localPosition == Vector3.zero)
            {
                cameraPivot.localPosition = new Vector3(0, 0.6f, 0.1f); // Adjust based on your spine size
            }
        }

        // Initialize rotation to current facing to prevent snapping
        yRotation = transform.eulerAngles.y;
    }

    // ---------- INPUT ----------

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.magnitude > 0.1f)
        {
            Vector3 forward = cameraPivot.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = cameraPivot.right;
            right.y = 0;
            right.Normalize();
            momentumDirection =
                (right * moveInput.x +
                 forward * moveInput.y).normalized;
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

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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

    [System.Obsolete]
    void FixedUpdate()
{
    CheckGrounded();
    //Fallnack direction
    if (momentumDirection == Vector3.zero)
        momentumDirection = transform.forward;

    //currwent horizontal velocity
    Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    float currentSpeed = currentVelocity.magnitude;

    float effectiveControl = 1f;

    bool hasInput = moveInput.magnitude > 0.1f;

    Vector3 forward = cameraPivot.forward;
    forward.y = 0;
    forward.Normalize();
    Vector3 right = cameraPivot.right;
    right.y = 0;
    right.Normalize();

    // Desired movement direction
    Vector3 desiredDirection = hasInput
        ? (right * moveInput.x + forward * moveInput.y)
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
        rb.linearVelocity = new Vector3(
            finalVelocity.x,
            rb.linearVelocity.y,
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

        // ---- CAMERA SWAY ----
        targetSway = 0f;

        currentSway = Mathf.Lerp(
            currentSway,
            targetSway,
            slideSwaySpeed * Time.deltaTime
        );

        // Compensate for body rotation so camera looks exactly where intended
        float parentY = cameraPivot.parent != null ? cameraPivot.parent.eulerAngles.y : 0f;
        float localY = Mathf.DeltaAngle(parentY, yRotation);

        cameraPivot.localRotation =
            Quaternion.Euler(xRotation, localY, currentSway);
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

        bool wasGrounded = isGrounded;

        // compute feet position from capsule
        Vector3 worldCenter = transform.TransformPoint(capsule.center);
        float sphereRadius = capsule.radius * 0.9f;
        Vector3 feetPos = worldCenter - transform.up * (capsule.height * 0.5f - capsule.radius);

        // Overlap sphere at feet to detect ground colliders on the specified layer
        Collider[] hits = Physics.OverlapSphere(feetPos, sphereRadius, groundLayer);
        isGrounded = (hits != null && hits.Length > 0);

        // Fallback: raycast downward from center if overlap didn't find anything
        RaycastHit rayHit = default;
        if (!isGrounded)
        {
            isGrounded = Physics.Raycast(worldCenter, Vector3.down, out rayHit, groundCheckDistance + 0.1f);
        }

        Debug.DrawLine(worldCenter, feetPos, isGrounded ? Color.green : Color.red);
        Debug.Log($"Ground Check - Feet: {feetPos}, Grounded: {isGrounded}, OverlapHits: {(hits!=null?hits.Length:0)}, RayHit: {(rayHit.collider? rayHit.collider.name : "None")}");

        if (!wasGrounded && isGrounded)
        {
            isJumping = false; // landed
        }
    }



    // ---------- ANIMATION ----------
    void UpdateAnimator()
    {
        // Calculate movement speed for blend tree transitions (0 = idle, 1 = running)
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        float velocityBased = Mathf.Clamp01(currentSpeed / (playerSpeed * 1.5f));

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

        // Debug: show animator input values when there's movement or input
        if (animatorSpeed > 0.01f || hasInput)
        {
            Debug.Log($"Animator Inputs - animatorSpeed: {animatorSpeed:F2}, velocityBased: {velocityBased:F2}, moveInput: {moveInput}");
        }
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
