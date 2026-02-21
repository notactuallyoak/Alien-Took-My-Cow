using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 12f;

    Rigidbody2D rb;
    PlayerController controller;
    PlayerMach mach;
    PlayerDash dash;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        mach = GetComponent<PlayerMach>();
        dash = GetComponent<PlayerDash>();
    }

    void FixedUpdate()
    {
        if (dash != null && dash.IsDashing)
            return;

        float finalX;

        if (mach != null && mach.CurrentSpeed > 0.1f)
            finalX = controller.facingDir * mach.CurrentSpeed;
        else
            finalX = controller.inputX * walkSpeed;

        rb.linearVelocity = new Vector2(finalX, rb.linearVelocity.y);
    }
}