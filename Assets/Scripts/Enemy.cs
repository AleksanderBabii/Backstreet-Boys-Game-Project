using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float feedThreshold = 25f; // % health
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += health.Die;
    }

    public bool CanBeFedOn()
    {
        float healthPercent = health.currentHealth / health.maxHealth;
        return healthPercent <= (feedThreshold / 100f);
    }
}

