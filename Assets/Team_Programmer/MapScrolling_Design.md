# 无限循环滚动设计方案 (草地、泥土、背景)

为了实现无限跑酷游戏的核心视觉效果，我们需要设计一套支持多图层视差滚动（Parallax Scrolling）的系统。针对您提出的“草地层”、“泥土层”和“背景层”，本方案采用 **双图交替补位法**。

## 1. 整体视觉分层与速度设计

跑酷游戏通过让场景元素向左移动来模拟玩家的向右奔跑。不同图层的移动速度应存在差异，从而产生纵深感。

| 图层名称 (Layer) | 视觉层级 | 移动速度比例 (相对于基准速度) | 元素内容 |
| :--- | :--- | :--- | :--- |
| **草地层 (Grass)** | 前景 / 交互层 | **1.0x** (100% 速度) | 玩家奔跑的表面，速度基准 |
| **泥土层 (Dirt)** | 前景 / 装饰层 | **1.0x** (100% 速度) | 草地下方的截面，与草地必须保持同步移动 |
| **背景层 (Background)** | 远景 | **0.3x - 0.5x** (较慢速度) | 远处的树木、天空或山脉，移动缓慢以营造距离感 |

## 2. 核心技术实现：双图交替补位法

为了实现无限循环，不需要在场景中生成无数张图片。对于每一层，我们只需要 **两张相同的 Sprite 图片** 拼接在一起即可。

### 运行机制
1. **初始状态**：将两张一样的图片（A 和 B）首尾相接拼好。A 位于屏幕正中，B 位于 A 的右侧紧挨着。
2. **向左移动**：在 `Update` 中，根据 GameManager 提供的全局速度和该图层的速度比例，让 A 和 B 同时向左匀速移动。
3. **补位重置**：当图片 A 完全移出屏幕左边缘时（根据图片的宽度计算），将其坐标瞬间传送到图片 B 的右侧紧挨着。
4. **循环往复**：图片 B 移出屏幕时，传送到 A 的右侧。如此交替，实现视觉上的无限延伸。

### 脚本架构 (`MapScroller.cs`)

我们将编写一个通用的挂载脚本，挂载到各个图层的父节点上：

```csharp
public class MapScroller : MonoBehaviour
{
    [Header("滚动配置")]
    public float parallaxFactor = 1f; // 视差系数（如背景设为0.5，草地设为1）
    public float spriteWidth;         // 单张切图的宽度
    
    private Transform[] backgrounds;  // 存放层内的A、B两张图

    void Update()
    {
        float currentSpeed = GameManager.Instance.GlobalSpeed * parallaxFactor;
        
        // 移动所有子节点
        foreach (var bg in backgrounds)
        {
            bg.Translate(Vector3.left * currentSpeed * Time.deltaTime);
            
            // 检查是否移出屏幕左侧（假设相机在原点，移出距离超过了宽度）
            if (bg.position.x <= -spriteWidth)
            {
                // 传送到最右侧补位
                Vector3 newPos = bg.position;
                newPos.x += spriteWidth * backgrounds.Length; 
                bg.position = newPos;
            }
        }
    }
}
```

## 3. 美术素材规范 (Artist)

为了配合上述程序实现，美术部门在制作草地、泥土和背景素材时需要遵循以下规则：
- **首尾无缝拼接**：每一层的单张 Sprite，其最左侧的边缘图案必须能够与最右侧的边缘图案完美拼合，否则在双图交替时会出现明显的接缝或断层。
- **宽度统一**：同一层的图片 A 和 B 尺寸必须完全一致。建议宽度略大于屏幕宽度（例如 1920x1080 屏幕，图片宽度建议至少为 2048），防止边缘穿帮。
- **贴图设置**：如果使用材质偏移（Texture Offset）方案，则需要在 Unity 中将图片的 Wrap Mode 设置为 `Repeat`。但本方案使用 Transform 移动，因此只需保证切图质量即可。

## 4. 实施计划

1. **美术阶段**：优先交付一张草地+泥土的无缝拼接图（可先合并为一张图，或分开交付），以及一张背景无缝拼接图。
2. **程序阶段**：在 `MainScene` 搭建测试环境，编写 `MapScroller.cs`，应用上述交替逻辑。
3. **策划阶段**：微调 `parallaxFactor` 变量，直到远近景的滚动速度看起来最舒服。
