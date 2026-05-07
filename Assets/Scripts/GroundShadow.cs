using UnityEngine;

/// <summary>
/// 地面阴影投影脚本：
/// - 每帧跟随目标角色的 X 轴位置
/// - 用向下射线找到地面表面，将阴影锁定在地面 Y 坐标
/// - 根据角色离地高度动态缩放和调整透明度
/// </summary>
public class GroundShadow : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("要跟随的角色 Transform")]
    public Transform target;

    [Header("地面检测")]
    [Tooltip("地面检测层")]
    public LayerMask groundLayer;
    [Tooltip("射线最大检测距离")]
    public float maxRayDistance = 20f;
    [Tooltip("阴影离地面的微小偏移（防止Z-fighting闪烁）")]
    public float groundOffset = 0.01f;

    [Header("高度自适应效果")]
    [Tooltip("角色达到此高度时阴影最小")]
    public float maxHeight = 5f;
    [Tooltip("阴影最小缩放比例")]
    public float minScale = 0.4f;
    [Tooltip("阴影最小透明度")]
    public float minAlpha = 0.2f;
    [Tooltip("阴影在地面时的透明度")]
    public float maxAlpha = 0.6f;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private Color baseColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseScale = transform.localScale;
            baseColor = spriteRenderer.color;
        }
    }

    void LateUpdate()
    {
        if (target == null || spriteRenderer == null) return;

        // 用 RaycastAll 从角色位置向下找地面，过滤掉角色自身的碰撞体
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            target.position, Vector2.down, maxRayDistance, groundLayer);

        RaycastHit2D? groundHit = null;
        foreach (var h in hits)
        {
            // 跳过角色自身及其子物体的碰撞体
            if (h.collider != null && !h.collider.transform.IsChildOf(target))
            {
                groundHit = h;
                break;
            }
        }

        if (groundHit.HasValue)
        {
            var hit = groundHit.Value;

            // 将阴影放在地面表面
            transform.position = new Vector3(
                target.position.x,
                hit.point.y + groundOffset,
                transform.position.z);

            // 计算角色离地高度
            float height = Mathf.Max(0, target.position.y - hit.point.y);
            float heightRatio = Mathf.Clamp01(height / maxHeight);

            // 高度越大，阴影越小
            float scaleMult = Mathf.Lerp(1f, minScale, heightRatio);
            transform.localScale = new Vector3(
                baseScale.x * scaleMult,
                baseScale.y * scaleMult,
                baseScale.z);

            // 高度越大，阴影越透明
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, heightRatio);
            Color c = baseColor;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }
}
