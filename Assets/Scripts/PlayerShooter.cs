using UnityEngine;
using System.Collections;

/// <summary>
/// 控制玩家的自动射击及武器状态切换（普通豌豆、豌豆机枪、玉米大炮）
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    public enum WeaponType
    {
        NormalPea,
        PeaGatling,
        CornCannon
    }

    [Header("Weapon Status")]
    public WeaponType currentWeapon = WeaponType.NormalPea;
    public Transform firePoint; // 子弹发射位置
    
    [Header("Projectile Prefabs")]
    public GameObject normalPeaPrefab;
    public GameObject gatlingPeaPrefab;
    public GameObject cornCannonPrefab;

    [Header("Fire Rates (Seconds)")]
    public float normalFireRate = 1.0f;
    public float gatlingFireRate = 0.15f; // 机枪高射速
    public float cornFireRate = 2.0f;     // 大炮低射速

    private float fireTimer = 0f;

    void Update()
    {
        fireTimer += Time.deltaTime;
        float currentFireRate = GetCurrentFireRate();

        if (fireTimer >= currentFireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    private float GetCurrentFireRate()
    {
        switch (currentWeapon)
        {
            case WeaponType.PeaGatling: return gatlingFireRate;
            case WeaponType.CornCannon: return cornFireRate;
            default: return normalFireRate;
        }
    }

    private void Shoot()
    {
        GameObject prefabToFire = GetCurrentPrefab();
        if (prefabToFire != null && firePoint != null)
        {
            // TODO: 替换为从对象池 (ObjectPool) 获取子弹以优化性能
            Instantiate(prefabToFire, firePoint.position, firePoint.rotation);
            
            // TODO: 播放开火音效与开火特效 (Muzzle Flash)
            // TODO: 结合 Feel 插件触发开火轻微震屏
        }
    }

    private GameObject GetCurrentPrefab()
    {
        switch (currentWeapon)
        {
            case WeaponType.PeaGatling: return gatlingPeaPrefab != null ? gatlingPeaPrefab : normalPeaPrefab;
            case WeaponType.CornCannon: return cornCannonPrefab != null ? cornCannonPrefab : normalPeaPrefab;
            default: return normalPeaPrefab;
        }
    }

    /// <summary>
    /// 拾取道具后调用此方法切换武器
    /// </summary>
    /// <param name="newWeapon">新武器类型</param>
    /// <param name="duration">持续时间</param>
    public void ChangeWeapon(WeaponType newWeapon, float duration = 10f)
    {
        StopAllCoroutines(); // 打断上一个武器的计时器
        currentWeapon = newWeapon;
        
        // TODO: 切换嘴部外观美术资源 (普通嘴 -> 机枪嘴 -> 玉米嘴)

        StartCoroutine(WeaponTimerRoutine(duration));
    }

    private IEnumerator WeaponTimerRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        currentWeapon = WeaponType.NormalPea; // 持续时间到，恢复普通豌豆
        
        // TODO: 恢复嘴部外观美术资源
    }
}
