using UnityEngine;

/// <summary>
/// 敌人 Idle 动画：周期 Y 轴浮动 + 轻微扭头（旋转），让敌人看起来活着。
/// 注意：仅修改 SpriteRenderer 所在的 transform，不会影响 Spawner 的水平滚动（SpawNer 写 transform.position.x，这里写 localPosition.y）。
/// 实际放在敌人 prefab 的根节点上即可，因为根节点的 X 由 Spawner 写 world position；本脚本只读取并叠加偏移。
/// </summary>
public class EnemyIdle : MonoBehaviour
{
    [Header("Bob")]
    public float bobAmplitude = 0.15f;
    public float bobSpeed = 2f;

    [Header("Tilt")]
    public float tiltAngle = 6f;
    public float tiltSpeed = 1.5f;

    private float baseY;
    private float phase;
    private bool baseCaptured;

    void Start()
    {
        baseY = transform.position.y;
        phase = Random.Range(0f, Mathf.PI * 2f);
        baseCaptured = true;
    }

    void LateUpdate()
    {
        if (!baseCaptured) return;

        float t = Time.time;
        float bob = Mathf.Sin(t * bobSpeed + phase) * bobAmplitude;
        var p = transform.position;
        p.y = baseY + bob;
        transform.position = p;

        float angle = Mathf.Sin(t * tiltSpeed + phase) * tiltAngle;
        var e = transform.localEulerAngles;
        e.z = angle;
        transform.localEulerAngles = e;
    }
}
