using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    public float dashForce = 25f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 0.6f;

    public bool IsDashing { get; private set; }

    bool canDash = true;

    Rigidbody2D rb;
    PlayerController controller;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        HandleDashInput();
    }

    public void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.X) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        controller.StartAction();
        controller.PlayDash();

        canDash = false;
        IsDashing = true;

        float dir = controller.facingDir;

        rb.linearVelocity = new Vector2(dir * dashForce, 0);

        yield return new WaitForSeconds(dashDuration);

        IsDashing = false;
        controller.EndAction();

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    public void StopDash()
    {
        StopAllCoroutines();
        IsDashing = false;
        canDash = true;
    }
}