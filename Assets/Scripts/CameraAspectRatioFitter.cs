using UnityEngine;

/// <summary>
/// 动态摄像机视口适配器（选项B实现）
/// 确保在任何屏幕比例下，核心游戏区域（含角色、重要物体）绝对不会被水平裁切。
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraAspectRatioFitter : MonoBehaviour
{
    [Header("设计基准分辨率 (Design Resolution)")]
    [Tooltip("游戏核心玩法和UI设计的基准宽度")]
    public float targetWidth = 1920f;
    [Tooltip("游戏核心玩法和UI设计的基准高度")]
    public float targetHeight = 1080f;

    [Header("基准正交高度 (Base Orthographic Size)")]
    [Tooltip("在基准分辨率下的 Camera Orthographic Size")]
    public float baseOrthographicSize = 5f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        AdjustCameraSize();
    }

#if UNITY_EDITOR
    void Update()
    {
        // 在编辑器下实时预览，发布时可移除或仅在屏幕尺寸变化时调用
        AdjustCameraSize();
    }
#endif

    private void AdjustCameraSize()
    {
        if (cam == null) return;

        // 计算设计宽高比 (例如 16:9 = 1.777)
        float targetAspect = targetWidth / targetHeight;
        
        // 计算当前设备实际宽高比
        float currentAspect = (float)Screen.width / (float)Screen.height;

        // 如果当前设备比设计的更宽 (例如带鱼屏 21:9)
        if (currentAspect >= targetAspect)
        {
            // 维持基础高度不变，水平视野变宽。
            // 场景中的角色和物体高度依然完美贴合设计高度，但左右能看到更多的背景。
            cam.orthographicSize = baseOrthographicSize;
        }
        else
        {
            // 如果当前设备比设计的更窄/更方 (例如 iPad 4:3，或者竖屏设备)
            // 强行增加 Orthographic Size（即拉远摄像机距离）。
            // 这样能确保横向的内容（场景宽度内的角色和物体）全部缩放进屏幕，不会被切掉。
            float differenceInSize = targetAspect / currentAspect;
            cam.orthographicSize = baseOrthographicSize * differenceInSize;
        }
    }
}
