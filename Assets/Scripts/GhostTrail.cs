using UnityEngine;
using System.Collections;

public class GhostTrail : MonoBehaviour
{
    [Header("Settings")]
    private float ghostInterval;
    private float ghostFadeTime;
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.5f);

    private SpriteRenderer playerSprite;

    private GameObject[] ghostPool;
    private int poolSize = 7;
    private int currentIndex = 0;

    private float timer;

    void Start()
    {
        playerSprite = GetComponentInChildren<SpriteRenderer>();

        // create ghost pool
        ghostPool = new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ghost = new GameObject("Ghost_" + i);
            ghost.transform.SetParent(transform.parent); // put in same parent as player

            SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
            sr.sortingLayerID = playerSprite.sortingLayerID;
            sr.sortingOrder = playerSprite.sortingOrder - 1; // make it behind player

            ghost.SetActive(false);
            ghostPool[i] = ghost;
        }
    }

    // called by PlayerController
    public void SpawnGhost(float gInterval = 0.1f, float gFadeTime = 0.1f)
    {
        ghostInterval = gInterval;
        ghostFadeTime = gFadeTime;

        // timer prevent spawn too fast
        if (Time.time < timer) return;
        timer = Time.time + ghostInterval;

        // ghost from pool
        GameObject ghost = ghostPool[currentIndex];
        currentIndex = (currentIndex + 1) % poolSize;

        ghost.transform.position = playerSprite.transform.position;
        ghost.transform.rotation = playerSprite.transform.rotation;
        ghost.transform.localScale = playerSprite.transform.lossyScale; // for facing (flip)

        SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
        sr.sprite = playerSprite.sprite;
        sr.flipX = playerSprite.flipX; // copy facing
        sr.color = ghostColor;

        ghost.SetActive(true);

        StartCoroutine(FadeOut(sr));
    }

    IEnumerator FadeOut(SpriteRenderer sr)
    {
        float t = 0f;
        Color startColor = ghostColor;

        while (t < ghostFadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, t / ghostFadeTime);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        sr.gameObject.SetActive(false);
    }
}