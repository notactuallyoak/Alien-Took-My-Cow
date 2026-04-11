using UnityEngine;
using System.Collections;

public class BossPhase1 : MonoBehaviour
{
    public GameObject prefabBossPhase2;

    [Header("Detection & Movement")]
    private int detectionRange = 12;
    public float moveSpeed;

    [Header("Prefabs")]
    public Collider2D hitbox;

    [Header("Prefabs")]
    public GameObject[] meteoritePrefabs;
    public GameObject ufoPrefab;
    public GameObject laserPrefab;

    [Header("Ground Settings")]
    public LayerMask groundLayer;

    private Transform target;
    private bool isAttacking = false;
    private bool fightStarted = false;

    private void Start()
    {
        hitbox.enabled = false;
    }

    private void Update()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            float distance = Vector2.Distance(transform.position, target.position);

            if (distance <= detectionRange)
            {
                if (!fightStarted)
                {
                    fightStarted = true;
                    AudioManager.Instance.PlaySFX("Level12BossIntro");
                    AudioManager.Instance.StopBGM();
                    AudioManager.Instance.PlayBGM("Level12BossPhase1");

                    detectionRange = detectionRange << 1; // shift left, double by 1 time
                                                          // increase detection range, so boss doesnt lose player after starting fight
                    StartAttackSequence();
                }

                // move towards player only if not currently doing an attack
                if (!isAttacking)
                {
                    MoveTowardsPlayer();
                }
            }
        }
        else
        {
            target = null;
        }

        EnemyBase enemy = GetComponent<EnemyBase>();
        if (enemy.health <= 50 && !isAttacking)
        {
            Instantiate(prefabBossPhase2, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void StartAttackSequence()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackLoop());
        }
    }

    private IEnumerator AttackLoop()
    {
        isAttacking = true;

        while (true)
        {
            if (target == null) { isAttacking = false; yield break; }

            // 1. Summon 5 Meteorites
            yield return StartCoroutine(Attack_Meteorites());
            yield return new WaitForSeconds(1f);

            // 2. Summon 2-7 UFOs
            yield return StartCoroutine(Attack_UFOs());
            yield return new WaitForSeconds(1f);

            // 3. Slam towards player
            yield return StartCoroutine(Attack_Slam());
            yield return new WaitForSeconds(1f);

            // 4. Summon offset lasers
            yield return StartCoroutine(Attack_Lasers());
            yield return new WaitForSeconds(1f);

            // 5. Slam towards player again
            yield return StartCoroutine(Attack_Slam());

            // pause before restart loop
            isAttacking = false;
            yield return new WaitForSeconds(1.8f);
        }
    }

    // --- ATTACK 1: METEORITES ---
    private IEnumerator Attack_Meteorites()
    {
        if (meteoritePrefabs.Length == 0) yield break;
        isAttacking = true;

        for (int i = 0; i < 7; i++)
        {
            if (target == null) yield break;
            isAttacking = true;

            // get player position + slight random X offset
            float offsetX = Random.Range(-3f, 3f);
            Vector3 spawnPos = target.position + new Vector3(offsetX, 6f, 0f);

            int randomIndex = Random.Range(0, meteoritePrefabs.Length);
            Instantiate(meteoritePrefabs[randomIndex], spawnPos, Quaternion.identity);

            isAttacking = false; // allow movement between meteorite spawns
            yield return new WaitForSeconds(1f);
        }

        isAttacking = false;
    }

    // --- ATTACK 2: UFOs ---
    private IEnumerator Attack_UFOs()
    {
        isAttacking = true;

        // Randomly choose between 2 and 7 UFOs
        int ufoCount = Random.Range(2, 8);

        for (int i = 0; i < ufoCount; i++)
        {
            if (target == null) yield break;
            isAttacking = true;

            // pick a random point in a circle around the player
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = 5f;
            Vector3 circleOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector3 desiredPos = target.position + circleOffset;

            // prevent spawn in ground, shoot raycast down from the sky to find the ground
            RaycastHit2D groundHit = Physics2D.Raycast(desiredPos + Vector3.up * 2f, Vector2.down, 50f, groundLayer);

            if (groundHit.collider != null)
            {
                // spawn the UFO slightly above the ground
                desiredPos.y = groundHit.point.y + 1f;
            }
            else
            {
                // fallback if no ground is found, spawn at player height + a bit up
                desiredPos.y = target.position.y + 1f;
            }

            Instantiate(ufoPrefab, desiredPos, Quaternion.identity);

            isAttacking = false; // allow movement between UFO spawns
            yield return new WaitForSeconds(0.4f);
        }

        isAttacking = false;
    }

    // --- ATTACK 3 & 5: SLAM ---
    private IEnumerator Attack_Slam()
    {
        if (target == null) yield break;
        isAttacking = true;

        Vector3 skyPosition = transform.position; // save old pos

        Vector3 groundTarget = new Vector3(target.position.x, transform.position.y, 0f);
        RaycastHit2D groundHit = Physics2D.Raycast(groundTarget + Vector3.up * 20f, Vector2.down, 50f, groundLayer);

        if (groundHit.collider == null) yield break;

        Vector3 groundPosition = new Vector3(target.position.x, groundHit.point.y + 3f, 0f);

        // wind up effect
        float shakeTime = 1f;
        float shakeIntensity = 0.25f;
        float shakeTimer = 0f;

        AudioManager.Instance.PlaySFX("BossSlamWindUp");

        while (shakeTimer < shakeTime)
        {
            Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity;

            transform.position = skyPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            shakeTimer += Time.deltaTime;
            yield return null;
        }
        transform.position = skyPosition;

        // dive fast
        hitbox.enabled = true;
        float diveSpeed = 40f;
        while (Vector2.Distance(transform.position, groundPosition) > 0.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, groundPosition, diveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = groundPosition;

        ParticleEmitter.Instance.Emit("BigSmoke", groundPosition, Quaternion.identity);
        CameraController.Instance.CamShake(0.5f, 0.25f);
        AudioManager.Instance.PlaySFX("BossSlamImpact", 0.1f);

        yield return null; // wait a frame
        hitbox.enabled = false;

        // stay on ground for
        yield return new WaitForSeconds(2f);

        // fly back up
        float returnSpeed = 25f;
        while (Vector2.Distance(transform.position, skyPosition) > 0.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, skyPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = skyPosition;

        isAttacking = false;
    }

    // --- ATTACK 4: LASERS ---
    private IEnumerator Attack_Lasers()
    {
        if (target == null || laserPrefab == null) yield break;
        isAttacking = true;

        // Spawn 4 lasers
        for (int i = 0; i < 6; i++)
        {
            isAttacking = true;

            // Slight circle offset around player
            float angle = (i == 0) ? 90f * Mathf.Deg2Rad : 270f * Mathf.Deg2Rad;
            float radius = 3f;
            Vector3 spawnPos = target.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);

            // 50% chance to sharply rotate 90 degrees (making it vertical)
            if (Random.value > 0.5f)
            {
                laser.transform.Rotate(0, 0, 90f);
            }

            isAttacking = false;
            yield return new WaitForSeconds(1f);
        }

        isAttacking = false;
    }

    private void MoveTowardsPlayer()
    {
        if (target == null) return;

        float dir = target.position.x - transform.position.x;
        Vector2 moveDir = (dir > 0) ? Vector2.right : Vector2.left;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        float distanceX = Mathf.Abs(transform.position.x - target.position.x);

        if (distanceX > 2.5f)
        {
            if (dir > 0 && transform.localScale.x < 0) Flip();
            else if (dir < 0 && transform.localScale.x > 0) Flip();
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}