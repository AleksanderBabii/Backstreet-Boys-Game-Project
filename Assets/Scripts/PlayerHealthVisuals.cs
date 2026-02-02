using UnityEngine;

public class PlayerHealthVisuals : MonoBehaviour
{
    public BloodScreenEffect bloodEffect;

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
        float intensity = 1f - (hp / health.maxHealth);
        bloodEffect.ShowBlood(intensity);
    }
}

