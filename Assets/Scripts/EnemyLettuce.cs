using UnityEngine;

public class EnemyLettuce : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed;
    public float checkDistance;
    public LayerMask groundLayer;

    [Header("Timing Settings")]
    public float moveDuration;
    public float pauseDuration;

    private float timer;
    private bool isMoving = true;

    private bool movingRight = true;

    private void Start()
    {
        timer = moveDuration;
    }

    private void Update()
    {
        if (isMoving)
        {
            HandleMovement();
        }
        else
        {
            HandlePause();
        }
    }

    private void HandleMovement()
    {
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;

        // move
        transform.Translate(rayDirection * speed * Time.deltaTime, Space.World);

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, rayDirection, checkDistance, groundLayer);
        Vector2 groundCheckPos = (Vector2)transform.position + rayDirection * checkDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckPos, Vector2.down, checkDistance, groundLayer);

        if (wallHit.collider != null || groundHit.collider == null)
        {
            Flip();
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SwitchToPause();
        }
    }

    private void HandlePause()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SwitchToMove();
        }
    }

    private void SwitchToPause()
    {
        isMoving = false;
        timer = pauseDuration;
    }

    private void SwitchToMove()
    {
        isMoving = true;
        timer = moveDuration;
    }

    private void Flip()
    {
        movingRight = !movingRight;

        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, rayDirection * checkDistance);

        Vector2 groundCheckPos = (Vector2)transform.position + rayDirection * checkDistance;
        Gizmos.DrawRay(groundCheckPos, Vector2.down * checkDistance);
    }
}