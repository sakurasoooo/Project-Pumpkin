using UnityEngine;

/// <summary>
/// 游戏结束面板：监听 GameManager.OnGameOver，激活 root 子树
/// </summary>
public class GameOverPanel : MonoBehaviour
{
    [Tooltip("结束面板根物体（默认 inactive，游戏结束时激活）")]
    public GameObject root;

    void Start()
    {
        if (root != null) root.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += Show;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= Show;
    }

    private void Show()
    {
        if (root != null) root.SetActive(true);
    }
}
