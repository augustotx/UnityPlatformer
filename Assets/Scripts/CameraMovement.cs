using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerName = "Player";
    private Transform player;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.2f;          // Damping for smooth movement
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f); // Camera offset from player

    [Header("Vertical Constraints")]
    [SerializeField] private bool limitVerticalMovement = true;
    [SerializeField] private float verticalDeadzone = 1.5f;    // How far player can move vertically before camera follows
    [SerializeField] private float minY = -2f;                 // Optional absolute clamp (set high if not needed)
    [SerializeField] private float maxY = 5f;

    [Header("Horizontal Constraints (Optional)")]
    [SerializeField] private bool limitHorizontalMovement = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 100f;

    private Vector3 velocity = Vector3.zero;
    private float targetY; // Used for deadzone calculation

    private void Start()
    {
        GameObject playerObj = GameObject.Find(playerName);
        if (playerObj != null)
        {
            player = playerObj.transform;
            targetY = player.position.y + offset.y;
        }
        else
        {
            Debug.LogError($"CameraMovement: No GameObject named '{playerName}' found!");
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // Calculate desired position
        Vector3 targetPos = player.position + offset;

        // Vertical deadzone handling
        if (limitVerticalMovement)
        {
            float playerY = player.position.y;
            float currentTargetY = targetY;

            // If player exceeds deadzone above or below current target, move target Y
            if (playerY > currentTargetY + verticalDeadzone)
            {
                targetY = playerY - verticalDeadzone;
            }
            else if (playerY < currentTargetY - verticalDeadzone)
            {
                targetY = playerY + verticalDeadzone;
            }

            // Apply vertical limits
            targetY = Mathf.Clamp(targetY, minY, maxY);
            targetPos.y = targetY + offset.y;
        }
        else
        {
            // Simple follow without deadzone
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }

        // Horizontal limits
        if (limitHorizontalMovement)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        }

        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    // Optional: Reset target Y when teleporting or respawning
    public void ResetVerticalTarget()
    {
        if (player != null)
            targetY = player.position.y + offset.y;
    }

    // Visualize deadzone in editor (optional)
    private void OnDrawGizmosSelected()
    {
        if (!limitVerticalMovement || player == null) return;

        Gizmos.color = Color.yellow;
        Vector3 deadzoneCenter = new Vector3(transform.position.x, targetY, 0f);
        Gizmos.DrawWireCube(deadzoneCenter, new Vector3(1f, verticalDeadzone * 2f, 0f));
    }
}