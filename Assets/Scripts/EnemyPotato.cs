using UnityEngine;

public class EnemyPotato : MonoBehaviour
{
    [Header("Movement")]
    public float chaseSpeed;
    public float detectionRadius;
    public float detectionDelay;

    [Header("Safety Checks (Prevent Falling)")]
    public float wallCheckDistance;
    public float groundCheckDistance;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    public Animator anim;

    private Transform target;
    private bool isChasing = false;
    private bool isLockedOn = false; // detected but not moving yet
    private float detectionTimer = 0f;

    private void Update()
    {
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerHit != null)
        {
            target = playerHit.transform;

            // when detected plyer, set timer
            if (!isLockedOn && !isChasing)
            {
                isLockedOn = true;
                detectionTimer = detectionDelay;
            }

            if (isLockedOn && !isChasing)
            {
                detectionTimer -= Time.deltaTime;

                // face player while waiting
                float faceDir = target.position.x - transform.position.x;
                if (faceDir > 0) transform.localScale = new Vector3(1, 1, 1);
                else transform.localScale = new Vector3(-1, 1, 1);


                if (detectionTimer <= 0f)
                {
                    isChasing = true;
                    isLockedOn = true;
                    target = playerHit.transform;
                }
            }
        }
        else
        {
            isChasing = false;
            isLockedOn = false;
            target = null;
        }

        if (isChasing && target != null)
        {
            ChaseWithSafety();
        }

        anim.SetBool("isChasing", isChasing);
    }

    private void ChaseWithSafety()
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

    private void OnDrawGizmosSelected()
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