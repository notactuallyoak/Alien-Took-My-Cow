using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Health")]
    public int health;

    private bool canTakeDamage = true;
    private SpriteRenderer sr;
    private Collider2D[] myColliders;

    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        myColliders = GetComponentsInChildren<Collider2D>();    // get all colliders in children (Hurtbox and Hitbox)
    }

    public void TakeDamage(int dmg)
    {
        StartCoroutine(DoDamage(dmg));

        if (health <= 0) Die();
    }

    private IEnumerator DoDamage(int dmg)
    {
        if (!canTakeDamage) yield break;

        health -= dmg;
        AudioManager.Instance.PlaySFX("PlayerHit");

        // debounce & flash red
        canTakeDamage = false;
        sr.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        canTakeDamage = true;
        sr.color = Color.white;
    }

    private void Die()
    {
        foreach (var col in myColliders)
        {
            col.enabled = false;
        }

        ParticleEmitter.Instance.Emit("WhiteFlash", transform.position, Quaternion.identity);
        ParticleEmitter.Instance.Emit("BigSmoke", transform.position, Quaternion.identity);
        ParticleEmitter.Instance.Emit("DeadStar", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("EnemyDead");

        Destroy(gameObject, 0.1f);
    }
}
