using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool followPlayer = true;

    [Header("Parallax Settings")]
    [SerializeField] private float parallaxEffect = 0.5f;
    [SerializeField] private bool infiniteHorizontal = true;
    [SerializeField] private Vector2 offset = Vector2.zero;

    private float spriteWidth;
    private Vector3 initialPosition;
    private Transform clone;          // Reference to the duplicate sprite

    private void Start()
    {
        if (target == null && followPlayer)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogError("Parallax: Player not found! Assign target manually.");
        }

        if (target == null)
        {
            enabled = false;
            return;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            spriteWidth = sr.bounds.size.x;
        else
            spriteWidth = 10f;

        initialPosition = transform.position;

        // Create clone for seamless tiling
        if (infiniteHorizontal)
        {
            CreateClone();
        }
    }

    private void CreateClone()
    {
        GameObject cloneObj = new GameObject(gameObject.name + "_Clone");
        cloneObj.transform.SetParent(transform.parent); // Same parent for world-space consistency
        cloneObj.transform.position = transform.position + Vector3.right * spriteWidth;
        cloneObj.transform.localScale = transform.localScale;

        SpriteRenderer originalSr = GetComponent<SpriteRenderer>();
        SpriteRenderer cloneSr = cloneObj.AddComponent<SpriteRenderer>();
        cloneSr.sprite = originalSr.sprite;
        cloneSr.color = originalSr.color;
        cloneSr.sortingLayerID = originalSr.sortingLayerID;
        cloneSr.sortingOrder = originalSr.sortingOrder;
        cloneSr.flipX = originalSr.flipX;
        cloneSr.flipY = originalSr.flipY;

        clone = cloneObj.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate parallax position (same as before)
        Vector3 targetPos = target.position;
        float parallaxX = targetPos.x * (1f - parallaxEffect);
        float parallaxY = targetPos.y * (1f - parallaxEffect);
        Vector3 newPosition = new Vector3(parallaxX, parallaxY, initialPosition.z) + (Vector3)offset;

        // Apply position to main object
        transform.position = newPosition;

        // Handle clone for infinite tiling
        if (infiniteHorizontal && clone != null)
        {
            // Position clone exactly one sprite width to the right of main object
            clone.position = newPosition + Vector3.right * spriteWidth;

            // Get camera view bounds
            Camera cam = Camera.main;
            if (cam != null)
            {
                float camHalfHeight = cam.orthographicSize;
                float camHalfWidth = camHalfHeight * cam.aspect;
                float camLeft = cam.transform.position.x - camHalfWidth;
                float camRight = cam.transform.position.x + camHalfWidth;

                // Check if the main object is completely off-screen to the left
                if (transform.position.x + spriteWidth / 2f < camLeft)
                {
                    // Move main object to the right of the clone
                    transform.position += Vector3.right * (2f * spriteWidth);
                }
                // Check if the main object is completely off-screen to the right
                else if (transform.position.x - spriteWidth / 2f > camRight)
                {
                    // Move main object to the left of the clone
                    transform.position -= Vector3.right * (2f * spriteWidth);
                }

                // Do the same for the clone
                if (clone.position.x + spriteWidth / 2f < camLeft)
                {
                    clone.position += Vector3.right * (2f * spriteWidth);
                }
                else if (clone.position.x - spriteWidth / 2f > camRight)
                {
                    clone.position -= Vector3.right * (2f * spriteWidth);
                }
            }
        }
    }
}