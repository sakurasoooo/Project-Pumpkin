using UnityEngine;
using DG.Tweening;

/// <summary>
/// 气泡上层闪光：自旋 + 周期缩放脉动 + 两张闪光贴图交替闪烁
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BubbleSparkle : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed = 90f;

    [Header("Pulse")]
    public float pulseScale = 1.25f;
    public float pulseDuration = 0.8f;

    [Header("Sprite Flicker (交替闪烁)")]
    public Sprite spriteA;
    public Sprite spriteB;
    [Tooltip("两张贴图切换的间隔时间（秒）")]
    public float flickerInterval = 0.25f;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private float flickerTimer;
    private bool showA = true;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        baseScale = transform.localScale;
        transform.DOScale(baseScale * pulseScale, pulseDuration)
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine);

        if (spriteA != null) sr.sprite = spriteA;
        flickerTimer = flickerInterval;
    }

    void OnDisable()
    {
        transform.DOKill();
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        if (spriteA == null || spriteB == null) return;
        flickerTimer -= Time.deltaTime;
        if (flickerTimer <= 0f)
        {
            showA = !showA;
            sr.sprite = showA ? spriteA : spriteB;
            flickerTimer = flickerInterval;
        }
    }
}
