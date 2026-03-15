using UnityEngine;

public class EnemyPotato : MonoBehaviour
{
    [Header("Movement")]
    public float chaseSpeed;
    public float detectionRadius;

    [Header("Safety Checks (Prevent Falling)")]
    public float wallCheckDistance;
    public float groundCheckDistance;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    public Animator anim;

    private Transform target;
    private bool isChasing = false;

    private void Update()
    {
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerHit != null)
        {
            isChasing = true;
            target = playerHit.transform;
        }
        else
        {
            isChasing = false;
            target = null;
        }

        if (isChasing && target != null)
        {
            ChaseWithSafety();
        }

        anim.SetBool("isChasing", isChasing);
    }

    void ChaseWithSafety()
    {
        float direction = target.position.x - transform.position.x;
        Vector2 moveDir = (direction > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDir, wallCheckDistance, groundLayer);

        Vector2 groundCheckOrigin = (Vector2)transform.position + moveDir * groundCheckDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckOrigin, Vector2.down, groundCheckDistance, groundLayer);


        if (wallHit.collider != null)
        {
            return; // wall blocked, do not move
        }

        if (groundHit.collider == null)
        {
            return; // ledge ahead, do not move
        }

        // move
        transform.Translate(moveDir * chaseSpeed * Time.deltaTime, Space.World);

        // facing
        if (moveDir == Vector2.right)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector2 facingDir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, facingDir * wallCheckDistance);

        Gizmos.color = Color.blue;
        Vector2 groundCheckOrigin = (Vector2)transform.position + facingDir * groundCheckDistance;
        Gizmos.DrawRay(groundCheckOrigin, Vector2.down * groundCheckDistance);
    }
}