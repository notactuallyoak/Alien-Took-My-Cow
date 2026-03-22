using UnityEngine;
using System.Collections;

public class EnemyAlien : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed;
    public float groundCheckDistance;
    public float wallCheckDistance;
    public LayerMask groundLayer;

    [Header("Detection Settings")]
    public float sightRange;
    public LayerMask playerLayer;
    public LayerMask sightBlockingLayers;

    [Header("Attack Settings")]
    public float attackCooldown;
    public Collider2D attackHitbox;

    [Header("References")]
    public Animator anim;
    public Transform gunPoint;

    private bool movingRight = true;
    private bool isAttacking = false;
    private float attackTimer;

    void Start()
    {
        attackTimer = 0f;
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    private void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (isAttacking) return;

        if (attackTimer <= 0 && CheckForTarget())
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        Patrol();
    }

    bool CheckForTarget()
    {
        Vector2 rayDir = movingRight ? Vector2.right : Vector2.left;

        // raycast forward to see if we hit a player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, sightRange, sightBlockingLayers);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    void Patrol()
    {
        // move
        if (movingRight)
            transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime, Space.World);
        else
            transform.Translate(Vector2.left * patrolSpeed * Time.deltaTime, Space.World);

        Vector2 rayDir = movingRight ? Vector2.right : Vector2.left;

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, rayDir, wallCheckDistance, groundLayer);

        Vector2 groundCheckOrigin = (Vector2)transform.position + rayDir * groundCheckDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckOrigin, Vector2.down, groundCheckDistance, groundLayer);

        if (wallHit.collider != null || groundHit.collider == null)
        {
            Flip();
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        
        ParticleEmitter.Instance.Emit("AlienGunCharge", gunPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(1.2f); // windup time

        anim.SetTrigger("Attack");
        ParticleEmitter.Instance.Emit("AlienGunMuzzleFlash", gunPoint.position, !movingRight);
        ParticleEmitter.Instance.Emit("AlienGunLaser", gunPoint.position, !movingRight);
        AudioManager.Instance.PlaySFX("AlienShoot");

        if (attackHitbox != null) attackHitbox.enabled = true;
        yield return new WaitForSeconds(0.125f); // hit active time
        if (attackHitbox != null) attackHitbox.enabled = false;

        // reset cooldown
        attackTimer = attackCooldown;

        yield return new WaitForSeconds(0.75f);
        isAttacking = false;
    }

    void Flip()
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
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(2, transform.position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector2 rayDir = movingRight ? Vector2.right : Vector2.left;

        Gizmos.DrawRay(transform.position, rayDir * wallCheckDistance);

        Vector2 groundCheckOrigin = (Vector2)transform.position + rayDir * groundCheckDistance;
        Gizmos.DrawRay(groundCheckOrigin, Vector2.down * groundCheckDistance);
    }
}