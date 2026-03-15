using UnityEngine;

public class EnemyLettuce : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed;
    public float checkDistance;
    public LayerMask groundLayer;

    private bool movingRight = true;

    private void Update()
    {
        // move
        if (movingRight) transform.Translate(Vector2.right * speed * Time.deltaTime, Space.World);
        else transform.Translate(Vector2.left * speed * Time.deltaTime, Space.World);

        // collision checks
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, rayDirection, checkDistance, groundLayer);

        Vector2 groundCheckPos = (Vector2)transform.position + rayDirection * checkDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckPos, Vector2.down, checkDistance, groundLayer);

        if (wallHit.collider != null || groundHit.collider == null)
        {
            Flip();
        }
    }
    private void Flip()
    {
        movingRight = !movingRight;

        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(1, transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, rayDirection * checkDistance);

        Vector2 groundCheckPos = (Vector2)transform.position + rayDirection * checkDistance;
        Gizmos.DrawRay(groundCheckPos, Vector2.down * checkDistance);
    }
}