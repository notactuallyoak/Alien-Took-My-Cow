using UnityEngine;
using System.Collections;

public class BigCar : MonoBehaviour
{
    [Header("Movement Settings")]
    public float dashSpeed;
    public float dashDuration;
    public float groundCheckDistance;
    public float wallCheckDistance;
    public LayerMask groundLayer;

    [Header("Detection")]
    public float detectionRadius;
    public float recoveryTime;

    [Header("Attack Timing")]
    private float windUpTime = 0.75f;
    private float shakeIntensity = 0.1f;

    [Header("References")]
    public Animator anim;
    public LayerMask playerLayer;

    private bool isBusy = false;
    private int facingDirection = 1; // 1 = Right, -1 = Left
    private Transform target;

    private void Start()
    {
        if (transform.localScale.x < 0) facingDirection = -1;
    }

    private void Update()
    {
        if (isBusy) return;

        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerHit != null)
        {
            target = playerHit.transform;
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isBusy = true;

        if (target == null)
        {
            isBusy = false;
            yield break;
        }

        FaceTarget(target.position);

        // wind up + Shake
        yield return StartCoroutine(DoWindUp());

        // dash
        yield return StartCoroutine(DashAttack());


        yield return new WaitForSeconds(recoveryTime);

        isBusy = false;
    }

    private IEnumerator DoWindUp()
    {
        anim.SetTrigger("Charge");
        AudioManager.Instance.PlaySFX("BigCarCharge", 0.1f);

        Vector3 originalPos = transform.position;
        float timer = 0f;

        while (timer < windUpTime)
        {
            Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity;
            transform.position = originalPos + new Vector3(randomOffset.x, randomOffset.y, 0f);

            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;
    }

    private IEnumerator DashAttack()
    {
        anim.SetTrigger("Attack");

        float timer = 0f;
        while (timer < dashDuration)
        {
            // move fast
            transform.Translate(Vector2.right * facingDirection * dashSpeed * Time.deltaTime);

            bool isGroundAhead = CheckGround();
            bool isWallAhead = CheckWall();

            // stop dashing if it hits a wall or edge
            if (!isGroundAhead || isWallAhead)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private bool CheckGround()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(facingDirection * 1f, 0f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private bool CheckWall()
    {
        Vector2 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * facingDirection, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private void Flip()
    {
        facingDirection *= -1;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void FaceTarget(Vector3 targetPos)
    {
        if (targetPos.x > transform.position.x && facingDirection == -1) Flip();
        else if (targetPos.x < transform.position.x && facingDirection == 1) Flip();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        float gizmoFacingDirection = transform.localScale.x > 0 ? 1 : -1;

        Gizmos.color = Color.green;
        Vector2 groundRayOrigin = (Vector2)transform.position + new Vector2(gizmoFacingDirection * 0.5f, 0f);
        Gizmos.DrawLine(groundRayOrigin, groundRayOrigin + Vector2.down * groundCheckDistance);

        Gizmos.color = Color.blue;
        Vector2 wallRayOrigin = transform.position;
        Gizmos.DrawLine(wallRayOrigin, wallRayOrigin + Vector2.right * gizmoFacingDirection * wallCheckDistance);
    }
}