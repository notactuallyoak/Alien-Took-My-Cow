using UnityEngine;

public class EnemyUFOSpawner : MonoBehaviour
{
    public GameObject enemyUFOPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Instantiate(enemyUFOPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
