using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 重新开始按钮：OnMouseDown 不受 Time.timeScale=0 影响
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RestartButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
