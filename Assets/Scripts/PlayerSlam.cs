using UnityEngine;

public class PlayerSlam : MonoBehaviour
{
    public float slamSpeed = 25f;

    [Header("Cooldown")]
    public float slamCooldown = 0.8f;

    bool isSlamming;
    float slamCDCounter;

    public bool IsSlamming => isSlamming;

    Rigidbody2D rb;
    PlayerJump jump;
    PlayerController controller;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        jump = GetComponent<PlayerJump>();
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (slamCDCounter > 0)
            slamCDCounter -= Time.deltaTime;

        if (!jump.IsGrounded &&
            Input.GetKeyDown(KeyCode.S) &&  
            !isSlamming &&
            slamCDCounter <= 0)
        {
            StartSlam();
        }

        if (isSlamming && jump.IsGrounded)
        {
            EndSlam();
        }
    }

    void StartSlam()
    {
        controller.PlaySlamBegin();

        isSlamming = true;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -slamSpeed);
    }

    void EndSlam()
    {
        controller.PlaySlamEnd();

        isSlamming = false;
        slamCDCounter = slamCooldown;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            0f
        );
    }
}