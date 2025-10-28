using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;                 // Rigidbody controlling motion
    bool isFacingRight = true;             // Sprite facing direction
    public float moveSpeed = 5f;           // Horizontal speed
    float horizontalMovement;              // Cached X input
    int jumpsRemaining;                    // Jumps left before touching ground

    [Header("Jumping")]
    public float jumpPower = 10f;          // Upward velocity applied on jump
    public int maxJumps = 2;               // Max allowed jumps (e.g., double jump)

    [Header("Ground Check")]
    public Transform groundCheckPos;                       // Overlap box center
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f); // Overlap box size
    public LayerMask groundLayer;                          // What counts as ground
    bool isGrounded;                                       // Grounded state

    [Header("Wall Check")]
    public Transform wallCheckPos;                         // Overlap box center
    public Vector2 wallCheckSize = new Vector2(0.49f, 0.03f); // Overlap box size
    public LayerMask wallLayer;                            // What counts as wall

    [Header("Wall Movement")]
    public float wallSlideSpeed = 2;       // Cap for downward speed while sliding
    bool isWallSliding;                    // Are we currently sliding on a wall?

    [Header("Wall Jumping")]
    bool isWallJumping;                    // Lock horizontal control briefly after wall jump
    float wallJumpDirection;               // +1 or -1 away from wall
    float wallJumpTime = 0.5f;             // Duration to keep wall-jump state
    float wallJumpTimer;                   // Countdown for wall-jump window
    public Vector2 wallJumpPower = new Vector2(5f, 10f); // X/Y power applied on wall jump

    [Header("Gravity")]
    public float baseGravity = 2f;         // Default gravity scale
    public float maxFallSpeed = 18f;       // Terminal velocity clamp
    public float fallSpeedMultiplier = 2f; // Extra gravity when falling

    // ------------------------------------------------------------------------

    private void Awake()
    {
        // Apply chosen ability tweaks from PlayerPrefs
        int ability = PlayerPrefs.GetInt("ability", 0);

        if (ability == 2) // 2 = SlowFalling
        {
            baseGravity *= 0.7f;
            fallSpeedMultiplier *= 0.6f;
            maxFallSpeed *= 0.6f;
        }
        else if (ability == 3) // 3 = SuperJump
        {
            jumpPower *= 1.4f; // Increase jump height
        }
    }

    private void Update()
    {
        GroundCheck();
        Gravity();
        ProcessWallSlide();
        ProcessWallJump();

        // Apply horizontal velocity & flip if not in "locked" wall jump state
        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
            Flip();
        }
    }

    public void Gravity()
    {
        if (rb.velocity.y < 0f)
        {
            // Falling: increase gravity and clamp velocity
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            // Rising or stationary: use base gravity
            rb.gravityScale = baseGravity;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // Ground/air jump
        if (context.performed && jumpsRemaining > 0)
        {
            if (context.performed)
            {
                // Full jump on performed
                rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                jumpsRemaining--;
            }
            else if (context.canceled)
            {
                // Early release for shorter jump
                if (rb.velocity.y > 0f)
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }

        // Wall jump
        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0f;

            // Force facing away from the wall
            if (transform.localScale.x != wallJumpDirection)
            {
                isFacingRight = !isFacingRight;
                var ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            // Keep wall-jump state slightly longer than the timer to prevent immediate re-input
            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);
        }
    }

    private void GroundCheck()
    {
        // Box overlap to detect ground contact
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0f, wallLayer);
    }

    private void ProcessWallSlide()
    {
        // Not grounded, touching wall, and attempting horizontal movement
        if (!isGrounded && WallCheck() && horizontalMovement != 0f)
        {
            isWallSliding = true;
            // Cap fall rate while sliding
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x; // Away from wall
            wallJumpTimer = wallJumpTime;

            // Ensure we don't cancel the just-primed window
            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if ((isFacingRight && horizontalMovement < 0f) || (!isFacingRight && horizontalMovement > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}
