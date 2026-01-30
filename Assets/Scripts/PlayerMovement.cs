using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float sprintSpeed = 4f;
    public float jumpHeight = 3f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraPivot;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 25f;
    public float staminaRegenRate = 15f;
    public float sprintCooldownThreshold = 10f;

    float currentStamina;
    bool canSprint = true;

    [Header("Speed Boost")]
    public float boostMultiplier = 1.5f;
    public float boostDuration = 3f;

float boostTimer = 0f;
bool boostActive = false;

    float playerSpeed;
    float xRotation = 0f;   // vertical
    float yRotation = 0f;   // horizontal

    bool isMoving;
    bool isSprinting;

    Vector2 moveInput;
    Vector2 lookInput;

    Animator anim;
    Rigidbody rb;

    void Start()
    {
        currentStamina = maxStamina;
        playerSpeed = walkSpeed;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        if (value.isPressed)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
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
        return 1f;
    }

    return boostMultiplier;
}

    // ---------- MOVEMENT ----------

    [System.Obsolete]
    void FixedUpdate()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        rb.velocity = new Vector3(
            move.x * playerSpeed,
            rb.velocity.y,
            move.z * playerSpeed);

        isMoving = moveInput.magnitude > 0.1f;
    }

    // ---------- UPDATE ----------

    void Update()
    {
        HandleCamera();
        HandleStamina();
        UpdateAnimator();
    }

    // ---------- CAMERA ----------
    void HandleCamera()
    {
        float mouseX = lookInput.x * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * 100f * Time.deltaTime;

        // vertical look
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        cameraPivot.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        // horizontal look
        yRotation += mouseX;
        transform.rotation =
            Quaternion.Euler(0f, yRotation, 0f);
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
