using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector] public bool isRunning;
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

    [HideInInspector] public bool isDashing;
    private float dashCooldownTimer;

    [Header("Ground Slam")]
    public float slamForce;
    public float slamCooldown;

    [HideInInspector] public bool isSlamming;
    private float slamCooldownTimer;

    [Header("Ground Check")]
    public Vector2 groundCheckOffset;
    public float checkRadius;
    public LayerMask whatIsGround;
    private Vector2 GroundCheckPosition => (Vector2)transform.position + groundCheckOffset;

    [Header("Air Control")]
    public float airAcceleration;
    public float airDeceleration;

    [Header("Combat Settings")]
    public LayerMask whatIsDamageable;
    public float runAttackDistance;
    public Vector2 runHitboxSize;
    public float dashAttackDistance;
    public Vector2 dashHitboxSize;
    public int rayCount;
    public float spreadAngle;
    public float rayDistance;
    public float slamSmallRadius;
    public float slamBigRadius;

    [Header("Damage Values")]
    private int runDamage = 1;
    private int dashDamage = 2;
    private int jumpDamage = 1;
    private int slamDamage = 3;

    private float runAttackTimer;
    private List<Collider2D> dashingHitEnemies = new List<Collider2D>(); // Track hits during dash

    private bool isGrounded;
    [HideInInspector] public bool facingRight = true;
    [HideInInspector] public bool isHurt = false;

    private float runParticleInterval = 0.2f;
    private float runParticleTimer;
    private bool wasHurt;
    private bool wasGrounded;
    private bool wasRunning;
    private bool wasSlamming;

    // Hash IDs for performance
    private int animSpeedHash;
    private int animIsGroundedHash;
    private int animJumpHash;
    private int animDashHash;
    private int animSlamHash;
    private int animSlamLandHash;

    private Animator anim;
    private Rigidbody2D rb;
    private GhostTrail ghostTrail;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        ghostTrail = GetComponent<GhostTrail>();

        extraJumps = extraJumpValue;

        animSpeedHash = Animator.StringToHash("Speed");
        animIsGroundedHash = Animator.StringToHash("IsGrounded");
        animJumpHash = Animator.StringToHash("Jump");
        animDashHash = Animator.StringToHash("Dash");
        animSlamHash = Animator.StringToHash("Slam");
        animSlamLandHash = Animator.StringToHash("SlamLand");
    }

    private void Update()
    {
        HandleTimers();
        HandleGroundCheck();
        HandleParticles();
        HandleJumpBuffer();
        HandleJumpLogic();
        HandleBetterGravity();

        // dash input
        if (Input.GetKeyDown(KeyCode.X) && !isDashing && dashCooldownTimer <= 0f)
            StartCoroutine(Dash());

        // slam input
        float inputY = Input.GetAxisRaw("Vertical");
        if (inputY < 0 && !isGrounded && !isSlamming && slamCooldownTimer <= 0f)
            StartSlam();

        HandleGhostEffects();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRunCombat();

        if (isSlamming) HandleSlamCombat();
    }

    private void TryApplyDamage(Collider2D hitCollider, int damage)
    {
        EnemyBase enemy = hitCollider.GetComponentInParent<EnemyBase>();
        if (enemy != null) enemy.TakeDamage(damage);

        Breakable breakable = hitCollider.GetComponent<Breakable>();
        if (breakable != null) breakable.TakeDamage();
    }

    private void HandleRunCombat()
    {
        // only attack if moving at max speed
        if (currentSpeed >= runSpeed && isGrounded && !isHurt)
        {
            if (runAttackTimer <= 0)
            {
                Vector2 origin = (Vector2)transform.position + Vector2.right * lastFacingDir * (runAttackDistance / 2);
                RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, runHitboxSize, 0f, Vector2.right * lastFacingDir, runAttackDistance, whatIsDamageable);

                foreach (var hit in hits)
                {
                    if (hit.collider != null)
                    {
                        TryApplyDamage(hit.collider, runDamage);
                    }
                }
                runAttackTimer = 0.015f; // hit interval
            }
            else
            {
                runAttackTimer -= Time.fixedDeltaTime;
            }
        }
    }

    private void HandleDashCombat()
    {
        Vector2 origin = (Vector2)transform.position;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, dashHitboxSize, 0f, Vector2.right * lastFacingDir, dashAttackDistance, whatIsDamageable);

        foreach (var hit in hits)
        {
            if (!dashingHitEnemies.Contains(hit.collider))
            {
                TryApplyDamage(hit.collider, dashDamage);
                dashingHitEnemies.Add(hit.collider);
            }
        }
    }

    private void HandleJumpCombat()
    {
        float startAngle = -spreadAngle / 2;
        float angleStep = spreadAngle / (rayCount - 1); 
        Vector2 origin = GroundCheckPosition;

        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * Vector2.down;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayDistance, whatIsDamageable);

            if (hit.collider != null)
            {
                TryApplyDamage(hit.collider, jumpDamage);
            }
        }
    }

    private void HandleSlamCombat()
    {
        // Small circle constant damage while falling
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamSmallRadius, whatIsDamageable);
        foreach (var hit in hits)
        {
            TryApplyDamage(hit, runDamage);
        }
    }

    // Called when slam lands
    private void HandleSlamLandCombat()
    {
        // Big circle impact
        Collider2D[] hits = Physics2D.OverlapCircleAll(GroundCheckPosition, slamBigRadius, whatIsDamageable);
        foreach (var hit in hits)
        {
            Vector2 dir = (hit.transform.position - transform.position).normalized;
            TryApplyDamage(hit, slamDamage);
        }
    }

    private void HandleGhostEffects()
    {
        if (ghostTrail == null) return;

        if (isSlamming) ghostTrail.SpawnGhost(0.05f, 0.15f);
        else if (isRunning && isGrounded && currentSpeed > machSpeed && !isHurt) ghostTrail.SpawnGhost(0.2f, 0.5f);
    }

    private void HandleTimers()
    {
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
        if (slamCooldownTimer > 0f) slamCooldownTimer -= Time.deltaTime;
        if (runParticleTimer > 0f) runParticleTimer -= Time.deltaTime;
    }

    private void UpdateAnimations()
    {
        anim.SetFloat(animSpeedHash, currentSpeed);
        anim.SetBool(animIsGroundedHash, isGrounded);
    }

    private void HandleParticles()
    {
        if (isHurt && !wasHurt)
        {
            ParticleEmitter.Instance.Emit("WhiteFlash", transform.position, Quaternion.identity);
        }
        wasHurt = isHurt;

        if (isGrounded && !wasGrounded && !isSlamming)
        {
            ParticleEmitter.Instance.Emit("LittleSmoke", GroundCheckPosition, Quaternion.identity);
            AudioManager.Instance.PlaySFX("PlayerWalk");
        }
        wasGrounded = isGrounded;

        if (isRunning && !wasRunning && isGrounded)
        {
            ParticleEmitter.Instance.Emit("PreRun", GroundCheckPosition, !facingRight);
        }
        wasRunning = isRunning;

        if (isGrounded && !isHurt && currentSpeed > walkSpeed)
        {
            if (runParticleTimer <= 0f)
            {
                if (currentSpeed > walkSpeed && currentSpeed < machSpeed)
                {
                    ParticleEmitter.Instance.Emit("LittleSmoke", GroundCheckPosition, Quaternion.identity);
                    AudioManager.Instance.PlaySFX("PlayerWalk");
                }
                else if (currentSpeed >= machSpeed)
                {
                    ParticleEmitter.Instance.Emit("Mach", GroundCheckPosition, !facingRight);
                }
                else if (isRunning)
                {
                    ParticleEmitter.Instance.Emit("Run", GroundCheckPosition, !facingRight);
                }
                runParticleTimer = runParticleInterval;
            }
        }
    }

    private void HandleMovement()
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

        float moveDir = isRunning ? lastFacingDir : inputX;

        if (isGrounded)
        {
            // Ground: full control with normal acceleration/deceleration
            if (currentSpeed < targetSpeed)
            {
                currentSpeed += acceleration * Time.fixedDeltaTime;
            }
            else
            {
                currentSpeed -= deceleration * Time.fixedDeltaTime;
            }
            currentSpeed = Mathf.Clamp(currentSpeed, 0, runSpeed);
            rb.linearVelocity = new Vector2(moveDir * currentSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Air: preserve momentum!
            if (inputX != 0)
            {
                // Air control - can change direction but with less authority
                float targetVelocity = moveDir * Mathf.Max(targetSpeed, currentSpeed);
                rb.linearVelocity = new Vector2(
                    Mathf.MoveTowards(rb.linearVelocity.x, targetVelocity, airAcceleration * Time.fixedDeltaTime),
                    rb.linearVelocity.y
                );
                currentSpeed = Mathf.Abs(rb.linearVelocity.x);
            }
            // If no input in air, DON'T touch horizontal velocity - let momentum carry!
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void HandleGroundCheck()
    {
        wasSlamming = isSlamming;
        isGrounded = Physics2D.OverlapCircle(GroundCheckPosition, checkRadius, whatIsGround);

        if (isGrounded)
        {
            // sync currentSpeed with velocity on landing
            currentSpeed = Mathf.Abs(rb.linearVelocity.x);

            coyoteTimeCounter = coyoteTime;
            extraJumps = extraJumpValue;

            if (isSlamming)
            {
                isSlamming = false;

                anim.SetTrigger(animSlamLandHash);
                CameraController.Instance?.CamShake(0.15f, 0.2f);

                HandleSlamLandCombat();

                ParticleEmitter.Instance.Emit("SlamLand", GroundCheckPosition, !facingRight);
                AudioManager.Instance.PlaySFX("PlayerSlamLand");

                slamCooldownTimer = slamCooldown;   // cd timer start when slam hits the ground
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleJumpBuffer()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleJumpLogic()
    {
        if (isHurt) return;

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

    private void Jump()
    {
        anim.SetTrigger(animJumpHash);
        AudioManager.Instance.PlaySFX("PlayerJump");
        CameraController.Instance?.CamShake(0.15f, 0.15f);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0;

        ParticleEmitter.Instance.Emit("MuzzleFlash", GroundCheckPosition, !facingRight);
        ParticleEmitter.Instance.Emit("ShotgunFire", GroundCheckPosition, !facingRight);
        AudioManager.Instance.PlaySFX("PlayerShotgun");

        HandleJumpCombat();
    }

    private void HandleBetterGravity()
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

    private IEnumerator Dash()
    {
        isDashing = true;
        anim.SetTrigger(animDashHash);
        dashCooldownTimer = dashCooldown;

        ParticleEmitter.Instance.Emit("DashSmoke", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("PlayerDash");

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(lastFacingDir * dashForce, 0);

        dashingHitEnemies.Clear();

        float timer = 0f;
        while (timer < dashDuration)
        {
            if (ghostTrail != null) ghostTrail.SpawnGhost(0.05f, 0.1f);

            HandleDashCombat();

            timer += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    private void StartSlam()
    {
        isSlamming = true;
        anim.SetTrigger(animSlamHash);

        ParticleEmitter.Instance.Emit("LittleSmoke", transform.position, !facingRight);
        AudioManager.Instance.PlaySFX("PlayerSlam");

        rb.linearVelocity = new Vector2(0, -slamForce);
    }

    // hurt
    public void CancelActions()
    {
        isDashing = false;
        isSlamming = false;
        isRunning = false;

        if (rb != null) rb.gravityScale = 4; // reset gravity in case player got hurt during dash

        StopAllCoroutines();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(GroundCheckPosition, checkRadius);

        // COMBAT GIZMO   
        // run hitbox
        Gizmos.color = Color.red;
        Vector2 runOrigin = (Vector2)transform.position + Vector2.right * lastFacingDir * (runAttackDistance / 2);
        Gizmos.DrawWireCube(runOrigin, runHitboxSize);

        // dash hitbox
        Gizmos.color = Color.yellow;
        Vector2 dashCenter = (Vector2)transform.position + Vector2.right * lastFacingDir * (dashAttackDistance / 2);
        Gizmos.DrawWireCube(dashCenter, dashHitboxSize);

        // shotgun hitbox
        Gizmos.color = Color.green;
        float startAngle = -spreadAngle / 2;
        float angleStep = spreadAngle / (rayCount - 1);

        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * Vector2.down;
            Gizmos.DrawRay(GroundCheckPosition, dir * rayDistance);
        }

        // slam impact hitbox
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(GroundCheckPosition, slamBigRadius);

        // slam falling hitbox
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slamSmallRadius);
    }
}