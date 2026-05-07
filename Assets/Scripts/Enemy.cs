using UnityEngine;
using DG.Tweening;

/// <summary>
/// 敌人受击表现：颜色闪白、缩放冲击、击退、死亡缩放淡出
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
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

    public void TakeHit(Vector2 fromDir)
    {
        if (dying) return;
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
    }

    private void Die()
    {
        dying = true;
        if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreOnKill);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0f, deathDuration).SetEase(Ease.InBack));
        seq.Join(sr.DOFade(0f, deathDuration));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
