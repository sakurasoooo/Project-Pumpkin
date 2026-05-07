using UnityEngine;

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

    private void Start()
    {
        // TODO: 替换为对象池回收逻辑 (Return to Pool)
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 子弹匀速向右飞行
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
                // 普通子弹：命中单体
                HitTarget(collision.gameObject);
                
                // TODO: 替换为对象池回收
                Destroy(gameObject);
            }
        }
    }

    private void HitTarget(GameObject target)
    {
        // TODO: 调用 Enemy 的受击扣血/死亡方法，并通知 GameManager 增加分数
        Destroy(target); // 临时直接销毁敌人
        
        // TODO: 播放命中粒子特效
    }

    private void Explode()
    {
        // 寻找爆炸范围内的所有敌人
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                HitTarget(hitCollider.gameObject);
            }
        }
        
        // TODO: 播放巨大的爆炸特效与震屏
        
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
