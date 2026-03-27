using UnityEngine;
using System.Collections;

public class EnemyUFO : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 5.5f);
    }
}
