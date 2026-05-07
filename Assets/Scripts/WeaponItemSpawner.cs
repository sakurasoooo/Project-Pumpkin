using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单一武器道具生成器：每次生成一个随机高度的道具气泡，从右向左滚动，掉出屏幕则销毁。
/// 多个生成器实例（机枪/玉米）独立配置间隔与高度区间。
/// 仅驱动 X 轴滚动，Y 轴交给 WeaponItem 的浮动逻辑。
/// </summary>
public class WeaponItemSpawner : MonoBehaviour
{
    [Header("Item Prefab")]
    public GameObject itemPrefab;

    [Header("Spawn Range (本地坐标)")]
    public float spawnX = 12f;
    public float despawnX = -12f;
    public float minY = 2f;
    public float maxY = 3.5f;

    [Header("Spawn Interval (seconds)")]
    public float minInterval = 8f;
    public float maxInterval = 14f;

    [Tooltip("到达间隔时间后，按概率决定是否真的生成；用于让节奏更随机")]
    [Range(0f, 1f)] public float spawnChance = 0.7f;

    [Header("Movement")]
    [Tooltip("视差因子：建议 1.0 与地面同步")]
    public float parallaxFactor = 1f;

    [Header("Initial Delay")]
    public float initialDelay = 4f;

    private readonly List<Transform> active = new List<Transform>();
    private float spawnTimer;

    void Start()
    {
        spawnTimer = initialDelay + Random.Range(0f, maxInterval);
    }

    void Update()
    {
        if (itemPrefab == null) return;

        float baseSpeed = (GameManager.Instance != null ? GameManager.Instance.GlobalSpeed : 5f) * parallaxFactor;
        float dt = Time.deltaTime;

        // 滚动 / 回收
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var t = active[i];
            if (t == null) { active.RemoveAt(i); continue; }
            Vector3 p = t.position;
            p.x -= baseSpeed * dt;
            t.position = new Vector3(p.x, t.position.y, p.z);
            if (p.x < despawnX)
            {
                Destroy(t.gameObject);
                active.RemoveAt(i);
            }
        }

        // 生成
        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            spawnTimer = Random.Range(minInterval, maxInterval);
            if (Random.value <= spawnChance)
            {
                SpawnItem();
            }
        }
    }

    private void SpawnItem()
    {
        Vector3 spawnPos = new Vector3(spawnX, Random.Range(minY, maxY), 0f);
        GameObject go = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        active.Add(go.transform);
    }
}
