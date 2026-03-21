using UnityEngine;

public class EnemyUFO : MonoBehaviour
{
    private void Start()
    {
        Destroy(transform.parent.gameObject, 4.8f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(1, transform.position);
        }
    }

}
