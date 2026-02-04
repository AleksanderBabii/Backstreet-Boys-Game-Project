using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public float maxWidth;
    public float height;
    private float maxHealthValue = 100f;
    private float currentHealthValue = 100f;

    [SerializeField]
    private RectTransform healthBarTransform;
    
    public BloodScreenEffect bloodEffect;

    void Update()
    {
        float healthPercent = currentHealthValue / maxHealthValue;

        //add pulsing effect to blood screen when health is low
        if (healthPercent <= 0.3f)
        {
            if (bloodEffect != null)
            {
                // Intensity increases as health gets lower
                float intensity = Mathf.Lerp(1f, 0.8f, healthPercent / 0.3f);
                bloodEffect.ShowBlood(intensity);
            }
        }
        else
        {
            if (bloodEffect != null)
            {
                bloodEffect.ClearBlood(); // Clear blood effect when health is above 30%
            }
        }
    }
    public void SetMaxHealth(float maxHealth)
    {
        maxHealthValue = maxHealth;
        healthBarTransform.sizeDelta = new Vector2(maxWidth, height);
    }

    public void SetHealth(float currentHealth)
    {
        currentHealthValue = currentHealth;

        // Calculate health percentage and update health bar size
        float healthPercent = Mathf.Clamp01(currentHealthValue / maxHealthValue);
        healthBarTransform.sizeDelta = new Vector2(maxWidth * healthPercent, height);
        Debug.Log($"Health updated: {currentHealthValue}/{maxHealthValue}");
    } 
}
