using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// 暂停/继续按钮：点击切换 timeScale；暂停时显示半透明遮罩并把按钮移到屏幕中央，恢复后回到原位。
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class PauseButton : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite pauseSprite;
    public Sprite resumeSprite;

    [Header("Pause Overlay")]
    [Tooltip("全屏半透明灰板（Gameplay 时需关闭）")]
    public GameObject pauseOverlay;

    [Header("Paused Layout")]
    public Vector2 pausedButtonSizeDelta = new Vector2(100f, 100f);

    [Header("Layout Tween")]
    [Tooltip(">0：暂停移到中央时按钮从小到正常（unscaled）；0 则不播")]
    public float pausedButtonAppearScaleTween = 0.22f;

    [Header("Click Feedback")]
    public Vector3 clickPunch = new Vector3(0.25f, 0.25f, 0f);
    public float clickDuration = 0.25f;

    [Header("Paused — continue label")]
    [Tooltip("暂停（继续）状态下显示在中央的 TMP")]
    public TextMeshProUGUI continueLabel;

    [Tooltip("居中显示文案")]
    public string continueText = "继续";

    private Image image;
    private RectTransform rt;
    private bool isPaused;
    private Vector3 baseScale;

    private Vector2 playAnchorMin, playAnchorMax, playPivot, playAnchoredPosition, playSizeDelta;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        if (pauseSprite != null) image.sprite = pauseSprite;
        baseScale = transform.localScale;
        CapturePlayLayout();
        GetComponent<Button>().onClick.AddListener(OnClick);

        if (continueLabel == null)
            continueLabel = GetComponentInChildren<TextMeshProUGUI>(true);
        if (continueLabel != null)
        {
            continueLabel.text = continueText;
            continueLabel.raycastTarget = false;
        }

        RefreshContinueLabel();
    }

    private IEnumerator Start()
    {
        while (GameManager.Instance == null)
            yield return null;

        GameManager.Instance.OnGameOver -= OnGameOverDismissPauseUi;
        GameManager.Instance.OnGameOver += OnGameOverDismissPauseUi;

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= OnGameOverDismissPauseUi;
        transform.DOKill(true);
    }

    private void CapturePlayLayout()
    {
        playAnchorMin = rt.anchorMin;
        playAnchorMax = rt.anchorMax;
        playPivot = rt.pivot;
        playAnchoredPosition = rt.anchoredPosition;
        playSizeDelta = rt.sizeDelta;
    }

    private void RestorePlayLayout()
    {
        rt.anchorMin = playAnchorMin;
        rt.anchorMax = playAnchorMax;
        rt.pivot = playPivot;
        rt.anchoredPosition = playAnchoredPosition;
        rt.sizeDelta = playSizeDelta;
    }

    private void ApplyPausedLayout()
    {
        Vector2 half = Vector2.one * 0.5f;
        rt.anchorMin = half;
        rt.anchorMax = half;
        rt.pivot = half;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = pausedButtonSizeDelta;
    }

    private void OnGameOverDismissPauseUi()
    {
        if (!isPaused) return;

        transform.DOKill(true);
        transform.localScale = baseScale;
        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);
        RestorePlayLayout();
        if (image != null && pauseSprite != null)
            image.sprite = pauseSprite;
        isPaused = false;
        RefreshContinueLabel();
    }

    private void RefreshContinueLabel()
    {
        if (continueLabel == null) return;
        continueLabel.text = continueText;
        bool gmOver = GameManager.Instance != null && GameManager.Instance.IsGameOver;
        continueLabel.gameObject.SetActive(isPaused && !gmOver);
    }

    private void OnClick()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            if (pauseOverlay != null && pauseOverlay.activeSelf)
                pauseOverlay.SetActive(false);
            if (isPaused)
            {
                isPaused = false;
                RestorePlayLayout();
                if (image != null && pauseSprite != null)
                    image.sprite = pauseSprite;
            }
            transform.DOKill(true);
            transform.localScale = baseScale;
            RefreshContinueLabel();
            return;
        }

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (image != null)
            image.sprite = isPaused ? resumeSprite : pauseSprite;

        transform.DOKill(true);
        transform.localScale = baseScale;

        if (isPaused)
        {
            if (pauseOverlay != null)
                pauseOverlay.SetActive(true);
            ApplyPausedLayout();
            Sequence seq = DOTween.Sequence().SetUpdate(true);
            if (pausedButtonAppearScaleTween > 0f)
            {
                transform.localScale = baseScale * 0.92f;
                seq.Append(transform.DOScale(baseScale, pausedButtonAppearScaleTween).SetEase(Ease.OutBack));
            }
            seq.Append(transform.DOPunchScale(clickPunch, clickDuration, 6, 0.6f));
        }
        else
        {
            if (pauseOverlay != null)
                pauseOverlay.SetActive(false);
            RestorePlayLayout();
            transform.DOPunchScale(clickPunch, clickDuration, 6, 0.6f).SetUpdate(true);
        }

        RefreshContinueLabel();
    }
}
