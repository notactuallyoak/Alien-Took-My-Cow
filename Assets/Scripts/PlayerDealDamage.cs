using UnityEngine;

public class PlayerDealDamage : MonoBehaviour
{
    public LayerMask whatIsDamagable;

    [Header("Damage")]
    public int runDamage;
    public int dashDamage;
    public int slamDamage;
    public int shotgunJumpDamage;

    [Header("Ranges")]
    public float attackRange;
    public float slamRange;

    [Header("Radius")]
    public float attackRadius;
    public float slamRadius;

    Transform player;

    void Awake()
    {
        player = transform.root;
    }

    // RUN
    public void RunHit()
    {
        Vector2 dir = new Vector2(player.localScale.x, 0);

        RaycastHit2D hit = Physics2D.CircleCast(
            player.position,
            attackRadius,
            dir,
            attackRange,
            whatIsDamagable
        );

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Breakable"))
            {
                CameraController.Instance?.CamShake(0.1f, 0.1f);
                Destroy(hit.collider.gameObject);
            }
            hit.collider.GetComponent<Enemy>()?.TakeDamage(runDamage);
        }
    }

    // DASH
    public void DashHit()
    {
        Vector2 dir = new Vector2(player.localScale.x, 0);

        RaycastHit2D hit = Physics2D.CircleCast(
            player.position,
            attackRadius,
            dir,
            attackRange,
            whatIsDamagable
        );

        if (hit.collider != null)
        {
            hit.collider.GetComponent<Enemy>()?.TakeDamage(dashDamage);
        }
    }

    // SLAM
    public void SlamHit()
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            player.position,
            slamRadius,
            Vector2.down,
            slamRange,
            whatIsDamagable
        );

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Breakable"))
            {
                CameraController.Instance?.CamShake(0.1f, 0.1f);
                Destroy(hit.collider.gameObject);
            }
            hit.collider.GetComponent<Enemy>()?.TakeDamage(slamDamage);
        }
    }

    // SHOTGUN JUMP
    public void ShotgunJumpHit()
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            player.position,
            attackRadius,
            Vector2.down,
            attackRange,
            whatIsDamagable
        );

        if (hit.collider != null)
        {
            hit.collider.GetComponent<Enemy>()?.TakeDamage(shotgunJumpDamage);
        }
    }

    // -------- GIZMOS --------

    void OnDrawGizmosSelected()
    {
        if (transform.root == null) return;

        Transform p = transform.root;

        Vector3 pos = p.position;

        // forward direction
        float dir = p.localScale.x >= 0 ? 1 : -1;

        // RUN / DASH
        Gizmos.color = Color.red;
        Vector3 forwardEnd = pos + new Vector3(dir * attackRange, 0, 0);
        Gizmos.DrawWireSphere(forwardEnd, attackRadius);

        // SLAM
        Gizmos.color = Color.blue;
        Vector3 slamEnd = pos + Vector3.down * slamRange;
        Gizmos.DrawWireSphere(slamEnd, slamRadius);

        // SHOTGUN JUMP
        Gizmos.color = Color.yellow;
        Vector3 sjEnd = pos + Vector3.down * attackRange;
        Gizmos.DrawWireSphere(sjEnd, attackRadius);
    }
}