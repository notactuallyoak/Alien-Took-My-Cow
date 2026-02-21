using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle = 0,
        Walk = 1,
        Mach = 2,
        Run = 3,
        Air = 4
    }

    public PlayerState currentState;
    public bool IsPerformingAction { get; private set; }

    public float facingDir = 1f;
    public float inputX;

    public Animator animator;

    Rigidbody2D rb;
    SpriteRenderer sr;

    PlayerMach mach;
    PlayerJump jump;
    PlayerDash dash;
    PlayerSlam slam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        mach = GetComponent<PlayerMach>();
        jump = GetComponent<PlayerJump>();
        dash = GetComponent<PlayerDash>();
        slam = GetComponent<PlayerSlam>();
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        // flip
        if (inputX != 0)
        {
            facingDir = Mathf.Sign(inputX);
            sr.flipX = facingDir < 0;
        }

        UpdateState();
    }

    public void StartAction()
    {
        IsPerformingAction = true;
    }

    public void EndAction()
    {
        IsPerformingAction = false;
    }

    void UpdateState()
    {
        if (dash.IsDashing || slam.IsSlamming)
            return;

        PlayerState newState;

        if (!jump.IsGrounded)
            newState = PlayerState.Air;
        else if (mach.MachLevel >= 3)
            newState = PlayerState.Run;
        else if (mach.MachLevel >= 1)
            newState = PlayerState.Mach;
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            newState = PlayerState.Walk;
        else
            newState = PlayerState.Idle;

        if (newState != currentState)
        {
            currentState = newState;
            animator.SetInteger("State", (int)currentState);
        }
    }

    public void PlayNormalJump()
    {
        animator.SetTrigger("PlayerJump");
    }
    public void PlayExtraJump()
    {
        animator.SetTrigger("PlayerJumpExtra");
    }
    public void PlayDash()
    {
        animator.SetTrigger("PlayerDash");
    }
    public void PlaySlamBegin()
    {
        animator.SetTrigger("PlayerSlam");
    }
    public void PlaySlamEnd()
    {
        animator.SetTrigger("PlayerSlamFeedback");
    }
}