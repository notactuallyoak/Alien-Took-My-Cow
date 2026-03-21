using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    public Animator anim;

    private Collider2D goalCollider;

    private void Start()
    {
        goalCollider = GetComponent<Collider2D>();
    }

    private IEnumerator ReachGoal(Collider2D collision)
    {
        GameManager.Instance.StopLevelTimer();

        anim.SetTrigger("Goal");
        goalCollider.enabled = false;

        yield return new WaitForSeconds(3f);
        GameManager.Instance.FinishLevel();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth.GetHealth() <= 0) return;

            playerHealth.isInvincible = true;
            playerHealth.enabled = false;

            StartCoroutine(ReachGoal(collision));
        }
    }
}
