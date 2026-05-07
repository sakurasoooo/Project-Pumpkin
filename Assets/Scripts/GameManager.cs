using UnityEngine;

/// <summary>
/// 控制游戏全局状态与全局速度的管理器
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [Tooltip("全局移动速度基准")]
    public float globalSpeed = 5f;

    public float GlobalSpeed => globalSpeed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
