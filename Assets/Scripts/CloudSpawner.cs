using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在滚轴背景中随机生成飘动的云。
/// 从右侧屏幕外随机生成（云1~4），向左飘出后销毁。
/// 速度跟随 GameManager.GlobalSpeed * parallaxFactor，营造视差。
/// </summary>
public class CloudSpawner : MonoBehaviour
{
    [Header("Sprites (云1~云4)")]
    public Sprite[] cloudSprites;

    [Header("Spawn Range (本地坐标)")]
    public float spawnX = 12f;
    public float despawnX = -12f;
    public float minY = 1.5f;
    public float maxY = 4.5f;

    [Header("Spawn Interval (seconds)")]
    public float minInterval = 1.2f;
    public float maxInterval = 3.5f;

    [Header("Scale & Alpha")]
    public float minScale = 0.5f;
    public float maxScale = 1.0f;
    public float minAlpha = 0.6f;
    public float maxAlpha = 1.0f;

    [Header("Movement")]
    [Tooltip("视差因子：建议小于地面层，0.2~0.5 比较自然")]
    public float parallaxFactor = 0.35f;
    [Tooltip("单云速度差异随机区间")]
    public float minSpeedMul = 0.85f;
    public float maxSpeedMul = 1.15f;

    [Header("Sorting")]
    [Tooltip("背景为 -10，草地/泥土为 0，云建议在 -5 ~ -1 之间")]
    public int sortingOrder = -5;

    [Header("Prewarm")]
    [Tooltip("启动时预生成几片云填充屏幕")]
    public int prewarmCount = 5;

    private struct Cloud
    {
        public Transform t;
        public float speedMul;
    }

    private readonly List<Cloud> active = new List<Cloud>();
    private float spawnTimer;

    void Start()
    {
        if (cloudSprites == null || cloudSprites.Length == 0) return;

        for (int i = 0; i < prewarmCount; i++)
        {
            float x = Random.Range(despawnX + 2f, spawnX - 2f);
            SpawnCloud(x);
        }
        spawnTimer = Random.Range(minInterval, maxInterval);
    }

    void Update()
    {
        if (cloudSprites == null || cloudSprites.Length == 0) return;

        float baseSpeed = (GameManager.Instance != null ? GameManager.Instance.GlobalSpeed : 5f) * parallaxFactor;
        float dt = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var c = active[i];
            if (c.t == null) { active.RemoveAt(i); continue; }
            Vector3 p = c.t.localPosition;
            p.x -= baseSpeed * c.speedMul * dt;
            c.t.localPosition = p;
            if (p.x < despawnX)
            {
                Destroy(c.t.gameObject);
                active.RemoveAt(i);
            }
        }

        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            SpawnCloud(spawnX);
            spawnTimer = Random.Range(minInterval, maxInterval);
        }
    }

    private void SpawnCloud(float startX)
    {
        Sprite sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
        if (sprite == null) return;

        var go = new GameObject("Cloud");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(startX, Random.Range(minY, maxY), 0f);
        float s = Random.Range(minScale, maxScale);
        go.transform.localScale = new Vector3(s, s, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        Color col = Color.white;
        col.a = Random.Range(minAlpha, maxAlpha);
        sr.color = col;
        if (Random.value < 0.5f) sr.flipX = true;

        active.Add(new Cloud
        {
            t = go.transform,
            speedMul = Random.Range(minSpeedMul, maxSpeedMul)
        });
    }
}
