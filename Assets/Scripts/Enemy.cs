using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool startFacingRight = true;
    [SerializeField] private float flipCooldown = 0.2f;

    [Header("Collision Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.5f);
    [SerializeField] private Transform ledgeCheckPoint;

    [Header("Stomp Settings")]
    [SerializeField] private float stompBounceForce = 10f;

    [Header("Stuck Detection")]
    [SerializeField] private float stuckThreshold = 0.1f;      // Time in seconds before considering stuck
    [SerializeField] private float unstuckHopForce = 1f;       // Small upward nudge

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Rigidbody2D rb;
    private Collider2D mainCollider;
    private bool isFacingRight;
    private bool isDead = false;
    private float lastFlipTime = -1f;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();

        // Zero-friction material to prevent sticking (but still may snag on seams)
        PhysicsMaterial2D frictionlessMaterial = new PhysicsMaterial2D("EnemyFrictionless");
        frictionlessMaterial.friction = 0f;
        frictionlessMaterial.bounciness = 0f;
        mainCollider.sharedMaterial = frictionlessMaterial;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        isFacingRight = startFacingRight;
        UpdateFacing();

        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Ground check
        bool isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);

        // Wall & Ledge checks (only when grounded)
        bool hitWall = isGrounded && Physics2D.OverlapBox(wallCheckPoint.position, wallCheckSize, 0f, groundLayer);
        bool ledgeAhead = ledgeCheckPoint != null && Physics2D.OverlapBox(ledgeCheckPoint.position, groundCheckSize, 0f, groundLayer);

        // Flip logic
        bool shouldFlip = (hitWall || (!ledgeAhead && isGrounded)) && Time.time > lastFlipTime + flipCooldown;
        if (shouldFlip)
        {
            Flip();
        }

        // --- Stuck Detection ---
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        bool isStuck = distanceMoved < 0.001f && isGrounded && !hitWall && ledgeAhead;

        if (isStuck)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckThreshold)
            {
                Unstick();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        // Normal movement
        Vector2 movement = new Vector2((isFacingRight ? 1 : -1) * moveSpeed * Time.fixedDeltaTime, 0);
        Vector2 targetPosition = rb.position + movement;
        rb.MovePosition(targetPosition);

        lastPosition = transform.position;

        if (showDebugInfo)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.down * 0.5f, isGrounded ? Color.green : Color.red);
            Debug.DrawRay(transform.position, Vector3.right * (isFacingRight ? 1 : -1) * 0.5f, Color.blue);
            if (isStuck) Debug.Log($"{name} stuck for {stuckTimer:F2}s");
        }
    }

    private void Unstick()
    {
        Debug.Log($"{name} unsticking!");

        // Temporarily ignore collision with ground layer
        int enemyLayer = gameObject.layer;
        int groundLayerInt = (int)Mathf.Log(groundLayer.value, 2);
        Physics2D.IgnoreLayerCollision(enemyLayer, groundLayerInt, true);

        // Nudge forward and slightly up
        Vector2 nudge = new Vector2((isFacingRight ? 1 : -1) * 0.1f, unstuckHopForce);
        rb.position += nudge;

        // Re-enable collision after a short delay
        StartCoroutine(ReenableCollision(enemyLayer, groundLayerInt));
    }

    private System.Collections.IEnumerator ReenableCollision(int enemyLayer, int groundLayerInt)
    {
        yield return new WaitForSeconds(0.1f);
        Physics2D.IgnoreLayerCollision(enemyLayer, groundLayerInt, false);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        UpdateFacing();
        lastFlipTime = Time.time;
    }

    private void UpdateFacing()
    {
        Vector3 scale = transform.localScale;
        scale.x = isFacingRight ? 1 : -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player == null) return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    Stomped(player);
                    return;
                }
            }
            HurtPlayer(player);
        }
    }

    private void Stomped(PlayerController player)
    {
        isDead = true;
        Debug.Log("Enemy stomped!");
        mainCollider.enabled = false;
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, stompBounceForce);
        }
        Destroy(gameObject, 0.2f);
    }

    private void HurtPlayer(PlayerController player)
    {
        Debug.Log("Player hit by enemy!");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(wallCheckPoint.position, wallCheckSize);
        }
        if (ledgeCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(ledgeCheckPoint.position, groundCheckSize);
        }
    }
}