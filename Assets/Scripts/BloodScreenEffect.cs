using UnityEngine;
using UnityEngine.UI;

public class BloodScreenEffect : MonoBehaviour
{
    public Image bloodImage;
    public float fadeSpeed = 2f;

    float targetAlpha = 0f;

    void Update()
    {
        Color c = bloodImage.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        bloodImage.color = c;
    }

    public void ShowBlood(float intensity)
    {
        targetAlpha = Mathf.Clamp01(intensity);
    }

    public void ClearBlood()
    {
        targetAlpha = 0f;
    }
}
