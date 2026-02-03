using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Low Health Warning Settings")]
    public float lowHealthThreshold = 0.3f;
    public float pulseSpeed = 2f;
    public float pulseScaleAmount = 0.1f;
    public Color lowHealthColor = Color.red;

    [Header("References")]
    public Health playerHealth;
    public Image fillImage;

    RectTransform rectTransform;
    Color originalColor;
    bool lowHealthActive;

  void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    void Start()
    {
        Debug.Log("PlayerHealthUI listening to: " + playerHealth.gameObject.name);

        if (!playerHealth || !fillImage)
        {
            Debug.LogError("PlayerHealthUI missing references!");
            enabled = false;
            return;
        }

        originalColor = fillImage.color;

        playerHealth.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar(playerHealth.currentHealth);
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

        if (!lowHealthActive)
        {
             rectTransform.localScale = Vector3.one;
            fillImage.color = originalColor;
        }
    }

    void Update()
    {
        if (!lowHealthActive) return;

        float pulse = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
        rectTransform.localScale = Vector3.one + Vector3.one * pulse * pulseScaleAmount;

        fillImage.color = Color.Lerp(originalColor, lowHealthColor, pulse);
    }
}
