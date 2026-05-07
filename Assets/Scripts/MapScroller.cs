using UnityEngine;

/// <summary>
/// 基于统一偏移量的视差滚动系统（绝对无缝版）
/// 修复了浮点数累积误差导致的“两张图之间出现速度差和缝隙”的现象。
/// </summary>
public class MapScroller : MonoBehaviour
{
    [Header("滚动配置")]
    [Tooltip("视差系数：草地与泥土设为 1，远景设为 0.3~0.5")]
    public float parallaxFactor = 1f;
    
    [Tooltip("单张图片的像素宽度（需与缩放比例对应，默认为世界坐标宽度）")]
    public float spriteWidth = 13.86f;

    [Tooltip("消除接缝微调（可选）：如果由于抗锯齿边缘仍有一丝缝隙，可设为 0.01~0.05，让图片轻微重叠")]
    public float overlapFix = 0.01f;

    [Header("自动获取配置")]
    [Tooltip("如果勾选，启动时会自动将所有子节点作为滚动图层")]
    public bool autoGetChildren = true;
    
    [Tooltip("手动指定的图层集合（仅在 autoGetChildren 为 false 时生效）")]
    public Transform[] backgrounds;

    // 单一全局偏移量，杜绝各个图片各自 += deltaTime 导致的浮点数运算漂移
    private float currentScrollOffset = 0f;

    void Awake()
    {
        if (autoGetChildren)
        {
            int childCount = transform.childCount;
            backgrounds = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                backgrounds[i] = transform.GetChild(i);
            }
        }

        // 自动对配置的图层进行克隆翻倍，确保宽屏覆盖
        if (backgrounds != null && backgrounds.Length > 0)
        {
            int originalLength = backgrounds.Length;
            Transform[] expandedBackgrounds = new Transform[originalLength * 2];
            
            // 真实排布宽度，扣除微小的重叠修复
            float actualWidth = spriteWidth - overlapFix;

            for (int i = 0; i < originalLength; i++)
            {
                Transform original = backgrounds[i];
                expandedBackgrounds[i] = original;
                
                // 动态实例化克隆体并向后排布
                Transform clone = Instantiate(original, original.parent);
                clone.name = original.name + "_Clone";
                
                Vector3 pos = original.localPosition;
                // 初始排布就保证微小的交错重叠
                pos.x += actualWidth * originalLength;
                clone.localPosition = pos;
                
                expandedBackgrounds[i + originalLength] = clone;
            }
            backgrounds = expandedBackgrounds;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || backgrounds == null || backgrounds.Length == 0) return;

        float currentSpeed = GameManager.Instance.GlobalSpeed * parallaxFactor;
        
        // 1. 统一累加全局偏移量
        currentScrollOffset += currentSpeed * Time.deltaTime;
        
        float actualWidth = spriteWidth - overlapFix;
        float totalWidth = actualWidth * backgrounds.Length;

        // 防止偏移量无限增大导致 float 精度丢失
        if (currentScrollOffset >= totalWidth)
        {
            currentScrollOffset %= totalWidth;
        }
        else if (currentScrollOffset < 0)
        {
            currentScrollOffset = (currentScrollOffset % totalWidth) + totalWidth;
        }

        // 2. 根据全局偏移量，绝对精准地重算每一张图片的位置
        for (int i = 0; i < backgrounds.Length; i++)
        {
            var bg = backgrounds[i];
            
            // 按照序号应该所在的初始数学位置
            float initialX = i * actualWidth;
            
            // 减去滚动偏移
            float currentX = initialX - currentScrollOffset;
            
            // 循环逻辑：如果向左移出了阈值，就加上总宽度传送到右边
            while (currentX <= -actualWidth * 2f)
            {
                currentX += totalWidth;
            }
            while (currentX > totalWidth - actualWidth * 2f)
            {
                currentX -= totalWidth;
            }
            
            bg.localPosition = new Vector3(currentX, bg.localPosition.y, bg.localPosition.z);
        }
    }
}

