using UnityEngine;
using System.Collections;

public class BossPhase2 : MonoBehaviour
{
    [Header("Detection & Movement")]
    private int detectionRange = 32;
    public float moveSpeed;

    [Header("Prefabs")]
    public Collider2D hitbox;
    public GameObject finalGoalEndGame;

    [Header("Prefabs")]
    public GameObject[] meteoritePrefabs;
    public GameObject alienPrefab;
    public GameObject ufoPrefab;
    public GameObject laserPrefab;

    [Header("Ground Settings")]
    public LayerMask groundLayer;

    private Transform target;
    private bool isAttacking = false;
    private bool fightStarted = false;
    private bool isDead = false;

    private void Start()
    {
        hitbox.enabled = false;

        // spawn effect
        ParticleEmitter.Instance.Emit("BossPhaseTransition", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("BossPhaseChange");
        AudioManager.Instance.StopBGM();
        AudioManager.Instance.PlayBGM("Level12BossPhase2");
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
                    StartAttackSequence();
                }

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

        // dead
        EnemyBase enemy = GetComponent<EnemyBase>();
        if (enemy.health <= 10 && !isDead)
        {
            isDead = true;
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        Vector3 originalPos = transform.position;

        // SHAKE
        float shakeTime = 3f;
        float shakeIntensity = 0.5f;
        float shakeTimer = 0f;

        AudioManager.Instance.PlaySFX("BossSlamWindUp");

        while (shakeTimer < shakeTime)
        {
            Vector2 randomOffset = Random.insideUnitCircle * shakeIntensity;
            transform.position = originalPos + new Vector3(randomOffset.x, randomOffset.y, 0f);
            shakeTimer += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;

        // FALL TO GROUND
        Vector2 groundCheck = new Vector2(transform.position.x, transform.position.y);
        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck + Vector2.up * 5f, Vector2.down, 50f, groundLayer);

        if (groundHit.collider == null) { isAttacking = false; yield break; }

        Vector3 groundPos = new Vector3(transform.position.x, groundHit.point.y, 0f);
        float fallSpeed = 30f;

        while (Vector2.Distance(transform.position, groundPos) > 0.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, groundPos, fallSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = groundPos;

        ParticleEmitter.Instance.Emit("BigSmoke", groundPos, Quaternion.identity);
        CameraController.Instance.CamShake(0.66f, 0.3f);
        AudioManager.Instance.PlaySFX("BossSlamImpact", 0.1f);

        ParticleEmitter.Instance.Emit("BossPhaseTransition", transform.position, Quaternion.identity);
        ParticleEmitter.Instance.Emit("DeadStar", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("BossDead");

        Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
        Instantiate(finalGoalEndGame, spawnPos, Quaternion.identity);

        Destroy(gameObject);
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

            // 1. MORE METEORITES (7 instead of 5, faster delay)
            yield return StartCoroutine(Attack_Meteorites());
            yield return new WaitForSeconds(0.5f); // less downtime

            // 2. ALIENS
            yield return StartCoroutine(Attack_Aliens());
            yield return new WaitForSeconds(0.5f);

            // 3. MORE UFOS (3-8 instead of 2-7)
            yield return StartCoroutine(Attack_UFOs());
            yield return new WaitForSeconds(0.5f);

            // 4. SLAM (Shorter wind up, faster dive)
            yield return StartCoroutine(Attack_Slam());
            yield return new WaitForSeconds(0.5f);

            // 5. CHAOS LASERS (3 lasers, random 0/45/90/135/180/225/270/315/360 degree spins)
            yield return StartCoroutine(Attack_Lasers());
            yield return new WaitForSeconds(0.5f);

            // 6. DOUBLE SLAM (Slams twice in a row with almost no break)
            yield return StartCoroutine(Attack_Slam());
            yield return StartCoroutine(Attack_Slam());

            isAttacking = false;
            yield return new WaitForSeconds(1f); // shorter overall loop pause
        }
    }

    private IEnumerator Attack_Meteorites()
    {
        if (meteoritePrefabs.Length == 0) yield break;
        isAttacking = true;

        for (int i = 0; i < 7; i++) // 5 => 7 meteorites
        {
            if (target == null) yield break;
            isAttacking = true;

            float offsetX = Random.Range(-3f, 3f); // winder range
            Vector3 spawnPos = target.position + new Vector3(offsetX, 5f, 0f);

            int randomIndex = Random.Range(0, meteoritePrefabs.Length);
            Instantiate(meteoritePrefabs[randomIndex], spawnPos, Quaternion.identity);

            isAttacking = false; // allow movement between meteorite spawns
            yield return new WaitForSeconds(0.75f); // less downtime between meteorites
        }

        isAttacking = false;
    }

    private IEnumerator Attack_Aliens()
    {
        if (target == null) yield break;
        isAttacking = true;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = 2f;
        Vector3 circleOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        Vector3 desiredPos = target.position + circleOffset;

        RaycastHit2D groundHit = Physics2D.Raycast(desiredPos + Vector3.up * 2f, Vector2.down, 50f, groundLayer);

        if (groundHit.collider != null)
        {
            desiredPos.y = groundHit.point.y + 0.5f;

            Instantiate(alienPrefab, desiredPos, Quaternion.identity);
            ParticleEmitter.Instance.Emit("WhiteFlash", desiredPos, Quaternion.identity);
            AudioManager.Instance.PlaySFX("ObjectSpawn");
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Attack_UFOs()
    {
        isAttacking = true;

        int ufoCount = Random.Range(3, 9); // 3 to 8 UFOs

        for (int i = 0; i < ufoCount; i++)
        {
            if (target == null) yield break;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = 6f; // Spawn slightly further out
            Vector3 circleOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector3 desiredPos = target.position + circleOffset;

            RaycastHit2D groundHit = Physics2D.Raycast(desiredPos + Vector3.up * 2f, Vector2.down, 50f, groundLayer);

            if (groundHit.collider != null)
            {
                desiredPos.y = groundHit.point.y + 1f;
            }
            else
            {
                desiredPos.y = target.position.y + 1f;
            }

            Instantiate(ufoPrefab, desiredPos, Quaternion.identity);
            yield return new WaitForSeconds(0.1f); // Spawn them much faster together
        }
    }

    private IEnumerator Attack_Slam()
    {
        if (target == null) yield break;
        isAttacking = true;

        Vector3 skyPosition = transform.position;
        Vector3 groundTarget = new Vector3(target.position.x, transform.position.y, 0f);
        RaycastHit2D groundHit = Physics2D.Raycast(groundTarget + Vector3.up * 20f, Vector2.down, 50f, groundLayer);

        if (groundHit.collider == null) { isAttacking = false; yield break; }

        Vector3 groundPosition = new Vector3(target.position.x, groundHit.point.y, 0f);

        // AGGRESSIVE WIND UP (Shakes faster/harder for only 0.5 seconds)
        float shakeTime = 0.5f;
        float shakeIntensity = 0.3f;
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

        ParticleEmitter.Instance.Emit("BigSmoke", groundPosition, Quaternion.identity);
        CameraController.Instance.CamShake(0.66f, 0.3f);
        AudioManager.Instance.PlaySFX("BossSlamImpact", 0.1f);

        // FASTER DIVE
        hitbox.enabled = true;
        float diveSpeed = 65f;
        while (Vector2.Distance(transform.position, groundPosition) > 0.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, groundPosition, diveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = groundPosition;

        yield return null; // wait a frame
        hitbox.enabled = false;

        yield return new WaitForSeconds(1.25f); // less stay on the ground

        // FASTER RETURN
        float returnSpeed = 40f;
        while (Vector2.Distance(transform.position, skyPosition) > 0.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, skyPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = skyPosition;

        isAttacking = false;
    }

    private IEnumerator Attack_Lasers()
    {
        if (target == null || laserPrefab == null) yield break;
        isAttacking = true;

        // Spawn 6 lasers in a triangle around the player
        for (int i = 0; i < 6; i++)
        {
            isAttacking = true;

            float angle = (i * 120f) * Mathf.Deg2Rad; // 0, 120, 240 degrees
            float radius = 2f;
            Vector3 spawnPos = target.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);

            // RANDOMLY ROTATE 0, 90, 180, OR 270 DEGREES
            int randomRotation = Random.Range(0, 8) * 45;
            laser.transform.Rotate(0, 0, randomRotation);

            isAttacking = false;
            yield return new WaitForSeconds(0.5f);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
