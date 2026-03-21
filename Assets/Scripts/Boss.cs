using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalDashSpeed;
    public float fastDashSpeed;
    public float groundCheckDistance;
    public float wallCheckDistance;
    public LayerMask groundLayer;

    [Header("Detection")]
    public float detectionRadius;
    public float recoveryTime;

    [Header("Attack Timing")]
    public float windUpTime;
    public float normalAttackDuration;
    public float fastAttackDuration;

    [Header("References")]
    public Animator anim;
    public LayerMask playerLayer;
    public GameObject ufoPrefab;
    public GameObject laserPrefab;

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
            StartCoroutine(BossRoutine());
        }
        else
        {
            target = null;
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

    private IEnumerator BossRoutine()
    {
        isBusy = true;

        if (target == null)
        {
            isBusy = false;
            yield break;
        }

        FaceTarget(target.position);

        if (Random.value > 0.7f)
        {
            yield return StartCoroutine(DoWindUp());
            yield return StartCoroutine(DashAttack(fastDashSpeed, fastAttackDuration));
        }
        else
        {
            yield return StartCoroutine(DoWindUp());
            yield return StartCoroutine(DashAttack(normalDashSpeed, normalAttackDuration));
        }

        // Random Summon UFO or Laser
        if (Random.value > 0.6f)
        {
            yield return StartCoroutine(DoSummonUFO());
        }
        else
        {
            yield return StartCoroutine(DoSummonLaser());
        }

        yield return new WaitForSeconds(recoveryTime);

        isBusy = false;
    }

    private IEnumerator DoWindUp()
    {
        anim.SetTrigger("WindUp");
        yield return new WaitForSeconds(windUpTime);
    }

    private IEnumerator DashAttack(float speed, float duration)
    {
        if (speed == normalAttackDuration)
        {
            anim.SetTrigger("NormalAttack");
        }
        else
        {
            anim.SetTrigger("FastAttack");
        }

        float timer = 0f;

        while (timer < duration)
        {
            transform.Translate(Vector2.right * facingDirection * speed * Time.deltaTime);

            bool isGroundAhead = CheckGround();
            bool isWallAhead = CheckWall();

            if (!isGroundAhead || isWallAhead)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DoSummonUFO()
    {
        if (ufoPrefab != null && target != null)
        {
            Vector2 spawnPos = target.position;

            RaycastHit2D hit = Physics2D.Raycast(target.position, Vector2.down, 13f, groundLayer);

            if (hit.collider != null)
            {
                spawnPos = hit.point;
                spawnPos.y += 1;
            }

            Instantiate(ufoPrefab, spawnPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator DoSummonLaser()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * Random.Range(-7f, 10f);
            Vector2 spawnPos = (Vector2)target.position + randomOffset;

            float randomAngle = Random.Range(30f, 270f);
            Quaternion spawnRotation = Quaternion.Euler(0, 0, randomAngle);

            // spawn Laser
            Instantiate(laserPrefab, spawnPos, spawnRotation);

            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(0.5f);
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
        if (targetPos.x > transform.position.x && facingDirection == -1)
        {
            Flip();
        }
        else if (targetPos.x < transform.position.x && facingDirection == 1)
        {
            Flip();
        }
    }

        private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(1, transform.position);
        }
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