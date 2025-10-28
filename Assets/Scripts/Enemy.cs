using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("References and Tuning")]
    private Transform player;                 // Cached reference to the player's Transform
    public float chaseSpeed = 2f;             // Horizontal chase speed while grounded
    public float jumpForce = 2f;              // Jump impulse strength
    public LayerMask groundLayer;             // Which layers count as "ground" for raycasts

    [Header("Physics and State")]
    private Rigidbody2D rb;                   // Rigidbody for movement / forces
    private Collider2D col;                   // Collider used to detect layer contacts
    private bool isGrounded;                  // Are we currently on the ground?
    private bool shouldJump;                  // Flag to jump on next FixedUpdate (when grounded)

    [Header("Combat")]
    public int damage = 1;                    // Damage this enemy would deal (usage external)
    public int maxHealth = 3;                 // Maximum health
    private int currentHealth;                // Current health

    [Header("Visuals")]
    private SpriteRenderer spriteRenderer;    // For flashing on hit
    private Color ogColor;                    // Original sprite color to restore after flash

    [Header("Kill Zone")]
    [SerializeField] private LayerMask neitherLayer; // If touching this, enemy dies immediately

    // ------------------------------------------------------------------------

    private void Start()
    {
        // Cache components and initial state
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform; // Assumes a single object tagged "Player"
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        ogColor = spriteRenderer.color;
    }

    private void Update()
    {
        // Do not let touch "Neither" yer
        if (col && col.IsTouchingLayers(neitherLayer))
        {
            Die();
            return;
        }

        // Check if grounded via downward raycast
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);

        // Horizontal direction toward the player
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // Check if the player is above 
        bool isPlayerAbove = Physics2D.Raycast(
            transform.position,
            Vector2.up,
            5f,
            1 << player.gameObject.layer // Build a mask from the player's layer index
        );

        if (isGrounded)
        {
            // Chase the player horizontally; preserve current vertical velocity
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);

            // Probe the environment for jump decisions:
            // Check if ground in front
            RaycastHit2D groundInFront = Physics2D.Raycast(
                transform.position,
                new Vector2(direction, 0f),
                2f,
                groundLayer
            );

            // Check if gap ahead
            RaycastHit2D gapAhead = Physics2D.Raycast(
                transform.position + new Vector3(direction, 0f, 0f),
                Vector2.down,
                2f,
                groundLayer
            );

            // Check if platform to jump to
            RaycastHit2D platformAbove = Physics2D.Raycast(
                transform.position,
                Vector2.up,
                5f,
                groundLayer
            );

            // Jump if there's no ground ahead or if the player is above and a platform exists above
            if (!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        // Execute the jump in FixedUpdate (physics step) for consistent forces
        if (isGrounded && shouldJump)
        {
            shouldJump = false;

            // Compute a jump that biases toward the player's horizontal position
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 jumpDirection = direction * jumpForce;

            // Apply an impulse with horizontal bias plus a guaranteed upward component
            rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
            Die();
    }

    public void DieAfterAttack() => Die();


    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = ogColor;
    }


    private void Die()
    {
        Destroy(gameObject);
    }
}
