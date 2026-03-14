using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    public static ParticleEmitter Instance;

    [System.Serializable]
    public class ParticleItem
    {
        public string name;
        public GameObject prefab;
    }

    [Header("Particle Registry")]
    public ParticleItem[] particles;
    private Dictionary<string, GameObject> particleDict;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // build the dictionary from the array
        particleDict = new Dictionary<string, GameObject>();
        foreach (var p in particles)
        {
            if (!string.IsNullOrEmpty(p.name) && p.prefab != null)
            {
                particleDict[p.name] = p.prefab;
            }
        }
    }

    // using name (look up in dictionary)
    public void Emit(string particleName, Vector3 position, Quaternion quaternion)
    {
        if (particleDict.TryGetValue(particleName, out GameObject prefab))
        {
            Instantiate(prefab, position, quaternion);
        }
    }

    public void Emit(string particleName, Vector3 position, bool flipX)
    {
        if (particleDict.TryGetValue(particleName, out GameObject prefab))
        {
            GameObject particle = Instantiate(prefab, position, Quaternion.identity);

            if (flipX)
            {
                Vector3 scale = particle.transform.localScale;
                scale.x *= -1;
                particle.transform.localScale = scale;
            }
        }
    }

    // original methods (pass prefab directly)
    public void Emit(GameObject prefab, Vector3 position, Quaternion quaternion)
    {
        if (prefab == null) return;

        Instantiate(prefab, position, quaternion);
    }

    public void Emit(GameObject prefab, Vector3 position, bool flipX)
    {
        if (prefab == null) return;

        GameObject particle = Instantiate(prefab, position, Quaternion.identity);

        if (flipX)
        {
            Vector3 scale = particle.transform.localScale;
            scale.x *= -1;
            particle.transform.localScale = scale;
        }
    }
}