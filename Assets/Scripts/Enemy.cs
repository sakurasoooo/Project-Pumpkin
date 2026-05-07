using UnityEngine;
using DG.Tweening;

/// <summary>
/// 敌人受击表现：颜色闪白、缩放冲击、击退、死亡缩放淡出。
/// 与主摄像机视锥不相交时为无敌（可防止提前射击屏幕外来敌）。
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Damagability")]
    [Tooltip("与主摄像机视锥无相交时忽略受击")]
    public bool invulnerableOutsideCamera = true;

    [Tooltip("判定可见时在包围盒上附加的冗余（世界单位），略大于 0 可减少边缘误判")]
    public float cameraBoundsPadding = 0.08f;

    [Header("Score")]
    public int scoreOnKill = 100;

    [Header("Hit Flash")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.08f;

    [Header("Punch Scale")]
    public Vector3 punchScale = new Vector3(0.4f, -0.3f, 0f);
    public float punchDuration = 0.25f;

    [Header("Knockback")]
    public float knockbackDistance = 0.3f;
    public float knockbackDuration = 0.12f;

    [Header("Death")]
    public float deathDuration = 0.18f;

    private SpriteRenderer sr;
    private Color originalColor;
    private bool dying;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    /// <returns>已成功进入受击/死亡流程</returns>
    public bool TakeHit(Vector2 fromDir)
    {
        if (dying) return false;
        if (invulnerableOutsideCamera && !OverlapsMainCameraFrustum(sr.bounds))
            return false;

        sr.DOKill();
        transform.DOKill();

        sr.color = flashColor;
        sr.DOColor(originalColor, flashDuration);
        transform.DOPunchScale(punchScale, punchDuration, 6, 0.6f);
        if (knockbackDistance > 0f)
        {
            transform.DOBlendableMoveBy((Vector3)fromDir.normalized * knockbackDistance, knockbackDuration);
        }
        Die();
        return true;
    }

    bool OverlapsMainCameraFrustum(Bounds bounds)
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Bounds b = bounds;
        float pad = cameraBoundsPadding;
        if (pad > 0f) b.Expand(pad);

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, b);
    }

    private void Die()
    {
        dying = true;
        if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreOnKill);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var idle = GetComponent<EnemyIdle>();
        if (idle != null) idle.enabled = false;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0f, deathDuration).SetEase(Ease.InBack));
        seq.Join(sr.DOFade(0f, deathDuration));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
