using System.Collections;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngineInternal;

public class EnemyBase : MonoBehaviour
{
    [Header("Health")]
    public int health;

    private bool canTakeDamage = true;

    public void TakeDamage(int dmg)
    {
        if (health <= 0) return;

        StartCoroutine(DoDamage(dmg));

        if (health <= 0) Die();
    }

    private IEnumerator DoDamage(int dmg)
    {
        if (!canTakeDamage) yield break;

        health -= dmg;

        // flash red & debounce
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

        sr.color = Color.red;
        canTakeDamage = false;

        yield return new WaitForSeconds(0.05f);

        sr.color = Color.white;
        canTakeDamage = true;
    }

    private void Die()
    {
        ParticleEmitter.Instance.Emit("WhiteFlash", transform.position, Quaternion.identity);
        ParticleEmitter.Instance.Emit("BigSmoke", transform.position, Quaternion.identity);
        ParticleEmitter.Instance.Emit("DeadStar", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("EnemyDead");

        Destroy(gameObject);
    }
}
