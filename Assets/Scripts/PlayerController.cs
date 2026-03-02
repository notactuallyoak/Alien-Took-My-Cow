using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // movement variables
    [Header("Movement")]
    public float walkSpeed;
    public float machSpeed;
    public float runSpeed;
    public float acceleration;
    public float deceleration;

    private float currentSpeed;
    private float targetSpeed;
    private bool isRunning;
    private float lastFacingDir = 1f;

    [Header("Jump")]
    public float jumpForce;
    public int extraJumpValue;
    public float coyoteTime;
    public float jumpBufferTime;
    public float fallMultiplier;
    public float lowJumpMultiplier;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private int extraJumps;

    [Header("Dash")]
    public float dashForce;
    public float dashDuration;
    public float dashCooldown;

    private bool isDashing;
    private float dashCooldownTimer;

    [Header("Ground Slam")]
    public float slamForce;
    public float slamCooldown;

    private bool isSlamming;
    private float slamCooldownTimer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;

    private bool isGrounded;
    private bool facingRight = true;

    // hit variables 
    [Header("Knockback Settings")]
    public float knockbackForceX;
    public float knockbackForceY;
    public float hurtDuration;  // uncontrollable time after hit

    private bool isHurt = false;

    // Hash IDs for performance
    private int animSpeedHash;
    private int animIsGroundedHash;
    //private int animYVelocityHash;
    private int animJumpHash;
    private int animDashHash;
    private int animSlamHash;
    private int animSlamLandHash;

    private Animator anim;
    private Rigidbody2D rb;
    private GhostTrail ghostTrail;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        ghostTrail = GetComponent<GhostTrail>();

        extraJumps = extraJumpValue;

        animSpeedHash = Animator.StringToHash("Speed");
        animIsGroundedHash = Animator.StringToHash("IsGrounded");
        //animYVelocityHash = Animator.StringToHash("YVelocity");
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
        if (inputY < 0 && !isGrounded && !isSlamming && slamCooldownTimer <= 0f)
            StartSlam();

        HandleGhostEffects();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ================= KNOCKBACK =================

    // for other to called
    public void TakeDamage(Vector2 enemyPosition)
    {
        if (isHurt || isDashing || isSlamming) return;

        // cancel other action
        isDashing = false;
        isSlamming = false;
        StopAllCoroutines();

        // decrease points or time
        //GameManager.Instance.OnPlayerHit();

        StartCoroutine(PerformKnockback(enemyPosition));
    }

    private IEnumerator PerformKnockback(Vector2 hitSource)
    {
        isHurt = true;

        // calc knockback dir (away from enemy position)
        Vector2 knockbackDir = (transform.position - (Vector3)hitSource).normalized;

        if (knockbackDir == Vector2.zero)
        {
            knockbackDir = new Vector2(-lastFacingDir, 1).normalized;
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackDir.x * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        // anim.SetTrigger("Hurt");

        yield return new WaitForSeconds(hurtDuration);
        isHurt = false;
    }


    // ================= GHOST EFFECTS =================

    void HandleGhostEffects()
    {
        if (ghostTrail == null) return;

        if (isSlamming || (isDashing && !isHurt))
        {
            ghostTrail.SpawnGhost();
        }

        else if (isRunning && isGrounded && currentSpeed > machSpeed && !isHurt)
        {
            ghostTrail.SpawnGhost();
        }
    }


    // ================= TIMERS =================

    void HandleTimers()
    {
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
        //anim.SetFloat(animYVelocityHash, rb.linearVelocity.y);
    }

    // ================= MOVEMENT =================

    void HandleMovement()
    {
        if (isDashing || isSlamming || isHurt) return;

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
                anim.SetTrigger(animSlamLandHash);
                StartCoroutine(Wait(0.1f));
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
        anim.SetTrigger(animJumpHash);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0;
    }

    void HandleBetterGravity()
    {
        if (isHurt) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
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
        anim.SetTrigger(animDashHash);
        dashCooldownTimer = dashCooldown;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(lastFacingDir * dashForce, 0);

        float timer = 0f;
        while (timer < dashDuration)
        {
            if (ghostTrail != null) ghostTrail.SpawnGhost();

            // (Optional)
            // rb.linearVelocity = new Vector2(lastFacingDir * dashForce, 0); 

            timer += Time.deltaTime;
            yield return null;
        }
        // -------------------------------------------------------------------------

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    void StartSlam()
    {
        isSlamming = true;
        anim.SetTrigger(animSlamHash);
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

    // ================= COLLISION DETECTION =================
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(other.transform.position);
        }
        else if (other.gameObject.CompareTag("Hazard"))
        {
            TakeDamage(other.transform.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Breakable"))
        {
            if (isSlamming || isDashing || currentSpeed >= machSpeed)
            {
                Destroy(col.gameObject);
            }
        }
    }

    private IEnumerator Wait(float sec = 0.1f)
    {
        yield return new WaitForSeconds(sec);
    }
}