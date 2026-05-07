using UnityEngine;
using DG.Tweening;

/// <summary>
/// 摄像机抖动：通过 DOShakePosition 提供命中震屏效果
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Defaults")]
    public float defaultDuration = 0.15f;
    public float defaultStrength = 0.25f;
    public int defaultVibrato = 14;
    public float defaultRandomness = 90f;

    private Vector3 originalLocalPos;

    void Awake()
    {
        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localPosition = originalLocalPos;
    }

    public static void Shake(float? duration = null, float? strength = null, bool ignoreTimeScale = false)
    {
        if (Instance == null) return;
        Instance.DoShake(duration ?? Instance.defaultDuration,
                         strength ?? Instance.defaultStrength,
                         ignoreTimeScale);
    }

    private void DoShake(float duration, float strength, bool ignoreTimeScale)
    {
        transform.DOComplete();
        transform.localPosition = originalLocalPos;
        var tween = transform.DOShakePosition(duration, strength, defaultVibrato, defaultRandomness, false, true)
                             .OnComplete(() => transform.localPosition = originalLocalPos);
        if (ignoreTimeScale)
        {
            tween.SetUpdate(true);
        }
    }
}
