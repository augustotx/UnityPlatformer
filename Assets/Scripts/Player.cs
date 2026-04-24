using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 

public class PlayerController : MonoBehaviour
{

    [Header("Game Logic")]
    public int coinsToCollect = 20;
    [SerializeField] private int lives = 3;
    [SerializeField] private int points = 0;
    private float invincibleTimer = 0f;
    private bool isInvincible = false;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float acceleration = 15f;     // How fast the player changes direction
    [SerializeField] private float deceleration = 10f;     // How fast the player stops

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;


    [Header("UI")]
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI lifeText;


    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;
    private bool jumpPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Prevent the player from rotating when colliding
        rb.freezeRotation = true;
    }

    private void Start()
    {
        pointText.text = "0";
        lifeText.text = $"{lives}";
    }

    private void Update()
    {
        if (coinsToCollect <= 0) EndLevel();
        if (this.transform.position.y <= -7.6f) Die();
        if (isInvincible) invincibleTimer += Time.deltaTime;
        if (invincibleTimer >= 3f)
        {
            isInvincible = false;
            invincibleTimer = 0f;
        }
        // Read input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        // Jump (W key)
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            jumpPressed = true;
        }

        // Shoot (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        // Update animator parameters
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        // Ground check
        isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);

        // Apply horizontal movement
        MoveHorizontal();

        // Apply jump
        if (jumpPressed)
        {
            Jump();
            jumpPressed = false;
        }
    }

    private void MoveHorizontal()
    {
        // Target velocity based on input
        float targetSpeed = horizontalInput * moveSpeed;

        // Calculate acceleration/deceleration
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = speedDiff * accelRate;

        // Apply force
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Jump()
    {
        // Reset vertical velocity for consistent jump height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void Shoot()
    {
        // Placeholder: logs to console
        Debug.Log("Shoot! (Projectile logic coming soon)");
        // You can add instantiation of a projectile prefab here later.
    }

    private void UpdateAnimation()
    {
        animator.SetBool("isMoving", Mathf.Abs(horizontalInput) > 0.01f);

        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1f, 1f);
        }
    }

    // Visualize ground check in editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
    }


    public void addLife(int n)
    {
        lives += n;
        lifeText.text = $"{lives}";
    }
    public void addPoints(int n)
    {
        points += n;
        pointText.text = $"{points}";

        if (points % 100 == 0) addLife(1);
    }

    public void removeLife(int n)
    {
        if(isInvincible) return;
        isInvincible = true;
        lives -= n;
        lifeText.text = $"{lives}";

        if (lives <= 0) Die();
    }

    public void Die()
    {
        Debug.Log("Player has zero lives!");
        SceneManager.LoadScene("Defeat");
    }

    private void EndLevel()
    {
        Scene curScene = SceneManager.GetActiveScene();
        if (curScene.name == "Lvl1") SceneManager.LoadScene("Lvl2");
        if (curScene.name == "Lvl2") SceneManager.LoadScene("Victory");
    }
}
