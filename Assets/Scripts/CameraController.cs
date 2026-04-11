using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private Transform target;
    public float smoothSpeed;
    public Vector3 offset;

    [Header("Look Ahead")]
    public float lookAheadDistance;
    public float lookAheadSpeed;

    private float currentLookAhead;
    private Rigidbody2D targetRb;

    // cam shake
    private Vector2 shakeOffset;
    private float shakeTimeRemaining;
    private float shakePower;
    private float shakeFadeTime;

    private Camera cam;
    private Coroutine sizeChangeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        if (target != null) targetRb = target.GetComponent<Rigidbody2D>();
        shakeOffset = Vector2.zero;
    }
    private void Update()
    {
        if (target == null) return;

        // determine direction based on player velocity
        float targetLookAhead = 0;
        if (Mathf.Abs(targetRb.linearVelocity.x) > 1f)
        {
            targetLookAhead = Mathf.Sign(targetRb.linearVelocity.x) * lookAheadDistance;
        }   

        // smoothly interpolate the look-ahead offset
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        // cam shake logic
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.deltaTime;

            float currentPower = Mathf.Lerp(shakePower, 0, 1 - (shakeTimeRemaining / shakeFadeTime));
            shakeOffset = Random.insideUnitCircle.normalized * currentPower;
        }
        else
        {
            shakeOffset = Vector2.Lerp(shakeOffset, Vector2.zero, smoothSpeed * Time.unscaledDeltaTime);
        }

        // calc final desired position with look-ahead + offset
        Vector3 desiredPosition = target.position + offset + new Vector3(currentLookAhead, 0, 0) + (Vector3)shakeOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    public void CamShake(float intensity = 0.1f, float duration = 0.1f)
    {
        shakePower = intensity;
        shakeTimeRemaining = duration;
        shakeFadeTime = duration;
    }

    public void ChangeCameraSize(float targetSize, float duration)
    {
        if (sizeChangeCoroutine != null) StopCoroutine(sizeChangeCoroutine);
        sizeChangeCoroutine = StartCoroutine(SizeChangeRoutine(targetSize, duration));
    }

    private IEnumerator SizeChangeRoutine(float targetSize, float duration)
    {
        float startSize = cam.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // SmoothStep makes it ease in/out beautifully
            float progress = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cam.orthographicSize = targetSize;
    }
}
