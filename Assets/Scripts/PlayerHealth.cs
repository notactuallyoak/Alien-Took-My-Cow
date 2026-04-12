using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private int maxHealth = 8;
    private int currentHealth;
    private int previousHealth = -1;

    [Header("Invincibility Settings")]
    public float invincibilityDuration;
    public float blinkInterval;
    [HideInInspector] public bool isInvincible = false;
    [HideInInspector] public bool godMode = false; // for debug cheats only

    [Header("Knockback Settings")]
    public float knockbackForceX_Min;
    public float knockbackForceY_Min;
    public float knockbackForceX_Max;
    public float knockbackForceY_Max;

    public float hurtDuration;

    private PlayerController playerController;
    private SpriteRenderer playerSprite;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;

        playerController = GetComponent<PlayerController>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        UpdateHeart();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHeart();
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (godMode) return;
        if (isInvincible || currentHealth <= 0) return;
        if (playerController.isDashing || playerController.isSlamming || playerController.isHurt) return; // I-frames

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
        GameManager.Instance.UpdateHealthUI(currentHealth);

        previousHealth = currentHealth;
    }

    void ApplyKnockback(Vector2 hitSource)
    {
        if (rb == null) return;

        StopAllCoroutines();

        if (playerSprite != null) playerSprite.enabled = true;
        if (playerController != null) playerController.CancelActions();

        if (rb != null) rb.gravityScale = 4; // reset gravity in case player got hurt during dash

        // calc dynamic knockback force
        float healthPercent = 1f - ((float)(currentHealth - 1) / (maxHealth - 1));
        float currentForceX = Mathf.Lerp(knockbackForceX_Min, knockbackForceX_Max, healthPercent);
        float currentForceY = Mathf.Lerp(knockbackForceY_Min, knockbackForceY_Max, healthPercent);

        Vector2 dir = (transform.position - (Vector3)hitSource).normalized;
        if (dir == Vector2.zero) dir = Vector2.up;

        // apply force
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir.x * currentForceX, currentForceY), ForceMode2D.Impulse);

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