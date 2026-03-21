using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Horizontal Settings")]
    public bool infiniteHorizontal = true;
    [Range(0f, 1f)]
    [Tooltip("0 = Moves with Camera, 1 = Static")]
    public float parallaxEffectMultiplier = 0.5f;

    private Transform cameraTransform;
    private float spriteWidth;
    private Vector3 previousCameraPosition;

    // track the background's target Y position separately
    private float targetBackgroundY;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        previousCameraPosition = cameraTransform.position;

        // initialize the target Y to where the background starts
        targetBackgroundY = transform.position.y - 1.25f;

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        spriteWidth = sprite.texture.width / sprite.pixelsPerUnit;
    }

    private void LateUpdate()
    {
        Vector3 currentCameraPosition = cameraTransform.position;

        // --- 1. HORIZONTAL MOVEMENT (Standard Parallax) ---
        float deltaX = currentCameraPosition.x - previousCameraPosition.x;
        float parallaxX = deltaX * parallaxEffectMultiplier;

        // Move X immediately
        transform.position += Vector3.right * parallaxX;

        // --- 2. VERTICAL MOVEMENT (Threshold Logic) ---

        // Calculate total distance between Camera and Background
        float distanceY = currentCameraPosition.y - targetBackgroundY;

        // Only move Y if the distance exceeds the threshold
        if (Mathf.Abs(distanceY) > 0)
        {
            // Calculate how much to move (remove the threshold "neutral zone")
            float movementY = distanceY - (Mathf.Sign(distanceY) * 0);

            // Apply the vertical parallax multiplier
            targetBackgroundY += movementY * 0.9f;

            // Instantly update Y position (or you can Lerp this for extra smoothness)
            transform.position = new Vector3(transform.position.x, targetBackgroundY, transform.position.z);
        }

        // Update previous camera position for the next frame
        previousCameraPosition = currentCameraPosition;

        // --- 3. INFINITE HORIZONTAL REPEAT ---
        if (infiniteHorizontal)
        {
            float distanceX = currentCameraPosition.x - transform.position.x;

            if (distanceX > spriteWidth)
            {
                transform.position += Vector3.right * spriteWidth;
            }
            else if (distanceX < -spriteWidth)
            {
                transform.position += Vector3.left * spriteWidth;
            }
        }
    }
}