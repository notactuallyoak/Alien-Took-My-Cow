using UnityEngine;

public class Breakable : MonoBehaviour
{
    public void TakeDamage()
    {
        ParticleEmitter.Instance.Emit("BlockBreak", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("BlockBreak", 0.05f);

        Destroy(gameObject);
    }
}
