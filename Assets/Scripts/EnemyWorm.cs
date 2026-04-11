using UnityEngine;
using System.Collections;

public class EnemyWorm : MonoBehaviour
{
    [Header("Ranges")]
    public float faceRange;
    public float attackRange;

    [Header("Attack Settings")]
    [Range(0f, 1f)]
    public float attackChance;
    private float checkInterval = 1f;

    [Header("References")]
    public Animator anim;
    public LayerMask playerLayer;

    private bool isAttacking = false;
    private float checkTimer;
    private Transform target;

    private void Start()
    {
        checkTimer = checkInterval;
    }

    private void Update()
    {
        if (isAttacking) return;

        Collider2D player = Physics2D.OverlapCircle(transform.position, faceRange, playerLayer);

        if (player != null)
        {
            target = player.transform;
            float distance = Vector2.Distance(transform.position, target.position);

            // far range: face player
            if (distance <= faceRange)
            {
                if (target.position.x > transform.position.x && transform.localScale.x < 0) Flip();
                else if (target.position.x < transform.position.x && transform.localScale.x > 0) Flip();
            }

            // near range: attack
            if (distance <= attackRange)
            {
                checkTimer -= Time.deltaTime;

                if (checkTimer <= 0f)
                {
                    checkTimer = checkInterval;

                    if (Random.value <= attackChance)
                    {
                        StartCoroutine(OpenMouth());
                    }
                }
            }
            else
            {
                // player in face range, but out of attack range
                checkTimer = checkInterval;
            }
        }
        else
        {
            // player face left range
            target = null;
            checkTimer = checkInterval;
        }
    }

    private IEnumerator OpenMouth()
    {
        isAttacking = true;

        if (target != null)
        {
            if (target.position.x > transform.position.x && transform.localScale.x < 0) Flip();
            else if (target.position.x < transform.position.x && transform.localScale.x > 0) Flip();
        }

        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(1f);

        isAttacking = false;
    }
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, faceRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}