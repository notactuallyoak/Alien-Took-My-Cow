using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float lifeTime;
    private int damage = 1;

    private void Start()
    {
        ParticleEmitter.Instance.Emit("MuzzleFlashSmall", transform.position, Quaternion.identity);
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        Vector2 moveDirection = new Vector2(transform.localScale.x, 0);
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}