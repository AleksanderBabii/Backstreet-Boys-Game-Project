using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    private PlayerMovementController player;

    void OnTriggerEnter(Collider other)
    {
        PlayerMovementController player =
            other.GetComponent<PlayerMovementController>();

        if (player != null)
        {
            player.ActivateSpeedBoost(player.boostMultiplier, player.boostDuration);
            Destroy(gameObject);
        }
    }
}
