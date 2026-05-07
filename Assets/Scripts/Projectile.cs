using UnityEngine;
using DG.Tweening;

/// <summary>
/// 通用的子弹飞行与碰撞逻辑，支持单体伤害和范围爆炸伤害
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float moveSpeed = 10f;
    public float lifeTime = 3f;

    [Header("Explosion Settings (For Corn Cannon)")]
    public bool isExplosive = false;
    public float explosionRadius = 2.5f;

    [Header("Impact Feedback")]
    public float hitStopDuration = 0.04f;
    public float explosiveHitStopDuration = 0.08f;

    private void Start()
    {
        // TODO: 替换为对象池回收逻辑 (Return to Pool)
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (isExplosive)
            {
                Explode();
            }
            else
            {
                bool killed = HitTarget(collision.gameObject);
                if (killed && GameManager.Instance != null)
                    GameManager.Instance.HitStop(hitStopDuration);

                if (killed)
                {
                    // TODO: 替换为对象池回收
                    Destroy(gameObject);
                }
            }
        }
    }

    private bool HitTarget(GameObject target)
    {
        Vector2 dir = transform.right;
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            return enemy.TakeHit(dir);
        }
        else
        {
            // 兼容未挂 Enemy 脚本的占位敌人
            Destroy(target);
            return true;
        }
    }

    private void Explode()
    {
        bool damagedAny = false;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                if (HitTarget(hitCollider.gameObject))
                    damagedAny = true;
            }
        }

        if (damagedAny && GameManager.Instance != null)
            GameManager.Instance.HitStop(explosiveHitStopDuration);

        // TODO: 播放巨大的爆炸特效
        // TODO: 替换为对象池回收
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (isExplosive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
