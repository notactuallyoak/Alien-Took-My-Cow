using UnityEngine;

public class ExpandCam : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float targetCameraSize;
    public float zoomDuration;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraController.Instance.ChangeCameraSize(targetCameraSize, zoomDuration);
            Destroy(gameObject);
        }
    }
}