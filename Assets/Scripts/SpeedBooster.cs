using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    public float multiplier = 2f;
    public float duration = 5f;

    void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.ActivateSpeedBoost(multiplier, duration);
            Destroy(gameObject);
        }
    }
}
