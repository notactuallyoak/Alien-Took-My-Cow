using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 8f;
    public float machSpeed = 10f;
    public float runSpeed = 16f;

    public float acceleration = 20f;
    public float deceleration = 25f;

    private float currentSpeed;
    private float targetSpeed;

    private bool isRunning;
    private float lastFacingDir = 1f;

    [Header("Jump")]
    public float jumpForce = 16f;
    public int extraJumpValue = 1;

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    [Header("Better Gravity")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Fast Fall")]
    public float fastFallMultiplier = 4f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    private bool isDashing;

    [Header("Ground Slam")]
    public float slamForce = 25f;
    private bool isSlamming;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool facingRight = true;
    private int extraJumps;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        extraJumps = extraJumpValue;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleJumpBuffer();
        HandleJumpLogic();
        HandleBetterGravity();

        // DASH
        if (Input.GetKeyDown(KeyCode.X) && !isDashing)
            StartCoroutine(Dash());

        // GROUND SLAM
        float inputY = Input.GetAxisRaw("Vertical");
        if (inputY < 0 && !isGrounded && !isSlamming)
            StartSlam();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ================= MOVEMENT =================

    void HandleMovement()
    {
        // prevent
        if (isDashing || isSlamming)
            return;

        float inputX = Input.GetAxisRaw("Horizontal");

        // save last facing dir
        if (inputX != 0)
        {
            lastFacingDir = Mathf.Sign(inputX);
            if ((lastFacingDir > 0 && !facingRight) ||
                (lastFacingDir < 0 && facingRight))
            {
                Flip();
            }
        }

        // Shift = start acceleration
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        // set target speed
        if (isRunning)
        {
            if (currentSpeed < machSpeed)
                targetSpeed = machSpeed;
            else
                targetSpeed = runSpeed;
        }
        else
        {
            targetSpeed = Mathf.Abs(inputX) > 0 ? walkSpeed : 0f;
        }

        // smooth acceleration / smooth deceleration
        if (currentSpeed < targetSpeed)
            currentSpeed += acceleration * Time.fixedDeltaTime;
        else
            currentSpeed -= deceleration * Time.fixedDeltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, runSpeed);

        // if running mode -> use lastFacingDir
        float moveDir = isRunning ? lastFacingDir : inputX;

        rb.linearVelocity = new Vector2(moveDir * currentSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ================= GROUND =================

    void HandleGroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            whatIsGround
        );

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            extraJumps = extraJumpValue;

            if (isSlamming)
            {
                isSlamming = false;
                // camshake
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    // ================= JUMP BUFFER =================

    void HandleJumpBuffer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    // ================= JUMP LOGIC =================

    void HandleJumpLogic()
    {
        if (jumpBufferCounter > 0)
        {
            // Normal jump (Coyote Time)
            if (coyoteTimeCounter > 0)
            {
                Jump();
                jumpBufferCounter = 0;
            }
            // Air jump
            else if (extraJumps > 0)
            {
                Jump();
                extraJumps--;
                jumpBufferCounter = 0;
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0;
    }

    // ================= BETTER GRAVITY + FAST FALL =================

    void HandleBetterGravity()
    {
        // Falling
        if (rb.linearVelocity.y < 0)
        {
            float multiplier = Input.GetKey(KeyCode.S) ? fastFallMultiplier : fallMultiplier;

            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1) * Time.deltaTime;
        }
        // Short hop
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
    IEnumerator Dash()
    {
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        rb.linearVelocity = new Vector2(lastFacingDir * dashForce, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    void StartSlam()
    {
        isSlamming = true;
        rb.linearVelocity = new Vector2(0, -slamForce);
    }

    // ================= DEBUG =================

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}