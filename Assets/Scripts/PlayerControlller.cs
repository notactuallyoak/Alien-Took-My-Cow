using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Idle, Walking, MachRun, Mach3, Skidding, Dashing, Slamming }

    [Header("Current State")]
    public PlayerState currentState = PlayerState.Idle;

    [Header("Speed Thresholds")]
    public float walkSpeed = 10f;
    public float mach2Speed = 16f;
    public float mach3Speed = 24f; // High speed!
    public float acceleration = 12f;

    [Header("Forces")]
    public float jumpForce = 16f;
    public float dashForce = 25f;
    public float slamForce = -35f;

    [Header("Shotgun Jump")]
    public float shotgunJumpForce = 14f;
    public int maxShotgunJumps = 2;
    private int jumpsRemaining;

    private Rigidbody2D rb;
    private float inputX;
    private float inputY;
    private float currentSpeed;
    private float facingDir = 1f;
    private bool isGrounded;

    private LayerMask groundLayer;

    void Start()
    {
         rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        isGrounded = CheckGrounded();

        // Reset jumps when landing
        if (isGrounded)
        {
            jumpsRemaining = maxShotgunJumps;
        }

        if (inputX != 0) facingDir = Mathf.Sign(inputX);

        // Inputs
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                HandleNormalJump();
            }
            else if (jumpsRemaining > 0)
            {
                HandleShotgunJump();
            }
        }

        if (Input.GetKeyDown(KeyCode.X)) SetState(PlayerState.Dashing);

        if (Input.GetKeyDown(KeyCode.LeftControl) && !isGrounded)
        {
            SetState(PlayerState.Slamming);
        }

        LogicUpdate();
    }

    void HandleNormalJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void HandleShotgunJump()
    {
        jumpsRemaining--;

        // Apply the blast force upward
        // Override velocity.y so the blast feels consistent
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, shotgunJumpForce);
        GameManager.Instance.CamShake(0.15f, 0.4f);

        // Feedback (Optional)
        // You could instantiate a muzzle flash particle here:
        // Instantiate(shotgunBlastPrefab, transform.position + Vector3.down, Quaternion.identity);
    }

    void LogicUpdate()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                currentSpeed = 0;
                if (inputX != 0) SetState(PlayerState.Walking);
                break;

            case PlayerState.Walking:
                currentSpeed = walkSpeed;
                if (inputX == 0) SetState(PlayerState.Idle);
                if (Input.GetKey(KeyCode.LeftShift)) SetState(PlayerState.MachRun);
                break;

            case PlayerState.MachRun:
                currentSpeed = Mathf.MoveTowards(currentSpeed, mach3Speed, acceleration * Time.deltaTime);

                // Transition to Mach 3 automatically based on speed
                if (currentSpeed >= mach3Speed - 1f) SetState(PlayerState.Mach3);

                if (inputX == 0 || !Input.GetKey(KeyCode.LeftShift)) SetState(PlayerState.Walking);
                if (inputX != 0 && Mathf.Sign(inputX) != facingDir) SetState(PlayerState.Skidding);
                break;

            case PlayerState.Mach3:
                currentSpeed = mach3Speed; // Stay at max speed
                if (inputX == 0 || !Input.GetKey(KeyCode.LeftShift)) SetState(PlayerState.Walking);
                if (inputX != 0 && Mathf.Sign(inputX) != facingDir) SetState(PlayerState.Skidding);
                break;

            case PlayerState.Skidding:
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, acceleration * 3 * Time.deltaTime);
                if (currentSpeed <= 0.1f) SetState(PlayerState.Idle);
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == PlayerState.Dashing || currentState == PlayerState.Slamming) return;
        rb.linearVelocity = new Vector2(inputX * currentSpeed, rb.linearVelocity.y);

        if (inputX != 0 && currentState != PlayerState.Skidding)
            transform.localScale = new Vector3(facingDir, 1, 1);
    }

    public void SetState(PlayerState newState)
    {
        if (currentState == newState) return;

        // Exit Logic (Runs when leaving the current state)
        StopAllCoroutines();
        rb.gravityScale = 4;

        currentState = newState;

        // Enter Logic (Runs when entering the new state)
        switch (newState)
        {
            case PlayerState.Dashing:
                StartCoroutine(DashRoutine());
                break;
            case PlayerState.Slamming:
                rb.linearVelocity = new Vector2(0, slamForce);
                break;
            case PlayerState.Mach3:
                GameManager.Instance.CamShake(0.1f, 0.2f);
                break;
        }
    }

    System.Collections.IEnumerator DashRoutine()
    {
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(facingDir * dashForce, 0);
        yield return new WaitForSeconds(0.25f);
        SetState(PlayerState.Walking);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (currentState == PlayerState.Slamming && isGrounded)
        {
            GameManager.Instance.CamShake(0.3f, 0.6f);
            SetState(PlayerState.Idle);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Breakable"))
        {
            if (currentState == PlayerState.Mach3 || currentState == PlayerState.Slamming)
            {
                Destroy(other.gameObject);
                GameManager.Instance.CamShake(0.1f, 0.3f);
            }
        }
    }

    bool CheckGrounded()
    {
        // Adjust 1.2f based on your sprite's actual height!
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, groundLayer);
        return hit.collider != null;
    }
}