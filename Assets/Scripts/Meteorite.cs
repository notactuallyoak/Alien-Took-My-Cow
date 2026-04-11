using UnityEngine;
using System.Collections;

public class Meteorite : MonoBehaviour
{
    [Header("References")]
    public LayerMask groundLayer;

    private Vector3 spawnPosition;
    private bool isFalling = false;

    private void Start()
    {
        ParticleEmitter.Instance.Emit("WhiteFlash", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("ObjectSpawn");

        spawnPosition = transform.position;

        StartCoroutine(WindUp());
        Destroy(gameObject, 5);
    }

    private IEnumerator WindUp()
    {
        float windUpTime = 1.5f;
        float timer = 0f;

        // shake for 1.5 second
        while (timer < windUpTime)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 0.2f;
            transform.position = spawnPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = spawnPosition;
        isFalling = true;
    }

    private void Update()
    {
        if (isFalling)
        {
            transform.Translate(Vector2.down * 25 * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(1, transform.position);
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            ParticleEmitter.Instance.Emit("BigSmoke", transform.position, Quaternion.identity);
            CameraController.Instance.CamShake(0.1f, 0.1f);
            AudioManager.Instance.PlaySFX("BlockBreak");
            Destroy(gameObject);
        }
    }
}
