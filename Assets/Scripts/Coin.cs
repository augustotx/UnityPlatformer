using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int points = 1;

    // Use trigger – no physical collision
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.addPoints(points);
                player.coinsToCollect -= 1;
                Destroy(gameObject);
            }
        }
    }
}