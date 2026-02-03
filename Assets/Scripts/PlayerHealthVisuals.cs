using UnityEngine;

public class PlayerHealthVisuals : MonoBehaviour
{
    public BloodScreenEffect bloodEffect;
    [Range(0f, 1f)]
    public float pulseThreshold = 0.5f;      // Pulse when below 50% health
    public float pulseSpeed = 2f;            // Speed of the pulse
    public float pulseStrength = 0.2f;

    Health health;

    float baseIntensity;
    bool lowHealth;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Start()
    {
        health.OnHealthChanged += OnHealthChanged;
        OnHealthChanged(health.currentHealth);
    }

    void OnDestroy()
    {
        health.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float hp)
    {
        float percent = hp / health.maxHealth;

        // Base blood (no pulse)
        baseIntensity = 1f - percent;

        // Decide if pulse is allowed
        lowHealth = percent <= pulseThreshold;

        bloodEffect.ShowBlood(baseIntensity);
    }
    void Update()
    {
        if (!lowHealth)
            return;

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseStrength;
        bloodEffect.ShowBlood(Mathf.Clamp01(baseIntensity + pulse));
    }
}

