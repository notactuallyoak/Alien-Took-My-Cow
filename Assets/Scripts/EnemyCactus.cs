using UnityEngine;
using System.Collections;

public class EnemyCactus : MonoBehaviour
{
    [Header("Ranges")]
    public float detectionRange;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float attackCooldown;

    [Header("References")]
    public Animator animator;
    public LayerMask playerLayer;

    private Transform target;
    private float attackTimer;
    private bool isShooting = false;

    void Start()
    {
        attackTimer = 0f;
    }

    void Update()
    {
        if (isShooting) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

        if (player != null)
        {
            target = player.transform;

            if (attackTimer <= 0)
            {
                StartCoroutine(ShootBurst());
            }
        }
        else
        {
            target = null;
        }
    }

    IEnumerator ShootBurst()
    {
        animator.SetTrigger("Shoot");
        isShooting = true;

        if (target.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        else if (target.position.x < transform.position.x && transform.localScale.x > 0) Flip();

        // fire 3 bullets
        for (int i = 0; i < 3; i++)
        {
            if (target != null && bulletPrefab != null && firePoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                bullet.transform.localScale = transform.localScale;
            }
            yield return new WaitForSeconds(0.2f);
        }

        attackTimer = attackCooldown;
        isShooting = false;
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}