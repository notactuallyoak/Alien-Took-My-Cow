using UnityEngine;
using System.Collections;

public class EnemyCaterpillar : MonoBehaviour
{
    [Header("Ranges")]
    public float detectionRange;
    public float attackRange;

    [Header("Movement")]
    public float walkSpeed;
    public float groundCheckDistance;
    public LayerMask groundLayer;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float attackCooldown;

    [Header("References")]
    public Animator anim;
    public LayerMask playerLayer;

    private Transform target;
    private float attackTimer;
    private bool isAttacking = false;

    void Start()
    {
        attackTimer = 0f;
    }

    private void Update()
    {
        if (isAttacking) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        FindTarget();
    
        // state
        if (target != null)
        {
            float distance = Vector2.Distance(transform.position, target.position);

            // prioritize attack (mid range)
            if (distance <= attackRange && attackTimer <= 0)
            {
                StartCoroutine(AttackRoutine());
            }
            // then walk (far range)
            else if (distance <= detectionRange)
            {
                WalkTowardsTarget();
            }
            else
            {
                Idle();
            }
        }
        else
        {
            Idle();
        }
    }

    void FindTarget()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            target = null;
        }
    }

    void Idle()
    {
        anim.SetBool("IsWalking", false);
    }

    void WalkTowardsTarget()
    {
        anim.SetBool("IsWalking", true);

        float dir = target.position.x - transform.position.x;
        Vector2 moveDir = (dir > 0) ? Vector2.right : Vector2.left;

        RaycastHit2D groundHit = Physics2D.Raycast(transform.position + (Vector3)(moveDir * 0.5f), Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDir, 0.5f, groundLayer);

        if (groundHit.collider != null && wallHit.collider == null)
        {
            transform.Translate(moveDir * walkSpeed * Time.deltaTime, Space.World);
        }

        // Flip to face player
        if (dir > 0 && transform.localScale.x < 0) Flip();
        else if (dir < 0 && transform.localScale.x > 0) Flip();
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("IsWalking", false); // stop walking
        anim.SetTrigger("Attack");

        // face player before shoot
        if (target.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        else if (target.position.x < transform.position.x && transform.localScale.x > 0) Flip();

        // wind up
        yield return new WaitForSeconds(1f);

        if (target != null && bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.transform.localScale = transform.localScale;
        }

        // wind down
        yield return new WaitForSeconds(0.5f);

        attackTimer = attackCooldown;
        isAttacking = false;
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // far range

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);    // mid range
    }
}