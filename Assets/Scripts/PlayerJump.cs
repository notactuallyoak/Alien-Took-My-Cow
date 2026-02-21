using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 16f;

    [Header("ShotJump")]
    public float shotJumpForce = 18f;
    public int maxAirJumps = 1;
    int airJumpsLeft;

    public bool IsGrounded { get; private set; }

    Rigidbody2D rb;

    PlayerDash dash;
    PlayerMach mach;
    PlayerController controller;

    LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        dash = GetComponent<PlayerDash>();
        mach = GetComponent<PlayerMach>();
        controller = GetComponent<PlayerController>();

        groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        CheckGround();
        HandleJumpInput();
    }

    void HandleJumpInput()
    {
        if (!Input.GetButtonDown("Jump"))
            return;

        // Dash Cancel -> ShotJump
        if (dash != null && dash.IsDashing)
        {
            dash.StopDash();
            ShotJump();
            return;
        }

        if (IsGrounded)
        {
            NormalJump();
        }
        else if (airJumpsLeft > 0)
        {
            ShotJump();
        }
    }

    void NormalJump()
    {
        airJumpsLeft = maxAirJumps;

        if (mach.MachLevel == 0)
            controller.PlayNormalJump();

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        IsGrounded = false;
    }

    void ShotJump()
    {
        controller.PlayExtraJump();
        GameManager.Instance.CamShake(0.1f, 0.25f);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, shotJumpForce);
        airJumpsLeft--;
    }

    void CheckGround()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,
            new Vector2(0.8f, 0.1f),
            0f,
            Vector2.down,
            0.1f,
            groundLayer);

        IsGrounded = hit.collider != null;

        if (IsGrounded)
        {
            airJumpsLeft = maxAirJumps;
        }
    }
}