using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public MovementType typeOfMovement;
    public Vector3 customDirection;
    public float distance;
    public float speed;
    public float waitTime;

    public enum MovementType
    {
        LeftRight,
        UpDown,
        Diagonal,
        DiagonalAlt,
        Custom
    }

    private Vector3 centerPos;
    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 targetPos;
    private float waitTimer;

    void Start()
    {
        centerPos = transform.position;
        CalculatePoints();

        targetPos = pointA;
    }

    void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        // move
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // switch dir when reached
        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            waitTimer = waitTime;
            targetPos = (targetPos == pointA) ? pointB : pointA; // toggle target
        }
    }

    private void CalculatePoints()
    {
        Vector3 dir = Vector3.zero;

        switch (typeOfMovement)
        {
            case MovementType.LeftRight: dir = Vector3.right; break;
            case MovementType.UpDown: dir = Vector3.up; break;
            case MovementType.Diagonal: dir = new Vector3(1, 1, 0); break;
            case MovementType.DiagonalAlt: dir = new Vector3(-1, 1, 0); break;
            case MovementType.Custom: dir = customDirection.normalized; break;
        }

        // expanding from center
        pointA = centerPos + dir.normalized * (distance / 2f);
        pointB = centerPos - dir.normalized * (distance / 2f);
    }

    private void OnValidate()
    {
        centerPos = transform.position;
        CalculatePoints();
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            centerPos = transform.position;
            CalculatePoints();
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPos, 0.25f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pointB, 0.3f);

        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawCube(pointA, transform.localScale);

        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawCube(pointB, transform.localScale);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(this.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}