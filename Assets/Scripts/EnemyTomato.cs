using UnityEngine;
using System.Collections;

public class EnemySimplePatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed;
    public float checkDistance;
    public LayerMask groundLayer;

    [Header("Attack Settings (Creeper Style)")]
    public float detectionRadius;
    public float explosionRadius;
    public float fuseTime;

    public LayerMask playerLayer;
    public Animator anim;

    private bool movingRight = true;
    private bool isAttacking = false;

    private void Update()
    {
        if (!isAttacking)
        {
            Collider2D playerHit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

            if (playerHit != null)
            {
                StartCoroutine(ExplodeRoutine());
            }
            else
            {
                Patrol();
            }
        }
    }

    void Patrol()
    {
        // move
        if (movingRight) transform.Translate(Vector2.right * speed * Time.deltaTime, Space.World);
        else transform.Translate(Vector2.left * speed * Time.deltaTime, Space.World);

        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, rayDirection, checkDistance, groundLayer);

        Vector2 groundCheckPos = (Vector2)transform.position + rayDirection * checkDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckPos, Vector2.down, checkDistance, groundLayer);

        if (wallHit.collider != null || groundHit.collider == null)
        {
            Flip();
        }
    }

    IEnumerator ExplodeRoutine()
    {
        isAttacking = true; // stop moving
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(fuseTime);

        Explode();
    }

    void Explode()
    {
        ParticleEmitter.Instance.Emit("MuzzleFlash", transform.position, Quaternion.identity);
        ParticleEmitter.Instance.Emit("BigSmoke", transform.position, Quaternion.identity);
        ParticleEmitter.Instance.Emit("DeadStar", transform.position, Quaternion.identity);

        // deal area damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                playerHealth.TakeDamage(1, transform.position);

                CameraController.Instance.CamShake(0.2f, 0.2f);
                ParticleEmitter.Instance.Emit("WhiteFlash", transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;

        Gizmos.DrawRay(transform.position, rayDirection * checkDistance);
        Vector2 groundCheckPos = (Vector2)transform.position + rayDirection * checkDistance;
        Gizmos.DrawRay(groundCheckPos, Vector2.down * checkDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}