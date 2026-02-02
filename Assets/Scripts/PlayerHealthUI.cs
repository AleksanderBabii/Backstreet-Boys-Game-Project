using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Low Health Warning Settings")]
    public float lowHealthThreshold = 0.3f;
    public float pulseSpeed = 2f;
    public float pulseScaleAmount = 0.1f;
    public Color lowHealthColor = Color.red;

    RectTransform rectTransform;
    Color originalColor;
    bool lowHealthActive;

    // Reference to the player's health component
    public Health playerHealth;
    public Image fillImage;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalColor = fillImage.color;

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(playerHealth.currentHealth);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    void UpdateHealthBar(float currentHealth)
    {
        float percent = currentHealth / playerHealth.maxHealth;
        fillImage.fillAmount = percent;

        lowHealthActive = percent <= lowHealthThreshold;
        if (lowHealthActive)
        {
            rectTransform.localScale = Vector3.one;
            fillImage.color = lowHealthColor;
        }
    }

    void Update()
    {
        if (!lowHealthActive)
            return;

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseScaleAmount;

        rectTransform.localScale = Vector3.one + Vector3.one * pulse;
        fillImage.color = Color.Lerp(
            originalColor,
            lowHealthColor,
            Mathf.Abs(pulse)
        );
    }
}

