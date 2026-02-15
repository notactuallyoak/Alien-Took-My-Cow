using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.25f;
    public Vector3 offset = new Vector3(0, 2, -10);

    [Header("Look Ahead")]
    public float lookAheadDistance = 4f;
    public float lookAheadSpeed = 2f;

    private float currentLookAhead;
    private Rigidbody2D targetRb;

    void Start()
    {
        if (target != null) targetRb = target.GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Determine direction based on player velocity
        float targetLookAhead = 0;
        if (Mathf.Abs(targetRb.linearVelocity.x) > 1f)
        {
            targetLookAhead = Mathf.Sign(targetRb.linearVelocity.x) * lookAheadDistance;
        }

        // 2. Smoothly interpolate the look-ahead offset
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        // 3. Calculate final position
        Vector3 desiredPosition = target.position + offset + new Vector3(currentLookAhead, 0, 0);

        // 4. Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}
