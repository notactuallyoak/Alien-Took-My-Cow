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

    [Header("References")]
    public Animator anim;
    public Transform gunPoint;

    private bool movingRight = true;
    private bool isAttacking = false;
    private float attackTimer;

    private void Start()
    {
        attackTimer = 0f;
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

    private bool CheckForTarget()
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

    private void Patrol()
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

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        
        ParticleEmitter.Instance.Emit("AlienGunCharge", gunPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(1.2f); // windup time

        anim.SetTrigger("Attack");
        ParticleEmitter.Instance.Emit("AlienGunMuzzleFlash", gunPoint.position, !movingRight);
        ParticleEmitter.Instance.Emit("AlienGunLaser", gunPoint.position, !movingRight);
        AudioManager.Instance.PlaySFX("AlienShoot");

        yield return new WaitForSeconds(0.125f); // hit active time

        // reset cooldown
        attackTimer = attackCooldown;

        yield return new WaitForSeconds(1f);
        isAttacking = false;
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
        Gizmos.color = Color.blue;
        Vector2 rayDir = movingRight ? Vector2.right : Vector2.left;

        Gizmos.DrawRay(transform.position, rayDir * wallCheckDistance);

        Vector2 groundCheckOrigin = (Vector2)transform.position + rayDir * groundCheckDistance;
        Gizmos.DrawRay(groundCheckOrigin, Vector2.down * groundCheckDistance);
    }
}