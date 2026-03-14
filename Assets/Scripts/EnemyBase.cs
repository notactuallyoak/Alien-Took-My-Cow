using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Health")]
    public int health;

    private bool canTakeDamage = true;

    public void TakeDamage(int dmg)
    {
        if (health <= 0) return;

        StartCoroutine(DoDamage(dmg));

        if (health <= 0) Destroy(gameObject);
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
}
