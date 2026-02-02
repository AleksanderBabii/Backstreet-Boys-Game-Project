using UnityEngine;
using TMPro;

public class FeedPromptUI : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float fadeSpeed = 6f;

    float targetAlpha = 0f;

    void Update()
    {
        Color c = text.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        text.color = c;
    }

    public void Show()
    {
        targetAlpha = 1f;
    }

    public void Hide()
    {
        targetAlpha = 0f;
    }
}
