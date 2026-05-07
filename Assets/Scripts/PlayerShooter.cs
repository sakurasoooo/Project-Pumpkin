using UnityEngine;
using System.Collections;
using DG.Tweening;
using MoreMountains.Feedbacks;

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

    [Header("Mouth Visuals")]
    public SpriteRenderer mouthRenderer; // 嘴部的 SpriteRenderer
    public Sprite normalMouthSprite;
    public Sprite gatlingMouthSprite;
    public Sprite cornMouthSprite;

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

    [Header("Feedback")]
    public MMF_Player shootFeedbacks;
    public Vector3 muzzlePunch = new Vector3(0.3f, 0.1f, 0f);
    public float muzzleDuration = 0.15f;

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
            Debug.Log($"Shooting {currentWeapon} from {firePoint.position}");
            GameObject projectileInstance = Instantiate(prefabToFire, firePoint.position, firePoint.rotation);
            projectileInstance.SetActive(true);
            Debug.Log($"Instantiated {projectileInstance.name}, Active: {projectileInstance.activeSelf}");

            firePoint.DOComplete();
            firePoint.DOPunchScale(muzzlePunch, muzzleDuration, 4, 0.5f);
            if (shootFeedbacks != null) shootFeedbacks.PlayFeedbacks();
        }
        else
        {
            Debug.LogWarning("Cannot shoot: Prefab or FirePoint is null!");
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
        
        UpdateMouthVisual();

        StartCoroutine(WeaponTimerRoutine(duration));
    }

    private void UpdateMouthVisual()
    {
        if (mouthRenderer == null) return;

        switch (currentWeapon)
        {
            case WeaponType.PeaGatling:
                mouthRenderer.sprite = gatlingMouthSprite;
                break;
            case WeaponType.CornCannon:
                mouthRenderer.sprite = cornMouthSprite;
                break;
            default:
                mouthRenderer.sprite = normalMouthSprite;
                break;
        }
    }

    private IEnumerator WeaponTimerRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        currentWeapon = WeaponType.NormalPea; // 持续时间到，恢复普通豌豆
        UpdateMouthVisual();
    }
}
