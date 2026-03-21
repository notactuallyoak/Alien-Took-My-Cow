using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{
    public float blinkInterval;
    public int blinkCount;

    public float activeDuration;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // disable damage at start
        col.enabled = false;

        StartCoroutine(LaserRoutine());
    }

    private IEnumerator LaserRoutine()
    {
        // blink
        for (int i = 0; i < blinkCount; i++)
        {
            SetAlpha(0.1f); // semi transparent
            yield return new WaitForSeconds(blinkInterval);

            SetAlpha(0.4f);
            yield return new WaitForSeconds(blinkInterval);
        }

        // active
        SetAlpha(1);
        col.enabled = true;

        yield return new WaitForSeconds(activeDuration);

        Destroy(gameObject);
    }

    private void SetAlpha(float a)
    {
        Color c = spriteRenderer.color;
        c.a = a;
        spriteRenderer.color = c;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(1);
            }
        }
    }
}