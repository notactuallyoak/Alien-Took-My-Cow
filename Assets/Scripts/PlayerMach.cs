using UnityEngine;

public class PlayerMach : MonoBehaviour
{
    public float maxMachSpeed = 30f;
    public float acceleration = 25f;
    public float deceleration = 40f;

    public float mach2Threshold = 15f;
    public float mach3Threshold = 25f;

    public int MachLevel { get; private set; }
    public float CurrentSpeed { get; private set; }

    void Update()
    {
        HandleMach();
        UpdateMachLevel();
    }

    void HandleMach()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            CurrentSpeed += acceleration * Time.deltaTime;
        else
            CurrentSpeed -= deceleration * Time.deltaTime;

        CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0, maxMachSpeed);
    }

    void UpdateMachLevel()
    {
        if (CurrentSpeed >= mach3Threshold)
            MachLevel = 3;
        else if (CurrentSpeed >= mach2Threshold)
            MachLevel = 2;
        else if (CurrentSpeed > 1f)
            MachLevel = 1;
        else
            MachLevel = 0;
    }

    //void OnCollisionEnter2D(Collision2D col)
    //{
    //    if (MachLevel >= 2 && col.gameObject.CompareTag("Enemy"))
    //    {
    //        col.gameObject.GetComponent<Enemy>()?.TakeDamage(999);
    //    }

    //    if (MachLevel >= 3 && col.gameObject.CompareTag("Breakable"))
    //    {
    //        Destroy(col.gameObject);
    //    }
    //}
}