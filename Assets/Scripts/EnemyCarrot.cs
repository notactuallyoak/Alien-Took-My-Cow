using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyCarrot : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed;
    public float checkDistance;
    public LayerMask groundLayer;

    [Header("Attack Settings")]
    public float detectionRadius;
    public LayerMask playerLayer;

    public Animator anim;

    private bool movingRight = true;
    private bool isAttacking = false;
    private Transform target;

    private void Update()
    {
        if (isAttacking) return;

        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerHit != null)
        {
            target = playerHit.transform;
            StartCoroutine(AttackRoutine());
            return;
        }

        Patrol();
    }

    void Patrol()
    {
        // move
        if (movingRight)
            transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime, Space.World);
        else
            transform.Translate(Vector2.left * patrolSpeed * Time.deltaTime, Space.World);

        Vector2 rayDir = movingRight ? Vector2.right : Vector2.left;

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, rayDir, checkDistance, groundLayer);

        Vector2 groundCheckPos = (Vector2)transform.position + rayDir * checkDistance;
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheckPos, Vector2.down, checkDistance, groundLayer);

        if (wallHit.collider != null || groundHit.collider == null)
        {
            Flip();
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.75f); // small delay after detect

        // hide in ground
        anim.SetTrigger("PreAttack");
        yield return new WaitForSeconds(0.8f);

        // teleport exactly to player pos
        if (target != null)
        {
            RaycastHit2D groundBelowPlayer = Physics2D.Raycast(target.position, Vector2.down, 2f, groundLayer);

            if (groundBelowPlayer.collider != null)
            {
                // snap X pos to player, keep Y pos (since we are underground)
                Vector3 newPos = transform.position;
                newPos.x = target.position.x;
                transform.position = newPos;
            }
            // else: player is over a pit/ledge, DO NOT move. attack current location.
        }

        // spike up
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(3f); // wind up before spikes come up
        yield return new WaitForSeconds(0.2f); // spike active time

        // backup from ground
        anim.SetTrigger("PostAttack");
        yield return new WaitForSeconds(0.8f);

        // reset
        isAttacking = false;
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Vector2 rayDir = movingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, rayDir * checkDistance);

        Vector2 groundCheckPos = (Vector2)transform.position + rayDir * checkDistance;
        Gizmos.DrawRay(groundCheckPos, Vector2.down * checkDistance);
    }
}