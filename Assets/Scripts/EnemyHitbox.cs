using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 1;
    public float knockbackPower = 1f;

    private Collider2D hitCollider;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        hitCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        hitCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        hitCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 knockbackDir = (playerHealth.gameObject.transform.position - transform.root.position).normalized;

                playerHealth.TakeDamage(damage, knockbackDir * knockbackPower);
            }
        }
    }
}