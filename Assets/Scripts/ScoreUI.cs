using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 分数 UI：等 GameManager 单例就绪后订阅 OnScoreChanged，刷新 TMP；分数变化时有缩放/闪烁反馈。
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreUI : MonoBehaviour
{
    [Header("Format")]
    public string format = "积分: {0}";

    [Header("Punch")]
    public Vector3 punchScale = new Vector3(0.4f, 0.4f, 0f);
    public float punchDuration = 0.35f;
    public int punchVibrato = 8;
    public float punchElasticity = 0.7f;

    [Header("Color Flash")]
    public Color flashColor = new Color(1f, 0.95f, 0.3f, 1f);
    public float flashDuration = 0.25f;

    private TextMeshProUGUI text;
    private Color baseColor;
    private Vector3 baseScale;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        baseColor = text.color;
        baseScale = transform.localScale;
    }

    private IEnumerator Start()
    {
        while (GameManager.Instance == null)
            yield return null;

        GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
        GameManager.Instance.OnScoreChanged += HandleScoreChanged;

        ApplyScoreText(GameManager.Instance.score, playFeedback: false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int newScore)
    {
        ApplyScoreText(newScore, playFeedback: true);
    }

    private void ApplyScoreText(int newScore, bool playFeedback)
    {
        text.text = string.Format(format, newScore);
        if (!playFeedback)
            return;

        transform.DOKill();
        transform.localScale = baseScale;
        transform.DOPunchScale(punchScale, punchDuration, punchVibrato, punchElasticity)
                 .SetUpdate(true);

        text.DOKill();
        text.color = flashColor;
        text.DOColor(baseColor, flashDuration).SetUpdate(true);
    }
}
