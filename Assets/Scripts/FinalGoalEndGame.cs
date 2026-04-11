using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalGoalEndGame : MonoBehaviour
{
    private Collider2D finalGoalCollider;

    private void Start()
    {
        finalGoalCollider = GetComponent<Collider2D>();
        finalGoalCollider.enabled = false;

        StartCoroutine(DelayEnable());
    }

    private IEnumerator DelayEnable()
    {
        yield return new WaitForSeconds(1f);
        finalGoalCollider.enabled = true;
    }

    private IEnumerator ReachFinalGoal()
    {
        finalGoalCollider.enabled = false;

        GameManager.Instance.StopLevelTimer();

        yield return null; // wait 1 frame

        GameManager.Instance.FinishLevel();

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("Credit");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth.GetHealth() <= 0) return;

            playerHealth.isInvincible = true;
            playerHealth.enabled = false;

            StartCoroutine(ReachFinalGoal());
        }
    }
}
