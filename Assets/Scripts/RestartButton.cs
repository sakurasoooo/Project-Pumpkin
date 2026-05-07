using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 重新开始按钮（UGUI Canvas 版）：呼吸式缩放强提示 + 点击缩放反馈 + 重载场景
/// </summary>
[RequireComponent(typeof(Button))]
public class RestartButton : MonoBehaviour
{
    [Header("Idle Pulse")]
    public float pulseScale = 1.08f;
    public float pulseDuration = 0.7f;

    private Vector3 baseScale;
    private Tween pulseTween;

    void Awake()
    {
        baseScale = transform.localScale;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        transform.localScale = baseScale;
        pulseTween?.Kill();
        pulseTween = transform.DOScale(baseScale * pulseScale, pulseDuration)
                              .SetEase(Ease.InOutSine)
                              .SetLoops(-1, LoopType.Yoyo)
                              .SetUpdate(true);
    }

    void OnDisable()
    {
        pulseTween?.Kill();
        pulseTween = null;
        transform.localScale = baseScale;
    }

    private void OnClick()
    {
        pulseTween?.Kill();
        transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.2f, 6, 0.6f).SetUpdate(true);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
