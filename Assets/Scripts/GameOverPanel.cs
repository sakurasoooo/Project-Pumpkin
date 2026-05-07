using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// 游戏结束面板：半透明黑遮罩 + 居中「重新开始」+ 刷新积分文本，播放缩放渐入
/// </summary>
public class GameOverPanel : MonoBehaviour
{
    [Tooltip("结束面板根物体（默认 inactive，游戏结束时激活）")]
    public GameObject root;

    [Header("Score")]
    [Tooltip("结束时显示总分（与 HUD 同源 GameManager.score）")]
    public TextMeshProUGUI scoreText;

    public string scoreFormat = "积分: {0}";

    [Header("Animation")]
    public float scaleInDuration = 0.45f;
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 endScale = Vector3.one;

    void Start()
    {
        if (root != null) root.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += Show;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= Show;
    }

    private void Show()
    {
        if (root == null) return;
        root.SetActive(true);

        if (scoreText != null && GameManager.Instance != null)
            scoreText.text = string.Format(scoreFormat, GameManager.Instance.score);

        Transform t = root.transform;
        t.localScale = startScale;
        t.DOKill();
        t.DOScale(endScale, scaleInDuration)
         .SetEase(Ease.OutBack)
         .SetUpdate(true);
    }
}
