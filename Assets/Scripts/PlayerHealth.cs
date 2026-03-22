using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private int maxHealth = 8;
    private int currentHealth;
    private int previousHealth = -1;

    [Header("UI Reference")]
    public Animator heartAnimator;

    [Header("Invincibility Settings")]
    public float invincibilityDuration;
    public float blinkInterval;
    [HideInInspector] public bool isInvincible = false;

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

        UpdateHeart();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isInvincible || currentHealth <= 0) return;
        if (playerController.isDashing || playerController.isSlamming || playerController.isHurt) return;

        currentHealth -= damage;
        CameraController.Instance?.CamShake(0.1f, 0.1f);
        AudioManager.Instance.PlaySFX("PlayerHurt");
        UpdateHeart();

        ApplyKnockback(enemyPosition);

        if (currentHealth <= 0)
        {
            AudioManager.Instance.PlaySFX("PlayerDeath");
            GameManager.Instance.GameOver();
        } 
    }

    public void TakeDamage(int damage) // overload
    {
        TakeDamage(damage, transform.position);
    }

    void UpdateHeart()
    {
        // only update if health changed
        if (currentHealth == previousHealth) return;

        // update anim parameter
        heartAnimator.SetInteger("Health", currentHealth);

        previousHealth = currentHealth;
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

    public int GetHealth()
    {
        return currentHealth;
    }
}