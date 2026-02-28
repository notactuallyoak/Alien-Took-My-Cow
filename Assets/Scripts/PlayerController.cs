using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

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
    public float jumpForce = 18f;
    public int extraJumpValue = 1;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float fastFallMultiplier = 4f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private int extraJumps;

    [Header("Dash")]
    public float dashForce = 25f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 0.5f;

    private bool isDashing;
    private float dashCooldownTimer;

    [Header("Ground Slam")]
    public float slamForce = 25f;
    public float slamCooldown = 1.0f;

    private bool isSlamming;
    private float slamCooldownTimer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    private bool isGrounded;
    private bool facingRight = true;

    // Hash IDs
    private int animSpeedHash;
    private int animIsGroundedHash;
    private int animYVelocityHash;
    private int animJumpHash;
    private int animDashHash;
    private int animSlamHash;
    private int animSlamLandHash;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        extraJumps = extraJumpValue;

        animSpeedHash = Animator.StringToHash("Speed");
        animIsGroundedHash = Animator.StringToHash("IsGrounded");
        animYVelocityHash = Animator.StringToHash("YVelocity");
        animJumpHash = Animator.StringToHash("Jump");
        animDashHash = Animator.StringToHash("Dash");
        animSlamHash = Animator.StringToHash("Slam");
        animSlamLandHash = Animator.StringToHash("SlamLand");
    }

    void Update()
    {
        HandleTimers();

        HandleGroundCheck();
        HandleJumpBuffer();
        HandleJumpLogic();
        HandleBetterGravity();
        UpdateAnimations();

        // DASH INPUT
        if (Input.GetKeyDown(KeyCode.X) && !isDashing && dashCooldownTimer <= 0f)
            StartCoroutine(Dash());

        // GROUND SLAM INPUT
        float inputY = Input.GetAxisRaw("Vertical");
        if (inputY < 0 && !isGrounded && !isSlamming && !isDashing && slamCooldownTimer <= 0f)
            StartSlam();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ================= TIMERS =================

    void HandleTimers()
    {
        // ลดเวลา Timer ลงทีละนิดทุกเฟรม
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (slamCooldownTimer > 0f)
            slamCooldownTimer -= Time.deltaTime;
    }

    // ================= ANIMATION LOGIC =================

    void UpdateAnimations()
    {
        anim.SetFloat(animSpeedHash, currentSpeed);
        anim.SetBool(animIsGroundedHash, isGrounded);
        anim.SetFloat(animYVelocityHash, rb.linearVelocity.y);
    }

    void TriggerJumpAnim() => anim.SetTrigger(animJumpHash);
    void TriggerDashAnim() => anim.SetTrigger(animDashHash);
    void TriggerSlamAnim() => anim.SetTrigger(animSlamHash);
    void TriggerSlamLandAnim() => anim.SetTrigger(animSlamLandHash);

    // ================= MOVEMENT =================

    void HandleMovement()
    {
        if (isDashing || isSlamming) return;

        float inputX = Input.GetAxisRaw("Horizontal");

        if (inputX != 0)
        {
            lastFacingDir = Mathf.Sign(inputX);
            if ((lastFacingDir > 0 && !facingRight) || (lastFacingDir < 0 && facingRight))
                Flip();
        }

        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isRunning)
        {
            targetSpeed = (currentSpeed < machSpeed) ? machSpeed : runSpeed;
        }
        else
        {
            targetSpeed = Mathf.Abs(inputX) > 0 ? walkSpeed : 0f;
        }

        if (currentSpeed < targetSpeed)
            currentSpeed += acceleration * Time.fixedDeltaTime;
        else
            currentSpeed -= deceleration * Time.fixedDeltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, runSpeed);

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

    // ================= GROUND & JUMP =================

    void HandleGroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            extraJumps = extraJumpValue;

            if (isSlamming)
            {
                isSlamming = false;
                TriggerSlamLandAnim();
                // cd timer start when slam hits the ground
                slamCooldownTimer = slamCooldown;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void HandleJumpBuffer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    void HandleJumpLogic()
    {
        if (jumpBufferCounter > 0)
        {
            if (coyoteTimeCounter > 0)
            {
                Jump();
                jumpBufferCounter = 0;
            }
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
        TriggerJumpAnim();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0;
    }

    void HandleBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            float multiplier = Input.GetKey(KeyCode.S) ? fastFallMultiplier : fallMultiplier;
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    // ================= DASH & SLAM =================

    IEnumerator Dash()
    {
        isDashing = true;
        TriggerDashAnim();

        //  cd timer start when dash starts  
        dashCooldownTimer = dashCooldown;

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
        TriggerSlamAnim();
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