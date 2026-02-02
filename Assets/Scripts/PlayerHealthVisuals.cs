using UnityEngine;

public class PlayerHealthVisuals : MonoBehaviour
{
    public BloodScreenEffect bloodEffect;
    public float lowHealthPulseStrength = 0.5f;

    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Start()
    {
        health.OnHealthChanged += OnHealthChanged;
    }

    void OnDestroy()
    {
        health.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float hp)
    {
        float percent = hp / health.maxHealth;
        float intensity = 1f - percent;
        if (percent <= 0.3f)
            intensity += Mathf.Sin(Time.time * 4f) * lowHealthPulseStrength;

        bloodEffect.ShowBlood(Mathf.Clamp01(intensity));
    }
}

