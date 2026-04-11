using UnityEngine;

public class EnemyWormSpace : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public LayerMask wallLayer;

    [Header("Wander Settings")]
    public float minWanderTime;
    public float maxWanderTime;
    public float minIdleTime;
    public float maxIdleTime;

    private float stateTimer;
    private bool isWandering = false;
    private int direction = 1; // 1 for right, -1 for left

    private void Start()
    {
        StartIdling();
    }

    private void Update()
    {
        if (isWandering)
        {
            Wander();
        }
        else
        {
            Idle();
        }
    }

    private void Wander()
    {
        Vector2 moveDir = (direction == 1) ? Vector2.right : Vector2.left;

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDir, 0.5f, wallLayer);

        if (stateTimer <= 0f || wallHit.collider != null)
        {
            StartIdling();
            return;
        }

        transform.Translate(moveDir * walkSpeed * Time.deltaTime, Space.World);

        FlipIfNeeded();

        stateTimer -= Time.deltaTime;
    }

    private void Idle()
    {
        if (stateTimer <= 0f)
        {
            StartWandering();
        }

        stateTimer -= Time.deltaTime;
    }

    private void StartWandering()
    {
        isWandering = true;
        stateTimer = Random.Range(minWanderTime, maxWanderTime);

        // 50% chance to go right, 50% chance to go left
        direction = Random.Range(0, 2) == 0 ? 1 : -1;
    }

    private void StartIdling()
    {
        isWandering = false;
        stateTimer = Random.Range(minIdleTime, maxIdleTime); // pick random idle time
    }

    private void FlipIfNeeded()
    {
        if (direction == 1 && transform.localScale.x < 0) Flip();
        else if (direction == -1 && transform.localScale.x > 0) Flip();
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}