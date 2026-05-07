using UnityEngine;

/// <summary>
/// 场景中的武器道具：浮动的气泡，玩家碰撞后切换武器并播放拾取特效
/// </summary>
public class WeaponItem : MonoBehaviour
{
    public PlayerShooter.WeaponType weaponType;
    public float duration = 10f;

    [Header("Bobbing")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.2f;

    [Header("Pickup VFX")]
    public GameObject pickupVFXPrefab;

    private float baseY;
    private float bobPhase;

    void Start()
    {
        baseY = transform.position.y;
        bobPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // 仅驱动 Y 轴浮动；X 轴由 WeaponItemSpawner 控制
        float bob = Mathf.Sin(Time.time * floatSpeed + bobPhase) * floatAmplitude;
        var p = transform.position;
        p.y = baseY + bob;
        transform.position = p;

        // 当生成器整体推动 baseY 改变时（实际上 X 轴推动并不影响 Y），保持鲁棒
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerShooter shooter = collision.GetComponent<PlayerShooter>();
            if (shooter != null)
            {
                shooter.ChangeWeapon(weaponType, duration);

                if (pickupVFXPrefab != null)
                {
                    Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}
