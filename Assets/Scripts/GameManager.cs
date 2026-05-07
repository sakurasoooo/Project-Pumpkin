using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 控制游戏全局状态、全局速度、分数与命中卡顿(HitStop)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [Tooltip("全局移动速度基准")]
    public float globalSpeed = 5f;

    public float GlobalSpeed => globalSpeed;

    [Header("Score")]
    public int score;
    public event Action<int> OnScoreChanged;

    [Header("Game Over")]
    public bool IsGameOver { get; private set; }
    public event Action OnGameOver;

    private Coroutine hitStopRoutine;

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

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }

    public void HitStop(float duration = 0.05f)
    {
        if (duration <= 0f) return;
        if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
        hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float prev = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = prev > 0f ? prev : 1f;
        hitStopRoutine = null;
    }
}
