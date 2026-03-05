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
    public float dashRange;
    public float slamRange;
    public float shotgunRange;

    [Header("Radius")]
    public float attackRadius;
    public float dashRadius;
    public float slamRadius;
    public float shotgunRadius;


    Transform player;

    void Awake()
    {
        player = transform.root;
    }

    // RUN
    public void RunHit()
    {
        Vector2 dir = new Vector2(player.localScale.x, 0);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            player.position,
            attackRadius,
            dir,
            attackRange,
            whatIsDamagable
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Breakable"))
                {
                    CameraController.Instance?.CamShake(0.15f, 0.1f);
                    Destroy(hit.collider.gameObject);
                }
                hit.collider.GetComponent<Enemy>()?.TakeDamage(runDamage);
            }
        }
    }

    // DASH
    public void DashHit()
    {
        Vector2 dir = new Vector2(player.localScale.x, 0);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            player.position,
            dashRadius,
            dir,
            dashRange,
            whatIsDamagable
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                hit.collider.GetComponent<Enemy>()?.TakeDamage(dashDamage);
            }
        }
    }

    // SLAM
    public void SlamHit()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            player.position,
            slamRadius,
            Vector2.down,
            slamRange,
            whatIsDamagable
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Breakable"))
                {
                    CameraController.Instance?.CamShake(0.15f, 0.1f);
                    Destroy(hit.collider.gameObject);
                }
                hit.collider.GetComponent<Enemy>()?.TakeDamage(slamDamage);
            }
        }
    }

    // SHOTGUN JUMP
    public void ShotgunJumpHit()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            player.position,
            shotgunRadius,
            Vector2.down,
            shotgunRange,
            whatIsDamagable
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                hit.collider.GetComponent<Enemy>()?.TakeDamage(shotgunJumpDamage);
            }
        }
    }

    // gizmo
    void OnDrawGizmosSelected()
    {
        if (transform.root == null) return;

        Transform p = transform.root;
        Vector3 pos = p.position;
        float dir = p.localScale.x >= 0 ? 1 : -1;

        Gizmos.color = Color.red;
        Vector3 forwardEnd = pos + new Vector3(dir * attackRange, 0, 0);
        Gizmos.DrawWireSphere(forwardEnd, attackRadius);

        Gizmos.color = Color.green;
        Vector3 dashEnd = pos + new Vector3(dir * dashRange, 0, 0);
        Gizmos.DrawWireSphere(dashEnd, dashRadius);

        Gizmos.color = Color.blue;
        Vector3 slamEnd = pos + Vector3.down * slamRange;
        Gizmos.DrawWireSphere(slamEnd, slamRadius);

        Gizmos.color = Color.yellow;
        Vector3 sjEnd = pos + Vector3.down * shotgunRange;
        Gizmos.DrawWireSphere(sjEnd, shotgunRadius);
    }
}