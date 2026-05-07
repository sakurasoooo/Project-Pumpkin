using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成器：从右侧外随机生成 1~N 个敌人，按 GameManager.GlobalSpeed 向左滚动，
/// 离开屏幕左侧后销毁。生成节奏随时间逐步加快，给出动态难度递增。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        [Tooltip("权重，权重越大越常出现")] public float weight = 1f;
        [Tooltip("勾选后使用本敌人专属高度区间，否则使用 spawner 的 groundY")] public bool overrideY = false;
        public float minY = 2f;
        public float maxY = 3f;
    }

    [Header("Enemy Prefabs")]
    public EnemyEntry[] enemies;

    [Header("Spawn Range (世界坐标)")]
    public float spawnX = 12f;
    public float despawnX = -14f;
    public float groundY = -1.1f;

    [Header("Spawn Group (一波生成 N 个)")]
    public int minPerWave = 1;
    public int maxPerWave = 3;
    [Tooltip("同一波内多个敌人之间的横向间距")]
    public float clusterSpacing = 1.4f;

    [Header("Spawn Interval (seconds)")]
    public float minInterval = 1.8f;
    public float maxInterval = 3.5f;

    [Header("Difficulty Ramp")]
    [Tooltip("游戏时间每过这么多秒，间隔倍率减少 intervalDecayPerStep")]
    public float rampStepSeconds = 20f;
    [Tooltip("每个 step 后间隔倍率乘以多少（<=1 加快）")]
    public float intervalDecayPerStep = 0.9f;
    [Tooltip("最小间隔倍率，防止无限加速")]
    public float minIntervalScale = 0.35f;

    [Header("Initial Delay")]
    public float initialDelay = 1.5f;

    private struct SpawnedEnemy { public Transform t; }
    private readonly List<SpawnedEnemy> active = new List<SpawnedEnemy>();
    private float spawnTimer;
    private float elapsed;
    private float totalWeight;

    void Start()
    {
        spawnTimer = initialDelay;
        RecalculateWeight();

        if (enemies == null || enemies.Length == 0 || totalWeight <= 0f)
        {
            Debug.LogWarning("[EnemySpawner] enemies 列表为空或所有权重为 0，无法生成敌人。");
            return;
        }

        SpawnWave();
    }

    private void RecalculateWeight()
    {
        totalWeight = 0f;
        if (enemies == null) return;
        foreach (var e in enemies)
        {
            if (e != null && e.prefab != null) totalWeight += Mathf.Max(0f, e.weight);
        }
    }

    void Update()
    {
        if (enemies == null || enemies.Length == 0) return;

        float dt = Time.deltaTime;
        elapsed += dt;

        float baseSpeed = GameManager.Instance != null ? GameManager.Instance.GlobalSpeed : 5f;

        // 滚动 / 回收
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var e = active[i];
            if (e.t == null) { active.RemoveAt(i); continue; }
            Vector3 p = e.t.position;
            p.x -= baseSpeed * dt;
            e.t.position = p;
            if (p.x < despawnX)
            {
                Destroy(e.t.gameObject);
                active.RemoveAt(i);
            }
        }

        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            SpawnWave();
            float scale = Mathf.Max(minIntervalScale, Mathf.Pow(intervalDecayPerStep, elapsed / Mathf.Max(0.01f, rampStepSeconds)));
            spawnTimer = Random.Range(minInterval, maxInterval) * scale;
        }
    }

    private void SpawnWave()
    {
        int count = Random.Range(minPerWave, maxPerWave + 1);
        for (int i = 0; i < count; i++)
        {
            EnemyEntry entry = PickRandomEntry();
            if (entry == null || entry.prefab == null) continue;

            float y = entry.overrideY ? Random.Range(entry.minY, entry.maxY) : groundY;
            Vector3 pos = new Vector3(spawnX + i * clusterSpacing, y, 0f);
            GameObject go = Instantiate(entry.prefab, pos, Quaternion.identity);
            active.Add(new SpawnedEnemy { t = go.transform });
        }
    }

    private EnemyEntry PickRandomEntry()
    {
        if (totalWeight <= 0f) RecalculateWeight();
        if (totalWeight <= 0f) return null;

        float r = Random.value * totalWeight;
        foreach (var e in enemies)
        {
            if (e == null || e.prefab == null) continue;
            r -= Mathf.Max(0f, e.weight);
            if (r <= 0f) return e;
        }
        return enemies[enemies.Length - 1];
    }
}
