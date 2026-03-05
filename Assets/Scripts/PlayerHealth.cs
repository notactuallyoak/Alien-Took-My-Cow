using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth;
    private int currentHealth;

    [Header("UI Reference")]
    public Image heartImage;
    public Sprite[] heartSprites;

    [Header("Invincibility Settings")]
    public float invincibilityDuration;
    public float blinkInterval;
    private bool isInvincible = false;

    [Header("Knockback Settings")]
    public float knockbackForceX;
    public float knockbackForceY;
    public float hurtDuration;

    private SpriteRenderer playerSprite;
    private Rigidbody2D rb;
    private PlayerController playerController;

    void Start()
    {
        currentHealth = maxHealth;

        playerSprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        UpdateHeartUI();
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isInvincible || currentHealth <= 1) return;

        currentHealth -= damage;
        CameraController.Instance?.CamShake(0.1f, 0.1f);
        UpdateHeartUI();

        ApplyKnockback(enemyPosition);

        if (currentHealth <= 1)
            Die();
    }

    public void TakeDamage(int damage) // overload
    {
        TakeDamage(damage, transform.position);
    }

    void UpdateHeartUI()
    {
        if (heartSprites.Length == 0 || heartImage == null) return;

        // prevent index out of range
        int index = maxHealth - currentHealth;
        if (index >= heartSprites.Length) index = heartSprites.Length - 1;

        heartImage.sprite = heartSprites[index];
    }

    void ApplyKnockback(Vector2 hitSource)
    {
        if (rb == null) return;

        StopAllCoroutines();

        if (playerSprite != null) playerSprite.enabled = true;
        if (playerController != null) playerController.CancelActions();

        if (rb != null) rb.gravityScale = 4; // reset gravity in case player got hurt during dash

        Vector2 dir = (transform.position - (Vector3)hitSource).normalized;
        if (dir == Vector2.zero) dir = Vector2.up;

        // apply force
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir.x * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        StartCoroutine(KnockbackRoutine());

        if (currentHealth > 1)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    IEnumerator KnockbackRoutine()
    {
        if (playerController != null) playerController.isHurt = true;
        yield return new WaitForSeconds(hurtDuration);
        if (playerController != null) playerController.isHurt = false;
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        float timer = 0f;
        while (timer < invincibilityDuration)
        {
            timer += Time.deltaTime;

            // toggle visible
            if (playerSprite != null)
                playerSprite.enabled = !playerSprite.enabled;

            yield return new WaitForSeconds(blinkInterval);
        }

        if (playerSprite != null) playerSprite.enabled = true;

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Logic Game Over
        // GameManager.Instance.GameOver();
        gameObject.SetActive(false);
    }
}